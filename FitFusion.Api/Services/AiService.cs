using System.Globalization;
using System.Text;
using System.Text.Json;
using FitFusion.Api.Data;
using FitFusion.Api.Models.Ai;
using FitFusion.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitFusion.Api.Services;

/// <summary>
/// Orquesta los 4 endpoints de IA: construye prompt + schema, llama a Gemini
/// y devuelve el DTO ya tipado. Si Gemini falla, lanza <see cref="GeminiException"/>.
/// </summary>
public sealed class AiService
{
    private readonly IGeminiClient _gemini;
    private readonly FitFusionDbContext _db;
    private readonly ILogger<AiService> _log;

    private static readonly JsonSerializerOptions PromptJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public AiService(IGeminiClient gemini, FitFusionDbContext db, ILogger<AiService> log)
    {
        _gemini = gemini;
        _db = db;
        _log = log;
    }

    // ------------------------------------------------------------------
    // 1. Recipe kcal refinement
    // ------------------------------------------------------------------
    public Task<RefineRecipeResponse> RefineRecipeAsync(RefineRecipeRequest req, CancellationToken ct)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Eres un nutricionista que verifica el valor energético de una receta.");
        sb.AppendLine("Recibes los ingredientes con su cantidad en gramos y sus macros por 100 g.");
        sb.AppendLine("Devuelve el total real estimado de kcal y macros para el plato terminado,");
        sb.AppendLine("teniendo en cuenta evaporación de agua, pérdidas de cocción y mezcla típica.");
        sb.AppendLine($"Receta: {req.Name}");
        if (!string.IsNullOrWhiteSpace(req.CookingMethod))
            sb.AppendLine($"Método de cocción: {req.CookingMethod}");
        sb.AppendLine("Ingredientes:");
        foreach (var ing in req.Ingredients)
        {
            sb.Append("- ").Append(ing.Name);
            if (!string.IsNullOrWhiteSpace(ing.Brand)) sb.Append(" (").Append(ing.Brand).Append(')');
            sb.Append(": ").Append(ing.QuantityG).Append(" g | ")
              .Append(ing.KcalPer100g).Append(" kcal/100g | P ")
              .Append(ing.ProteinPer100g).Append(" / C ")
              .Append(ing.CarbsPer100g).Append(" / F ")
              .Append(ing.FatsPer100g).Append(" por 100 g")
              .AppendLine();
        }
        sb.AppendLine("Devuelve JSON con totalKcal, totalProteinG, totalCarbsG, totalFatG, notes (opcional, <=120 caracteres) y confidence entre 0 y 1.");

        const string schema = """
        {
          "type": "object",
          "properties": {
            "totalKcal":     { "type": "integer" },
            "totalProteinG": { "type": "integer" },
            "totalCarbsG":   { "type": "integer" },
            "totalFatG":     { "type": "integer" },
            "notes":         { "type": "string" },
            "confidence":    { "type": "number" }
          },
          "required": ["totalKcal","totalProteinG","totalCarbsG","totalFatG","confidence"]
        }
        """;

        return _gemini.GenerateAsync<RefineRecipeResponse>(sb.ToString(), schema, ct);
    }

    // ------------------------------------------------------------------
    // 2. Plate estimation
    // ------------------------------------------------------------------
    public Task<EstimatePlateResponse> EstimatePlateAsync(EstimatePlateRequest req, CancellationToken ct)
    {
        var prompt = $"""
        Eres un nutricionista. El usuario describe un plato en lenguaje natural.
        Devuelve un nombre canónico, los macros por 100 g, y una porción
        estándar razonable (etiqueta + gramos) para ese tipo de plato.

        Descripción: {req.Description}
        Idioma: {req.Locale}

        Si la descripción es ambigua, asume la versión casera más común.
        """;

        const string schema = """
        {
          "type": "object",
          "properties": {
            "name":                { "type": "string" },
            "kcalPer100g":         { "type": "number" },
            "proteinPer100g":      { "type": "number" },
            "carbsPer100g":        { "type": "number" },
            "fatsPer100g":         { "type": "number" },
            "defaultServingLabel": { "type": "string" },
            "defaultServingGrams": { "type": "number" },
            "notes":               { "type": "string" }
          },
          "required": ["name","kcalPer100g","proteinPer100g","carbsPer100g","fatsPer100g","defaultServingLabel","defaultServingGrams"]
        }
        """;

        return _gemini.GenerateAsync<EstimatePlateResponse>(prompt, schema, ct);
    }

    // ------------------------------------------------------------------
    // 3. Routine generation
    // ------------------------------------------------------------------
    public Task<GenerateRoutineResponse> GenerateRoutineAsync(GenerateRoutineRequest req, CancellationToken ct)
    {
        var equipment = (req.Equipment is null || req.Equipment.Count == 0)
            ? "Sin equipamiento (peso corporal)"
            : string.Join(", ", req.Equipment);
        var focus = (req.FocusMuscleGroups is null || req.FocusMuscleGroups.Count == 0)
            ? "Cuerpo completo"
            : string.Join(", ", req.FocusMuscleGroups);

        var prompt = $"""
        Eres un entrenador personal. Diseña UNA sesión de entrenamiento.

        Objetivo:           {req.GoalType}
        Nivel:              {req.Level}
        Días por semana:    {req.DaysPerWeek}
        Minutos por sesión: {req.SessionMinutes}
        Equipamiento:       {equipment}
        Foco muscular:      {focus}

        Reglas:
        - Usa nombres de ejercicio estándar en español (p.ej. "Sentadilla goblet").
        - El grupo muscular debe ser uno de: piernas, glúteos, espalda, pecho, hombros, bíceps, tríceps, abdominal, core, cardio.
        - Series 2-5, repeticiones 5-20, descanso 30-180 s.
        - estimatedDurationMin debe acercarse a {req.SessionMinutes}.
        - 4-8 ejercicios.
        """;

        const string schema = """
        {
          "type": "object",
          "properties": {
            "name":                 { "type": "string" },
            "description":          { "type": "string" },
            "estimatedDurationMin": { "type": "integer" },
            "exercises": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "exerciseName":  { "type": "string" },
                  "muscleGroup":   { "type": "string" },
                  "targetSets":    { "type": "integer" },
                  "targetReps":    { "type": "integer" },
                  "targetWeightKg":{ "type": "number" },
                  "restSeconds":   { "type": "integer" },
                  "notes":         { "type": "string" }
                },
                "required": ["exerciseName","muscleGroup","targetSets","targetReps","restSeconds"]
              }
            }
          },
          "required": ["name","description","estimatedDurationMin","exercises"]
        }
        """;

        return _gemini.GenerateAsync<GenerateRoutineResponse>(prompt, schema, ct);
    }

    // ------------------------------------------------------------------
    // 4. Weekly meal plan
    // ------------------------------------------------------------------
    public async Task<MealPlanResponse> GenerateMealPlanAsync(MealPlanRequest req, CancellationToken ct)
    {
        var days = Math.Clamp(req.DaysCount, 1, 7);
        var meals = Math.Clamp(req.MealsPerDay, 2, 6);
        var slots = PickSlots(meals);
        var targetKcal = Math.Clamp(req.TargetKcal <= 0 ? 2000 : req.TargetKcal, 900, 5000);
        var requiredTags = MapRestrictions(req.Restrictions);

        var allDishes = await _db.Dishes.AsNoTracking().ToListAsync(ct);
        var candidates = allDishes
            .Where(d => HasAllTags(d, requiredTags) && IsRelevantForSlots(d, slots))
            .OrderBy(d => d.Id)
            .ToList();

        if (candidates.Count < meals * 2)
        {
            var restrictions = requiredTags.Count == 0 ? "sin restricciones" : string.Join(", ", requiredTags);
            throw new InvalidOperationException(
                $"No hay suficientes platos en el catalogo para generar {meals} comidas/dia con restricciones: {restrictions}.");
        }

        var dishesById = candidates.ToDictionary(d => d.Id, StringComparer.OrdinalIgnoreCase);
        var prompt = BuildMealPlanSelectionPrompt(req, days, meals, targetKcal, slots, requiredTags, candidates);

        try
        {
            var selection = await _gemini.GenerateAsync<GeminiSelection>(prompt, MealPlanSelectionSchema, ct);
            return BuildPlanFromSelection(selection, dishesById, candidates, slots, days, targetKcal);
        }
        catch (GeminiException e)
        {
            _log.LogWarning(e, "Gemini meal-plan selection failed; using greedy fallback.");
            return BuildGreedyPlan(candidates, slots, days, targetKcal);
        }
    }

    private static string BuildMealPlanSelectionPrompt(
        MealPlanRequest req,
        int days,
        int meals,
        int targetKcal,
        IReadOnlyList<MealSlot> slots,
        IReadOnlyList<string> requiredTags,
        IReadOnlyList<Dish> candidates)
    {
        var compact = candidates.Select(d => new GeminiDishCandidate(
            d.Id,
            d.Name,
            d.SuitableSlots,
            PortionValue(d.KcalPer100g, d.DefaultPortionG),
            PortionValue(d.ProteinPer100g, d.DefaultPortionG),
            PortionValue(d.CarbsPer100g, d.DefaultPortionG),
            PortionValue(d.FatsPer100g, d.DefaultPortionG)));

        var slotTargets = slots.Select(s => new GeminiSlotTarget(
            s.Key,
            s.Name,
            SlotTargetKcal(s, slots, targetKcal)));

        var restrictions = requiredTags.Count == 0 ? "ninguna" : string.Join(", ", requiredTags);
        var compactJson = JsonSerializer.Serialize(compact, PromptJson);
        var slotJson = JsonSerializer.Serialize(slotTargets, PromptJson);

        return $"""
        Eres un nutricionista español. No inventes platos ni macros.
        Tu unica tarea es seleccionar IDs del catalogo local.

        Días: {days}
        Comidas por día: {meals}
        Objetivo kcal/día: {targetKcal}
        Preferencia macros: {req.MacroPreference ?? "equilibrado"}
        Restricciones ya filtradas por backend: {restrictions}

        Slots obligatorios por día, en orden:
        {slotJson}

        Reglas:
        - Devuelve exactamente {days} días y exactamente {meals} comidas por día.
        - Usa solo dishIds que existan en el catalogo.
        - Cada comida debe tener 1 o 2 dishIds.
        - Varía entre días y evita repetir el mismo plato dentro del mismo día.
        - Cada día debe acercarse a {targetKcal} kcal (+-10%) usando las kcal del catalogo.
        - dayName debe ser Lunes, Martes, Miercoles... en orden.
        - slotName debe copiar el nombre español del slot.

        Catalogo disponible en JSON compacto:
        {compactJson}
        """;
    }

    private static MealPlanResponse BuildPlanFromSelection(
        GeminiSelection? selection,
        IReadOnlyDictionary<string, Dish> dishesById,
        IReadOnlyList<Dish> candidates,
        IReadOnlyList<MealSlot> slots,
        int days,
        int targetKcal)
    {
        var selectionDays = selection?.Days ?? [];
        var responseDays = new List<MealPlanDay>(days);

        for (var dayIndex = 0; dayIndex < days; dayIndex++)
        {
            var selectedDay = dayIndex < selectionDays.Count ? selectionDays[dayIndex] : null;
            var usedDishIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var meals = new List<MealPlanMeal>(slots.Count);

            for (var slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                var slot = slots[slotIndex];
                var selectedMeal = FindSelectedMeal(selectedDay, slot, slotIndex);
                var slotTarget = SlotTargetKcal(slot, slots, targetKcal);
                var mealDishes = new List<MealPlanDish>(2);

                foreach (var id in selectedMeal?.DishIds ?? [])
                {
                    if (mealDishes.Count == 2)
                    {
                        break;
                    }

                    if (!dishesById.TryGetValue(id, out var dish) || usedDishIds.Contains(dish.Id))
                    {
                        continue;
                    }

                    usedDishIds.Add(dish.Id);
                    mealDishes.Add(ToMealPlanDish(dish));
                }

                FillMealIfNeeded(mealDishes, candidates, slots, slot, slotTarget, usedDishIds, dayIndex, slotIndex);
                meals.Add(new MealPlanMeal(slot.Name, mealDishes));
            }

            responseDays.Add(new MealPlanDay(DayName(dayIndex, selectedDay?.DayName), meals));
        }

        return new MealPlanResponse(responseDays);
    }

    private static MealPlanResponse BuildGreedyPlan(
        IReadOnlyList<Dish> candidates,
        IReadOnlyList<MealSlot> slots,
        int days,
        int targetKcal)
    {
        var responseDays = new List<MealPlanDay>(days);
        for (var dayIndex = 0; dayIndex < days; dayIndex++)
        {
            var usedDishIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var meals = new List<MealPlanMeal>(slots.Count);

            for (var slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                var slot = slots[slotIndex];
                var slotTarget = SlotTargetKcal(slot, slots, targetKcal);
                var mealDishes = new List<MealPlanDish>(2);
                FillMealIfNeeded(mealDishes, candidates, slots, slot, slotTarget, usedDishIds, dayIndex, slotIndex);
                meals.Add(new MealPlanMeal(slot.Name, mealDishes));
            }

            responseDays.Add(new MealPlanDay(DayName(dayIndex), meals));
        }

        return new MealPlanResponse(responseDays);
    }

    private static void FillMealIfNeeded(
        List<MealPlanDish> mealDishes,
        IReadOnlyList<Dish> candidates,
        IReadOnlyList<MealSlot> slots,
        MealSlot slot,
        int slotTarget,
        HashSet<string> usedDishIds,
        int dayIndex,
        int slotIndex)
    {
        if (mealDishes.Count == 0)
        {
            var first = PickFallbackDish(candidates, slot, slotTarget, usedDishIds, dayIndex, slotIndex);
            usedDishIds.Add(first.Id);
            mealDishes.Add(ToMealPlanDish(first));
        }

        var currentKcal = mealDishes.Sum(d => d.Kcal);
        if (mealDishes.Count < 2 && currentKcal < slotTarget * 0.75f)
        {
            var remainingTarget = Math.Max(120, slotTarget - currentKcal);
            var second = PickFallbackDish(candidates, slot, remainingTarget, usedDishIds, dayIndex + slots.Count, slotIndex);
            usedDishIds.Add(second.Id);
            mealDishes.Add(ToMealPlanDish(second));
        }
    }

    private static Dish PickFallbackDish(
        IReadOnlyList<Dish> candidates,
        MealSlot slot,
        int targetKcal,
        HashSet<string> usedDishIds,
        int dayIndex,
        int slotIndex)
    {
        var suitable = candidates
            .Where(d => IsSuitableForSlot(d, slot))
            .OrderBy(d => Math.Abs(PortionKcal(d) - targetKcal))
            .ThenBy(d => d.Id, StringComparer.Ordinal)
            .ToList();

        if (suitable.Count == 0)
        {
            suitable = candidates
                .OrderBy(d => Math.Abs(PortionKcal(d) - targetKcal))
                .ThenBy(d => d.Id, StringComparer.Ordinal)
                .ToList();
        }

        var close = suitable
            .Where(d => Math.Abs(PortionKcal(d) - targetKcal) <= targetKcal * 0.2f)
            .ToList();
        var pool = close.Count > 0 ? close : suitable;
        var unusedPool = pool.Where(d => !usedDishIds.Contains(d.Id)).ToList();
        if (unusedPool.Count > 0)
        {
            pool = unusedPool;
        }

        var offset = Math.Abs((dayIndex * 17) + (slotIndex * 5)) % pool.Count;
        return pool[offset];
    }

    private static GeminiSelectionMeal? FindSelectedMeal(GeminiSelectionDay? day, MealSlot slot, int slotIndex)
    {
        if (day?.Meals is null || day.Meals.Count == 0)
        {
            return null;
        }

        var bySlot = day.Meals.FirstOrDefault(m => SlotKeyFromName(m.SlotName) == slot.Key);
        if (bySlot is not null)
        {
            return bySlot;
        }

        return slotIndex < day.Meals.Count ? day.Meals[slotIndex] : null;
    }

    private static bool HasAllTags(Dish dish, IReadOnlyList<string> requiredTags)
    {
        if (requiredTags.Count == 0)
        {
            return true;
        }

        var tags = SplitCsv(dish.Tags);
        return requiredTags.All(tags.Contains);
    }

    private static bool IsRelevantForSlots(Dish dish, IReadOnlyList<MealSlot> slots) =>
        slots.Any(slot => IsSuitableForSlot(dish, slot));

    private static bool IsSuitableForSlot(Dish dish, MealSlot slot)
    {
        var slots = SplitCsv(dish.SuitableSlots);
        return slots.Contains("any")
            || slots.Contains(slot.Key)
            || (slot.Key is "midmorning" or "latenight") && slots.Contains("snack");
    }

    private static List<string> MapRestrictions(List<string>? restrictions)
    {
        if (restrictions is null || restrictions.Count == 0)
        {
            return [];
        }

        var result = new List<string>();
        foreach (var raw in restrictions)
        {
            var normalized = NormalizeText(raw);
            var tag = normalized switch
            {
                "vegan" or "vegano" => "vegan",
                "vegetarian" or "vegetariano" or "vegetariana" => "vegetarian",
                "gluten-free" or "gluten free" or "sin gluten" or "celiaco" => "gluten-free",
                "lactose-free" or "lactose free" or "sin lactosa" => "lactose-free",
                "no-pork" or "no pork" or "sin cerdo" => "no-pork",
                "no-seafood" or "no seafood" or "sin marisco" or "sin pescado" => "no-seafood",
                "low-carb" or "low carb" or "bajo en carbohidratos" or "bajo en carbos" => "low-carb",
                "high-protein" or "high protein" or "alto en proteina" => "high-protein",
                "low-sodium" or "low sodium" or "bajo en sodio" => "low-sodium",
                "nut-free" or "nut free" or "sin frutos secos" => "nut-free",
                _ => null,
            };

            if (tag is not null)
            {
                result.Add(tag);
            }
        }

        return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static IReadOnlyList<MealSlot> PickSlots(int meals) => meals switch
    {
        2 => [Slot("lunch"), Slot("dinner")],
        3 => [Slot("breakfast"), Slot("lunch"), Slot("dinner")],
        4 => [Slot("breakfast"), Slot("lunch"), Slot("snack"), Slot("dinner")],
        5 => [Slot("breakfast"), Slot("midmorning"), Slot("lunch"), Slot("snack"), Slot("dinner")],
        _ => [Slot("breakfast"), Slot("midmorning"), Slot("lunch"), Slot("snack"), Slot("dinner"), Slot("latenight")],
    };

    private static MealSlot Slot(string key) => key switch
    {
        "breakfast" => new MealSlot(key, "Desayuno", 0.25f),
        "midmorning" => new MealSlot(key, "Almuerzo", 0.10f),
        "lunch" => new MealSlot(key, "Comida", 0.35f),
        "snack" => new MealSlot(key, "Merienda", 0.10f),
        "dinner" => new MealSlot(key, "Cena", 0.30f),
        "latenight" => new MealSlot(key, "Snack", 0.08f),
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, null),
    };

    private static string? SlotKeyFromName(string? name)
    {
        var normalized = NormalizeText(name);
        return normalized switch
        {
            "breakfast" or "desayuno" => "breakfast",
            "midmorning" or "media manana" or "almuerzo" => "midmorning",
            "lunch" or "comida" => "lunch",
            "snack" or "merienda" => "snack",
            "dinner" or "cena" => "dinner",
            "latenight" or "recena" => "latenight",
            _ => null,
        };
    }

    private static int SlotTargetKcal(MealSlot slot, IReadOnlyList<MealSlot> slots, int targetKcal)
    {
        var totalWeight = slots.Sum(s => s.Weight);
        return Math.Max(120, (int)Math.Round(targetKcal * slot.Weight / totalWeight));
    }

    private static MealPlanDish ToMealPlanDish(Dish dish) => new(
        dish.Name,
        PortionKcal(dish),
        PortionValue(dish.ProteinPer100g, dish.DefaultPortionG),
        PortionValue(dish.CarbsPer100g, dish.DefaultPortionG),
        PortionValue(dish.FatsPer100g, dish.DefaultPortionG),
        Shorten(dish.Description, 80));

    private static int PortionKcal(Dish dish) => PortionValue(dish.KcalPer100g, dish.DefaultPortionG);

    private static int PortionValue(float per100g, float portionG) =>
        Math.Max(0, (int)Math.Round(per100g * portionG / 100f));

    private static string? Shorten(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= max ? value : value[..Math.Max(0, max - 3)] + "...";
    }

    private static string DayName(int index, string? geminiName = null)
    {
        if (!string.IsNullOrWhiteSpace(geminiName))
        {
            return geminiName;
        }

        string[] names = ["Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado", "Domingo"];
        return names[index % names.Length];
    }

    private static HashSet<string> SplitCsv(string value) =>
        value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static string NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private const string MealPlanSelectionSchema = """
    {
      "type": "object",
      "properties": {
        "days": {
          "type": "array",
          "items": {
            "type": "object",
            "properties": {
              "dayName": { "type": "string" },
              "meals": {
                "type": "array",
                "items": {
                  "type": "object",
                  "properties": {
                    "slotName": { "type": "string" },
                    "dishIds": {
                      "type": "array",
                      "items": { "type": "string" }
                    }
                  },
                  "required": ["slotName","dishIds"]
                }
              }
            },
            "required": ["dayName","meals"]
          }
        }
      },
      "required": ["days"]
    }
    """;

    private sealed record MealSlot(string Key, string Name, float Weight);
    private sealed record GeminiDishCandidate(string Id, string Name, string Slots, int Kcal, int P, int C, int F);
    private sealed record GeminiSlotTarget(string Key, string Name, int TargetKcal);
    private sealed record GeminiSelection(List<GeminiSelectionDay>? Days);
    private sealed record GeminiSelectionDay(string DayName, List<GeminiSelectionMeal>? Meals);
    private sealed record GeminiSelectionMeal(string SlotName, List<string>? DishIds);
}
