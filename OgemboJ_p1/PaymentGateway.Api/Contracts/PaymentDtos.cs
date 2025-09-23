namespace PaymentGateway.Api.Contracts;

public record CreatePaymentDto(
    Guid MerchantId,
    Guid CustomerId,
    int PaymentMethodId,
    decimal Amount,
    string Currency = "USD"
);

public record PaymentResponseDto(
    Guid Id,
    Guid MerchantId,
    Guid CustomerId,
    int PaymentMethodId,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt
);
