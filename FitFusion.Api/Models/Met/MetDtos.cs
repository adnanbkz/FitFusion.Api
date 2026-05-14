namespace FitFusion.Api.Models.Met;

public sealed record MetEstimateRequest(
    string ExerciseName,
    string Intensity,
    int DurationMin,
    float? WeightKgOverride = null);

public sealed record MetEstimateResponse(
    float Met,
    float Kcal,
    float WeightKgUsed);
