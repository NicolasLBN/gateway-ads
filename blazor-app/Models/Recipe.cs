namespace BlazorApp.Models;

public enum RecipeStepType
{
    Ajout = 0,
    Melange = 1,
    Cuisson = 2
}

public class Ingredient
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Volume or mass value.</summary>
    public double Amount { get; set; }

    /// <summary>e.g. mL, L, g, kg</summary>
    public string AmountUnit { get; set; } = "mL";

    public double Concentration { get; set; }

    /// <summary>e.g. mol/L, %, mg/mL</summary>
    public string ConcentrationUnit { get; set; } = "%";

    public bool IsMassUnit => AmountUnit is "g" or "kg" or "mg";

    public double QuantityForPlc => IsMassUnit ? Amount : 0;
    public double VolumeForPlc => IsMassUnit ? 0 : Amount;
    public double MolarMassForPlc => ConcentrationUnit.Contains("mol", StringComparison.OrdinalIgnoreCase) ? Concentration : 0;

    public string Summary =>
        $"{Name} — {Amount:G4} {AmountUnit}, {Concentration:G4} {ConcentrationUnit}";
}

public class RecipeStep
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public RecipeStepType Type { get; set; }

    public List<Ingredient> Ingredients { get; set; } = new();

    public List<string> SelectedIngredientNames { get; set; } = new();
    public double MixDurationMinutes { get; set; }
    public double MixSpeedRpm { get; set; }

    public double TargetTemperatureC { get; set; }
    public double CookDurationMinutes { get; set; }

    public string TypeLabel => Type switch
    {
        RecipeStepType.Ajout => "Ajout",
        RecipeStepType.Melange => "Mélange",
        RecipeStepType.Cuisson => "Cuisson",
        _ => Type.ToString()
    };

    public string PlcStepName => Type switch
    {
        RecipeStepType.Ajout => "Ajout",
        RecipeStepType.Melange => "Melange",
        RecipeStepType.Cuisson => "Cuisson",
        _ => Type.ToString()
    };

    public string DetailSummary => Type switch
    {
        RecipeStepType.Ajout => Ingredients.Count == 0
            ? "Aucun ingrédient"
            : string.Join("; ", Ingredients.Select(i => i.Summary)),
        RecipeStepType.Melange =>
            $"Ingrédients: {string.Join(", ", SelectedIngredientNames)} | {MixDurationMinutes:G4} min @ {MixSpeedRpm:G4} RPM",
        RecipeStepType.Cuisson =>
            $"{TargetTemperatureC:G4} °C pendant {CookDurationMinutes:G4} min",
        _ => string.Empty
    };
}

public class Recipe
{
    public string Name { get; set; } = string.Empty;
    public List<RecipeStep> ProcessSteps { get; set; } = new();

    public List<string> Steps => ProcessSteps.Select(s => s.PlcStepName).ToList();

    public List<Ingredient> Ingredients =>
        ProcessSteps
            .Where(s => s.Type == RecipeStepType.Ajout)
            .SelectMany(s => s.Ingredients)
            .ToList();

    public IReadOnlyList<string> AllIngredientNames =>
        Ingredients
            .Select(i => i.Name.Trim())
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    public int TotalIngredientCount => AllIngredientNames.Count;

    public bool HasAjoutStep => ProcessSteps.Any(s => s.Type == RecipeStepType.Ajout);

    public double PreparationVolume { get; set; }
    public double PreparationConcentration { get; set; }
}
