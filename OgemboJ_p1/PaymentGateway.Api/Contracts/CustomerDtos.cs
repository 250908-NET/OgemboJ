namespace PaymentGateway.Api.Contracts;

public record UpdateCustomerDto(
    string Email,
    string FullName
);
