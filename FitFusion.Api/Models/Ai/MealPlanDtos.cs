namespace FitFusion.Api.Models.Ai;

public sealed record MealPlanRequest(
    int TargetKcal,
    int MealsPerDay,
    int DaysCount = 7,
    List<string>? Restrictions = null,
    string? MacroPreference = null);

public sealed record MealPlanResponse(List<MealPlanDay> Days);

public sealed record MealPlanDay(string DayName, List<MealPlanMeal> Meals);

public sealed record MealPlanMeal(string SlotName, List<MealPlanDish> Dishes);

public sealed record MealPlanDish(
    string Name,
    int Kcal,
    int ProteinG,
    int CarbsG,
    int FatG,
    string? DescriptionShort = null);
