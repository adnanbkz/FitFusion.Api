namespace FitFusion.Api.Models.Ai;

public sealed record EstimatePlateRequest(
    string Description,
    string Locale = "es");

public sealed record EstimatePlateResponse(
    string Name,
    float KcalPer100g,
    float ProteinPer100g,
    float CarbsPer100g,
    float FatsPer100g,
    string DefaultServingLabel,
    float DefaultServingGrams,
    string? Notes = null);
