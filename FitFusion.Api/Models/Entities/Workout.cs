using System.ComponentModel.DataAnnotations;

namespace FitFusion.Api.Models.Entities;

public class Workout
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string OwnerUid { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int DurationMin { get; set; }
    public int? KcalEstimated { get; set; }
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<WorkoutExercise> Exercises { get; set; } = new();
}
