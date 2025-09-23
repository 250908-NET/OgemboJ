namespace PaymentGateway.Api.Domain;

public enum PaymentStatus { Pending, Authorized, Captured, Refunded, Failed }

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; } = default!;

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;

    public int PaymentMethodId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = default!;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // in Payment.cs
    public DateTime? RefundedAt { get; set; }
    public bool IsDeleted { get; set; } 

}
