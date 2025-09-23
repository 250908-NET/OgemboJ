namespace MinimalApi.DTOs;

public record CourseCreateDto(string Title, string Code, int Credits = 3);
public record CourseReadDto(int Id, string Title, string Code, int Credits);
