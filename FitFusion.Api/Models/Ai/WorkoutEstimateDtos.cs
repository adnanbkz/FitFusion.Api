namespace FitFusion.Api.Models.Ai;

public sealed record WorkoutEstimateRequest(
    int DurationMin,
    List<WorkoutEstimateExercise> Exercises,
    float? WeightKgOverride = null);

public sealed record WorkoutEstimateExercise(
    string Name,
    int? Sets = null,
    int? Reps = null,
    float? WeightKg = null);

public sealed record WorkoutEstimateResponse(
    string Category,
    string Intensity,
    float EstimatedMet,
    int Kcal,
    float WeightKgUsed,
    float Confidence,
    string? Explanation);

internal sealed record GeminiWorkoutClassification(
    string Category,
    string Intensity,
    float EstimatedMet,
    float Confidence,
    string? Reason);
