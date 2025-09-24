using System.Linq;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Data;
using PaymentGateway.Api.Domain;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDb");
    if (useInMemory)
    {
        var dbName = builder.Configuration.GetValue<string>("InMemoryDbName") ?? "PaymentGatewayTests";
        opt.UseInMemoryDatabase(dbName);
    }
    else
    {
        opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
    }
});



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Health 
app.MapGet("/", () => Results.Ok(new { ok = true, service = "PaymentGateway.Api" }));

// Create Customers (simple demo create using entity)
app.MapPost("/customers", async (Customer customer, AppDbContext db) =>
{
    customer.Id = Guid.NewGuid();
    db.Customers.Add(customer);
    await db.SaveChangesAsync();
    return Results.Created($"/customers/{customer.Id}", customer);
});

// Update customer info
app.MapPut("/customers/{id:guid}", async (Guid id, UpdateCustomerDto dto, AppDbContext db) =>
{
    var customer = await db.Customers.FindAsync(id);
    if (customer is null) return Results.NotFound(new { error = "Customer not found" });

    customer.Email = dto.Email;
    customer.FullName = dto.FullName;

    await db.SaveChangesAsync();
    return Results.Ok(new { updated = true, id = customer.Id, customer.Email, customer.FullName });
});

// Customers (list)
app.MapGet("/customers", async (AppDbContext db) =>
    await db.Customers
        .Select(c => new { c.Id, c.FullName, c.Email })
        .ToListAsync());

// Merchants (list)
app.MapGet("/merchants", async (AppDbContext db) =>
    await db.Merchants
        .Select(m => new { m.Id, m.Name })
        .ToListAsync());

// Create Payments
app.MapPost("/payments", async (CreatePaymentDto dto, AppDbContext db) =>
{
    var merchant = await db.Merchants.FindAsync(dto.MerchantId);
    var customer = await db.Customers.FindAsync(dto.CustomerId);
    var method = await db.PaymentMethods.FindAsync(dto.PaymentMethodId);
    if (merchant is null || customer is null || method is null)
        return Results.BadRequest(new { error = "Invalid merchant/customer/paymentMethod." });

    // Check m-m support
    var supportsMethod = await db.MerchantPaymentMethods
        .AnyAsync(x => x.MerchantId == merchant.Id && x.PaymentMethodId == method.Id);
    if (!supportsMethod)
        return Results.BadRequest(new { error = "Merchant does not support this payment method." });

    var payment = new Payment
    {
        MerchantId = merchant.Id,
        CustomerId = customer.Id,
        PaymentMethodId = method.Id,
        Amount = dto.Amount,
        Currency = dto.Currency,
        Status = PaymentStatus.Authorized
    };

    db.Payments.Add(payment);
    await db.SaveChangesAsync();

    var response = new PaymentResponseDto(
        payment.Id, payment.MerchantId, payment.CustomerId, payment.PaymentMethodId,
        payment.Amount, payment.Currency, payment.Status.ToString(), payment.CreatedAt);

    return Results.Created($"/payments/{payment.Id}", response);
});

//  GET by id 
app.MapGet("/payments/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var p = await db.Payments
        .Where(x => x.Id == id && !x.IsDeleted)
        .Select(x => new PaymentResponseDto(
            x.Id, x.MerchantId, x.CustomerId, x.PaymentMethodId,
            x.Amount, x.Currency, x.Status.ToString(), x.CreatedAt))
        .SingleOrDefaultAsync();

    return p is null ? Results.NotFound() : Results.Ok(p);
});

// GET all for a merchant 
app.MapGet("/merchants/{merchantId:guid}/payments", async (Guid merchantId, AppDbContext db) =>
{
    var merchant = await db.Merchants.FindAsync(merchantId);
    if (merchant is null) return Results.NotFound(new { error = "Merchant not found" });

    var payments = await db.Payments
        .Where(p => p.MerchantId == merchantId && !p.IsDeleted)
        .Select(p => new PaymentResponseDto(
            p.Id, p.MerchantId, p.CustomerId, p.PaymentMethodId,
            p.Amount, p.Currency, p.Status.ToString(), p.CreatedAt))
        .ToListAsync();

    return Results.Ok(payments);
});

// Hard delete 
app.MapDelete("/payments/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var p = await db.Payments.FindAsync(id);
    if (p is null) return Results.NotFound();

    if (p.Status is PaymentStatus.Captured or PaymentStatus.Authorized)
        p.Status = PaymentStatus.Refunded;

    db.Payments.Remove(p);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Merchant initiated refund 
app.MapDelete("/merchants/{merchantId:guid}/payments/{id:guid}", async (Guid merchantId, Guid id, AppDbContext db) =>
{
    var payment = await db.Payments.FindAsync(id);
    if (payment is null || payment.MerchantId != merchantId) return Results.NotFound();

    if (payment.Status is PaymentStatus.Refunded) // idempotent
        return Results.Ok(new { refunded = true, id = payment.Id, alreadyRefunded = true });

    if (payment.Status is PaymentStatus.Failed)
        return Results.BadRequest(new { error = "Cannot refund a failed payment." });

    payment.Status = PaymentStatus.Refunded;
    payment.RefundedAt = DateTime.UtcNow;
    payment.IsDeleted = true;
    await db.SaveChangesAsync();

    return Results.Ok(new { refunded = true, id = payment.Id, refundedAt = payment.RefundedAt });
});

// Payment list
app.MapGet("/paymentmethods", async (AppDbContext db) =>
    await db.PaymentMethods
        .Select(pm => new { pm.Id, pm.Code, pm.DisplayName })
        .ToListAsync());

// Merchant's supported payment methods
app.MapGet("/merchants/{id:guid}/paymentmethods", async (Guid id, AppDbContext db) =>
{
    var merchant = await db.Merchants
        .Include(m => m.MerchantPaymentMethods)
        .ThenInclude(mpm => mpm.PaymentMethod)
        .FirstOrDefaultAsync(m => m.Id == id);

    if (merchant is null) return Results.NotFound();

    var supportedMethods = merchant.MerchantPaymentMethods
        .Select(mpm => new { mpm.PaymentMethod.Id, mpm.PaymentMethod.Code, mpm.PaymentMethod.DisplayName });

    return Results.Ok(supportedMethods);
});

// Seed helper
app.MapPost("/dev/seed", async (AppDbContext db) =>
{
    if (!await db.Merchants.AnyAsync())
    {
        var m1 = new Merchant { Name = "Acme Inc", ApiKey = "acme-key" };
        var m2 = new Merchant { Name = "Globex", ApiKey = "globex-key" };
        db.Merchants.AddRange(m1, m2);

        var c1 = new Customer { Email = "alice@example.com", FullName = "Alice Jones" };
        var c2 = new Customer { Email = "bob@example.com", FullName = "Bob Smith" };
        db.Customers.AddRange(c1, c2);

        db.MerchantPaymentMethods.AddRange(
            new MerchantPaymentMethod { Merchant = m1, PaymentMethodId = 1 },
            new MerchantPaymentMethod { Merchant = m1, PaymentMethodId = 2 },
            new MerchantPaymentMethod { Merchant = m2, PaymentMethodId = 1 }
        );

        await db.SaveChangesAsync();
    }
    return Results.Ok(new { seeded = true });
});

app.Run();

// For integration testing
public partial class Program { }
