using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Api.Data;
using PaymentGateway.Api.Domain;

public class HardDeleteIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HardDeleteIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((ctx, cfg) =>
            {
                cfg.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["UseInMemoryDb"] = "true",
                    ["InMemoryDbName"] = "PaymentGatewayTests" 
                });
            });
        });
    }

    [Fact]
    public async Task DeletePayments_ById_Should_RemoveRow()
    {
    
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var m  = new Merchant { Name = "Globex", ApiKey = "gkey" };
        var c  = new Customer { Email = "bob@example.com", FullName = "Bob" };
        var pm = new PaymentMethod { Code = "CARD", DisplayName = "Card" };
        db.AddRange(m, c, pm);
        db.MerchantPaymentMethods.Add(new MerchantPaymentMethod { Merchant = m, PaymentMethod = pm });

        var p = new Payment
        {
            Merchant = m, Customer = c, PaymentMethod = pm,
            Amount = 20m, Currency = "USD", Status = PaymentStatus.Authorized
        };
        db.Payments.Add(p);
        db.SaveChanges();

        var client = _factory.CreateClient();

        // Act
        var resp = await client.DeleteAsync($"/payments/{p.Id}");
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        db.Payments.Any(x => x.Id == p.Id).Should().BeFalse();
    }
}
