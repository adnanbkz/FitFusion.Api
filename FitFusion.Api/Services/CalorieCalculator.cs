using System.Globalization;
using System.Text;
using FitFusion.Api.Models.Nutrition;

namespace FitFusion.Api.Services;

/// <summary>
/// Calcula el objetivo calórico diario y el reparto de macros a partir de las
/// métricas del usuario:
///   1. BMR  — metabolismo basal con la ecuación de Mifflin-St Jeor.
///   2. TDEE — gasto de mantenimiento = BMR × multiplicador de actividad.
///   3. Objetivo — TDEE ajustado según el objetivo (perder grasa / ganar / ...).
///   4. Macros — proteína por peso corporal, grasa al 25 % de las kcal y el
///      resto en carbohidratos.
/// Es determinista (no usa IA), por eso vive aquí y no en <see cref="AiService"/>.
/// </summary>
public sealed class CalorieCalculator
{
    public CalorieGoalResponse Compute(CalorieGoalRequest req)
    {
        var heightCm = Math.Clamp(req.HeightCm, 80, 260);
        var weightKg = Math.Clamp(req.WeightKg, 25f, 300f);
        var age      = Math.Clamp(req.Age, 10, 100);

        // Mifflin-St Jeor. La constante final depende del sexo; si el cliente no
        // lo envía se usa -78 (media de +5 hombres / -161 mujeres).
        var sexConstant = NormalizeSex(req.Sex) switch
        {
            "male"   => 5f,
            "female" => -161f,
            _        => -78f,
        };
        var bmr = (10f * weightKg) + (6.25f * heightCm) - (5f * age) + sexConstant;

        var maintenance = bmr * ActivityFactor(req.ActivityLevel);
        var target = Math.Max(1200f, maintenance * GoalFactor(req.GoalType));

        // Macros: proteína proporcional al peso, grasa al 25 % de las kcal y el
        // resto de energía en carbohidratos (nunca negativo).
        var proteinG = (int)Math.Round(ProteinPerKg(req.GoalType) * weightKg);
        var fatG     = (int)Math.Round(target * 0.25f / 9f);
        var carbKcal = target - (proteinG * 4f) - (fatG * 9f);
        var carbsG   = (int)Math.Round(Math.Max(0f, carbKcal) / 4f);

        return new CalorieGoalResponse(
            Bmr:             (int)Math.Round(bmr),
            MaintenanceKcal: (int)Math.Round(maintenance),
            TargetKcal:      (int)Math.Round(target),
            ProteinG:        proteinG,
            CarbsG:          carbsG,
            FatG:            fatG);
    }

    // Multiplicadores de actividad estándar. Acepta las claves en inglés y las
    // etiquetas en español que usa la app Android.
    private static float ActivityFactor(string? level) => Normalize(level) switch
    {
        "sedentary" or "sedentario"         => 1.2f,
        "light" or "ligero"                 => 1.375f,
        "moderate" or "medio" or "moderado" => 1.55f,
        "active" or "alto" or "activo"      => 1.725f,
        "athlete" or "atleta"               => 1.9f,
        _                                   => 1.375f,
    };

    // Ajuste sobre el mantenimiento según el objetivo del usuario.
    private static float GoalFactor(string? goal)
    {
        var g = Normalize(goal);
        if (g.Contains("perder") || g.Contains("grasa") || g.Contains("lose")) return 0.80f;
        if (g.Contains("ganar") || g.Contains("musculo") || g.Contains("gain")) return 1.12f;
        return 1.0f; // mantener peso / mejorar resistencia / desconocido
    }

    // Proteína objetivo por kg de peso corporal: más alta en déficit o volumen.
    private static float ProteinPerKg(string? goal)
    {
        var g = Normalize(goal);
        if (g.Contains("perder") || g.Contains("grasa") || g.Contains("lose")) return 2.0f;
        if (g.Contains("ganar") || g.Contains("musculo") || g.Contains("gain")) return 2.0f;
        return 1.6f;
    }

    private static string? NormalizeSex(string? sex) => Normalize(sex) switch
    {
        "male" or "hombre" or "masculino" or "m"  => "male",
        "female" or "mujer" or "femenino" or "f"  => "female",
        _                                         => null,
    };

    /// <summary>Minúsculas + sin acentos, para tolerar "músculo"/"musculo" etc.</summary>
    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
