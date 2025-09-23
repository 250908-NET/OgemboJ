namespace PaymentGateway.Api.Domain;

public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
