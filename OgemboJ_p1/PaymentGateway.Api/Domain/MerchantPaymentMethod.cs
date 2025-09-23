namespace PaymentGateway.Api.Domain;

public class MerchantPaymentMethod
{
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; } = default!;

    public int PaymentMethodId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = default!;
}
