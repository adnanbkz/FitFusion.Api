namespace FitFusion.Api.Services;

/// <summary>
/// Calcula kcal de un ejercicio con la fórmula MET:
///   kcal = MET * peso(kg) * (duración_min / 60).
/// La tabla MET es aproximada y deliberadamente corta — para ejercicios fuera
/// de la tabla se usa un fallback prudente según la intensidad.
/// </summary>
public sealed class MetCalculator
{
    // exercise (lower) -> intensity -> MET
    private static readonly Dictionary<string, Dictionary<string, float>> Table = new(StringComparer.OrdinalIgnoreCase)
    {
        ["running"]      = new() { ["low"] = 6.0f,  ["moderate"] = 9.8f,  ["vigorous"] = 12.3f },
        ["walking"]      = new() { ["low"] = 2.8f,  ["moderate"] = 3.5f,  ["vigorous"] = 5.0f },
        ["cycling"]      = new() { ["low"] = 4.0f,  ["moderate"] = 6.8f,  ["vigorous"] = 10.0f },
        ["swimming"]     = new() { ["low"] = 5.8f,  ["moderate"] = 8.3f,  ["vigorous"] = 10.0f },
        ["rowing"]       = new() { ["low"] = 4.8f,  ["moderate"] = 7.0f,  ["vigorous"] = 11.0f },
        ["weight"]       = new() { ["low"] = 3.5f,  ["moderate"] = 5.0f,  ["vigorous"] = 6.0f  },
        ["calisthenics"] = new() { ["low"] = 3.5f,  ["moderate"] = 5.0f,  ["vigorous"] = 8.0f  },
        ["yoga"]         = new() { ["low"] = 2.0f,  ["moderate"] = 3.0f,  ["vigorous"] = 4.0f  },
        ["hiit"]         = new() { ["low"] = 6.0f,  ["moderate"] = 8.0f,  ["vigorous"] = 10.0f },
    };

    private static readonly Dictionary<string, float> Fallback = new(StringComparer.OrdinalIgnoreCase)
    {
        ["low"] = 3.0f, ["moderate"] = 5.0f, ["vigorous"] = 8.0f
    };

    public float MetFor(string exercise, string intensity)
    {
        var key = NormalizeExercise(exercise);
        if (Table.TryGetValue(key, out var row) && row.TryGetValue(intensity.ToLowerInvariant(), out var met))
            return met;
        return Fallback.TryGetValue(intensity.ToLowerInvariant(), out var f) ? f : 4.0f;
    }

    public float Kcal(string exercise, string intensity, int durationMin, float weightKg)
    {
        var met = MetFor(exercise, intensity);
        return met * weightKg * (durationMin / 60f);
    }

    private static string NormalizeExercise(string name)
    {
        var lower = name.ToLowerInvariant();
        return lower switch
        {
            var s when s.Contains("correr") || s.Contains("run")              => "running",
            var s when s.Contains("camin") || s.Contains("walk")              => "walking",
            var s when s.Contains("bici")  || s.Contains("cycl") || s.Contains("bike") => "cycling",
            var s when s.Contains("nad")   || s.Contains("swim")              => "swimming",
            var s when s.Contains("remo")  || s.Contains("row")               => "rowing",
            var s when s.Contains("pesa")  || s.Contains("weight") || s.Contains("strength") => "weight",
            var s when s.Contains("calist") || s.Contains("body")             => "calisthenics",
            var s when s.Contains("yoga")                                     => "yoga",
            var s when s.Contains("hiit")  || s.Contains("interval")          => "hiit",
            _ => lower
        };
    }
}
