namespace PaymentGateway.Api.Domain;

public class Merchant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public string ApiKey { get; set; } = default!;  // basic auth for demo

    public ICollection<MerchantPaymentMethod> MerchantPaymentMethods { get; set; } = new List<MerchantPaymentMethod>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
