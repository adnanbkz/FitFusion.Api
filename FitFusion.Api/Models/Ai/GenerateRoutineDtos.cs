namespace FitFusion.Api.Models.Ai;

public sealed record GenerateRoutineRequest(
    string GoalType,
    string Level,
    int DaysPerWeek,
    int SessionMinutes,
    List<string>? Equipment = null,
    List<string>? FocusMuscleGroups = null);

public sealed record GenerateRoutineResponse(
    string Name,
    string Description,
    int EstimatedDurationMin,
    List<RoutineExerciseDto> Exercises);

public sealed record RoutineExerciseDto(
    string ExerciseName,
    string MuscleGroup,
    int TargetSets = 3,
    int TargetReps = 10,
    float? TargetWeightKg = null,
    int RestSeconds = 60,
    string? Notes = null);
