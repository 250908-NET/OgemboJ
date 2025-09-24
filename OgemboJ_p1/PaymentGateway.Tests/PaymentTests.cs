using Microsoft.EntityFrameworkCore;
using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Data;
using PaymentGateway.Api.Domain;
using FluentAssertions;




namespace PaymentGateway.Tests;

public class PaymentTests
{
    private AppDbContext InMem()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(opts);

        // Seed
        var m = new Merchant { Name = "Test Merchant", ApiKey = "key" };
        var c = new Customer { Email = "t@example.com", FullName = "Test User" };
        var pm1 = new PaymentMethod { Id = 1, Code = "CARD", DisplayName = "Card" };
        db.Merchants.Add(m);
        db.Customers.Add(c);
        db.PaymentMethods.Add(pm1);
        db.MerchantPaymentMethods.Add(new MerchantPaymentMethod { Merchant = m, PaymentMethod = pm1 });
        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task CreatePayment_Should_Persist_WithAuthorizedStatus()
    {
        var db = InMem();
        var merchant = await db.Merchants.FirstAsync();
        var customer = await db.Customers.FirstAsync();

        var dto = new CreatePaymentDto(merchant.Id, customer.Id, 1, 49.99m, "USD");

        var payment = new Payment
        {
            MerchantId = dto.MerchantId,
            CustomerId = dto.CustomerId,
            PaymentMethodId = dto.PaymentMethodId,
            Amount = dto.Amount,
            Currency = dto.Currency,
            Status = PaymentStatus.Authorized
        };

        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        var saved = await db.Payments.FindAsync(payment.Id);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(PaymentStatus.Authorized);
        saved.Amount.Should().Be(49.99m);
    }

    [Fact]
    public async Task Merchant_Must_Support_Method()
    {
        var db = InMem();
        var merchant = await db.Merchants.FirstAsync();
        var customer = await db.Customers.FirstAsync();

    
        db.PaymentMethods.Add(new PaymentMethod { Id = 2, Code = "MPESA", DisplayName = "M-Pesa" });
        await db.SaveChangesAsync();

        var supports = await db.MerchantPaymentMethods
            .AnyAsync(x => x.MerchantId == merchant.Id && x.PaymentMethodId == 2);

        supports.Should().BeFalse();
    }
}
