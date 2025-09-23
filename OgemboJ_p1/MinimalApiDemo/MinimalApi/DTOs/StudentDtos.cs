namespace MinimalApi.DTOs;

public record StudentCreateDto(string FullName, string? Email);
public record StudentReadDto(int Id, string FullName, string? Email, DateTime CreatedAt);
