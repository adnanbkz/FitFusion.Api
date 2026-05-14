using System.ComponentModel.DataAnnotations;

namespace FitFusion.Api.Models.Entities;

public class WorkoutExercise
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WorkoutId { get; set; }
    public Workout Workout { get; set; } = null!;

    public string ExerciseName { get; set; } = string.Empty;
    public string? MuscleGroup { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public float? WeightKg { get; set; }
    public int? RestSeconds { get; set; }
    public string? Intensity { get; set; }   // "low" | "moderate" | "vigorous"
    public int OrderIndex { get; set; }
}
