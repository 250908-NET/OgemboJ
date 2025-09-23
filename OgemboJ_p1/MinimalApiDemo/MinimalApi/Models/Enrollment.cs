using System.ComponentModel.DataAnnotations;

namespace MinimalApi.Models;

public class Enrollment
{
    public int StudentId { get; set; }
    public int CourseId  { get; set; }

    public DateTime EnrolledOn { get; set; } = DateTime.UtcNow;

    [MaxLength(10)]
    public string? Grade { get; set; } // optional (e.g., A, B+, etc.)

    // Navigation
    public Student? Student { get; set; }
    public Course?  Course  { get; set; }
}
