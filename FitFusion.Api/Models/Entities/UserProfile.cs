using System.ComponentModel.DataAnnotations;

namespace FitFusion.Api.Models.Entities;

public class UserProfile
{
    [Key]
    public string Uid { get; set; } = string.Empty;

    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public float? WeightKg { get; set; }
    public int? HeightCm { get; set; }
    public int? Age { get; set; }
    public string? Sex { get; set; }            // "male" | "female" | "other"
    public string? ActivityLevel { get; set; }  // "sedentary" | "light" | "moderate" | "active" | "athlete"
    public int? TargetKcal { get; set; }
    public int? TargetProteinG { get; set; }
    public int? TargetCarbsG { get; set; }
    public int? TargetFatG { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
