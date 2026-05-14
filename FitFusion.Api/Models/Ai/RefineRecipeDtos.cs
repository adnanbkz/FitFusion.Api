namespace FitFusion.Api.Models.Ai;

public sealed record IngredientInput(
    string Name,
    int QuantityG,
    float KcalPer100g,
    float ProteinPer100g = 0f,
    float CarbsPer100g = 0f,
    float FatsPer100g = 0f,
    string? Brand = null);

public sealed record RefineRecipeRequest(
    string Name,
    List<IngredientInput> Ingredients,
    string? CookingMethod = null);

public sealed record RefineRecipeResponse(
    int TotalKcal,
    int TotalProteinG,
    int TotalCarbsG,
    int TotalFatG,
    string? Notes = null,
    float Confidence = 0.7f);
