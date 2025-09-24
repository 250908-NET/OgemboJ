using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Api.Data;
using PaymentGateway.Api.Domain;

public class RefundIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RefundIntegrationTests(WebApplicationFactory<Program> factory)
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
    public async Task Delete_Should_RefundPayment_And_SoftDelete()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var m = new Merchant { Name = "Acme", ApiKey = "key" };
        var c = new Customer { Email = "alice@example.com", FullName = "Alice" };
        var pm = new PaymentMethod { Code = "CARD", DisplayName = "Card" };
        db.AddRange(m, c, pm);
        db.MerchantPaymentMethods.Add(new MerchantPaymentMethod { Merchant = m, PaymentMethod = pm });

        var payment = new Payment
        {
            Merchant = m,
            Customer = c,
            PaymentMethod = pm,
            Amount = 49.99m,
            Currency = "USD",
            Status = PaymentStatus.Authorized
        };
        db.Payments.Add(payment);
        db.SaveChanges();

        var client = _factory.CreateClient();

        // Act
        var resp = await client.DeleteAsync($"/merchants/{m.Id}/payments/{payment.Id}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var updated = verifyDb.Payments.First(p => p.Id == payment.Id);
        updated.Status.Should().Be(PaymentStatus.Refunded);

    }


    [Fact]
    public async Task Delete_With_WrongMerchant_Should_Return_NotFound()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var m = new Merchant { Name = "Acme", ApiKey = "key" };
        var c = new Customer { Email = "alice@example.com", FullName = "Alice" };
        var pm = new PaymentMethod { Code = "CARD", DisplayName = "Card" };
        db.AddRange(m, c, pm);
        db.MerchantPaymentMethods.Add(new MerchantPaymentMethod { Merchant = m, PaymentMethod = pm });

        var payment = new Payment
        {
            Merchant = m,
            Customer = c,
            PaymentMethod = pm,
            Amount = 12.34m,
            Currency = "USD",
            Status = PaymentStatus.Authorized
        };
        db.Payments.Add(payment);
        db.SaveChanges();

        var client = _factory.CreateClient();
        var wrongMerchantId = Guid.NewGuid();

        var resp = await client.DeleteAsync($"/merchants/{wrongMerchantId}/payments/{payment.Id}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_FailedPayment_Should_Return_BadRequest()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var m = new Merchant { Name = "Acme", ApiKey = "key" };
        var c = new Customer { Email = "alice@example.com", FullName = "Alice" };
        var pm = new PaymentMethod { Code = "CARD", DisplayName = "Card" };
        db.AddRange(m, c, pm);
        db.MerchantPaymentMethods.Add(new MerchantPaymentMethod { Merchant = m, PaymentMethod = pm });
        db.SaveChanges();

        var failed = new Payment
        {
            MerchantId = m.Id,
            CustomerId = c.Id,
            PaymentMethodId = pm.Id,
            Amount = 10m,
            Currency = "USD",
            Status = PaymentStatus.Failed
        };
        db.Payments.Add(failed);
        db.SaveChanges();

        var client = _factory.CreateClient();

        var resp = await client.DeleteAsync($"/merchants/{m.Id}/payments/{failed.Id}");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
