namespace PaymentGateway.Api.Domain;

public class PaymentMethod
{
    public int Id { get; set; } // seed small ints (1=Card, 2=Mpesa, etc.)
    public string Code { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public ICollection<MerchantPaymentMethod> MerchantPaymentMethods { get; set; } = new List<MerchantPaymentMethod>();
}
