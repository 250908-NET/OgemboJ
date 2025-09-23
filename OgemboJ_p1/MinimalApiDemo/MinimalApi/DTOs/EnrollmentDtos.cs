namespace MinimalApi.DTOs;

public record EnrollmentCreateDto(int StudentId, int CourseId, DateTime? EnrolledOn = null, string? Grade = null);
public record EnrollmentReadDto(int StudentId, int CourseId, DateTime EnrolledOn, string? Grade);
