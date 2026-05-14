using System.ComponentModel.DataAnnotations;

namespace FitFusion.Api.Models.Entities;

public class Dish
{
    [Key]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SuitableSlots { get; set; } = string.Empty;
    public float DefaultPortionG { get; set; }
    public float KcalPer100g { get; set; }
    public float ProteinPer100g { get; set; }
    public float CarbsPer100g { get; set; }
    public float FatsPer100g { get; set; }
    public string Tags { get; set; } = string.Empty;
}
