using System.ComponentModel.DataAnnotations;

namespace MinimalApi.Models;

public class Course
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Code { get; set; } = string.Empty; // e.g., CSC101

    public int Credits { get; set; } = 3;

    // Navigation
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
