using Microsoft.EntityFrameworkCore;
using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<MerchantPaymentMethod> MerchantPaymentMethods => Set<MerchantPaymentMethod>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MerchantPaymentMethod>()
            .HasKey(x => new { x.MerchantId, x.PaymentMethodId });

        modelBuilder.Entity<MerchantPaymentMethod>()
            .HasOne(x => x.Merchant)
            .WithMany(m => m.MerchantPaymentMethods)
            .HasForeignKey(x => x.MerchantId);

        modelBuilder.Entity<MerchantPaymentMethod>()
            .HasOne(x => x.PaymentMethod)
            .WithMany(p => p.MerchantPaymentMethods)
            .HasForeignKey(x => x.PaymentMethodId);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount).HasColumnType("decimal(18,2)");

        // Seed a couple PaymentMethods
        modelBuilder.Entity<PaymentMethod>().HasData(
            new PaymentMethod { Id = 1, Code = "CARD",  DisplayName = "Card" },
            new PaymentMethod { Id = 2, Code = "MPESA", DisplayName = "M-Pesa" }
        );
    }
}
