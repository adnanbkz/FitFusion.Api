namespace FitFusion.Api.Models.Nutrition;

/// <summary>
/// Petición para calcular el objetivo calórico diario a partir de las métricas
/// del usuario. <see cref="Sex"/> es opcional: si no se indica, el cálculo usa
/// una constante neutra (media hombre/mujer) en Mifflin-St Jeor.
/// </summary>
public sealed record CalorieGoalRequest(
    int HeightCm,
    float WeightKg,
    int Age,
    string ActivityLevel,
    string GoalType,
    string? Sex = null);

/// <summary>
/// Resultado del cálculo: metabolismo basal, gasto de mantenimiento (TDEE),
/// objetivo calórico ajustado al objetivo del usuario y reparto de macros.
/// </summary>
public sealed record CalorieGoalResponse(
    int Bmr,
    int MaintenanceKcal,
    int TargetKcal,
    int ProteinG,
    int CarbsG,
    int FatG);
