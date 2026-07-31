using BlazorApp.Models;
using LiteDB;

namespace BlazorApp.Services;

public class FavoritesService : IDisposable
{
    private readonly LiteDatabase _db;
    private readonly ILiteCollection<FavoriteRecipe> _favorites;
    private readonly ILogger<FavoritesService> _logger;
    private readonly object _lock = new();

    public FavoritesService(ILogger<FavoritesService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        var dataDir = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDir);
        _db = new LiteDatabase(Path.Combine(dataDir, "favorites.db"));
        _favorites = _db.GetCollection<FavoriteRecipe>("favorites");
        _favorites.EnsureIndex(f => f.SavedAt);

        SeedIfEmpty();
    }

    public List<FavoriteRecipe> GetFavorites()
    {
        lock (_lock)
        {
            return _favorites.FindAll().OrderByDescending(f => f.SavedAt).ToList();
        }
    }

    public FavoriteRecipe? GetFavorite(string id)
    {
        lock (_lock)
        {
            return _favorites.FindById(id);
        }
    }

    public FavoriteRecipe AddFavorite(Recipe recipe)
    {
        var favorite = new FavoriteRecipe
        {
            Recipe = CloneRecipe(recipe),
            SavedAt = DateTime.Now
        };

        lock (_lock)
        {
            _favorites.Insert(favorite);
        }

        _logger.LogInformation("Favorite saved: {Name} ({Steps} steps)", recipe.Name, recipe.ProcessSteps.Count);
        return favorite;
    }

    public void DeleteFavorite(string id)
    {
        lock (_lock)
        {
            _favorites.Delete(id);
        }
    }

    private void SeedIfEmpty()
    {
        lock (_lock)
        {
            if (_favorites.Count() > 0)
                return;

            foreach (var recipe in BuildSeedRecipes())
            {
                _favorites.Insert(new FavoriteRecipe
                {
                    Recipe = recipe,
                    SavedAt = DateTime.Now.AddDays(-Random.Shared.Next(1, 14))
                });
            }

            _logger.LogInformation("Seeded {Count} professional favorite recipes", 3);
        }
    }

    private static List<Recipe> BuildSeedRecipes() =>
    [
        new Recipe
        {
            Name = "Sérum Hydratant Acide Hyaluronique 2%",
            ProcessSteps =
            [
                new RecipeStep
                {
                    Type = RecipeStepType.Ajout,
                    Ingredients =
                    [
                        new() { Name = "Eau purifiée (phase aqueuse)", Amount = 85, AmountUnit = "mL", Concentration = 100, ConcentrationUnit = "%" },
                        new() { Name = "Acide hyaluronique (polymère)", Amount = 2, AmountUnit = "g", Concentration = 2, ConcentrationUnit = "%" },
                        new() { Name = "Conservateur (phenoxyéthanol)", Amount = 1, AmountUnit = "mL", Concentration = 1, ConcentrationUnit = "%" }
                    ]
                },
                new RecipeStep
                {
                    Type = RecipeStepType.Melange,
                    SelectedIngredientNames =
                    [
                        "Eau purifiée (phase aqueuse)",
                        "Acide hyaluronique (polymère)",
                        "Conservateur (phenoxyéthanol)"
                    ],
                    MixDurationMinutes = 20,
                    MixSpeedRpm = 500
                },
                new RecipeStep
                {
                    Type = RecipeStepType.Cuisson,
                    TargetTemperatureC = 40,
                    CookDurationMinutes = 15
                }
            ]
        },
        new Recipe
        {
            Name = "Solution Tampon Phosphate (PBS 10x, pH 7.4)",
            ProcessSteps =
            [
                new RecipeStep
                {
                    Type = RecipeStepType.Ajout,
                    Ingredients =
                    [
                        new() { Name = "NaCl", Amount = 80, AmountUnit = "g", Concentration = 1.37, ConcentrationUnit = "mol/L" },
                        new() { Name = "KCl", Amount = 2, AmountUnit = "g", Concentration = 0.027, ConcentrationUnit = "mol/L" },
                        new() { Name = "Na2HPO4", Amount = 14.4, AmountUnit = "g", Concentration = 0.1, ConcentrationUnit = "mol/L" },
                        new() { Name = "KH2PO4", Amount = 2.4, AmountUnit = "g", Concentration = 0.018, ConcentrationUnit = "mol/L" },
                        new() { Name = "Eau distillée q.s.", Amount = 800, AmountUnit = "mL", Concentration = 100, ConcentrationUnit = "%" }
                    ]
                },
                new RecipeStep
                {
                    Type = RecipeStepType.Melange,
                    SelectedIngredientNames = ["NaCl", "KCl", "Na2HPO4", "KH2PO4", "Eau distillée q.s."],
                    MixDurationMinutes = 30,
                    MixSpeedRpm = 300
                },
                new RecipeStep
                {
                    Type = RecipeStepType.Cuisson,
                    TargetTemperatureC = 25,
                    CookDurationMinutes = 10
                }
            ]
        },
        new Recipe
        {
            Name = "Accord Parfum Fleuri - Base Éthanolique",
            ProcessSteps =
            [
                new RecipeStep
                {
                    Type = RecipeStepType.Ajout,
                    Ingredients =
                    [
                        new() { Name = "Éthanol 85%", Amount = 850, AmountUnit = "mL", Concentration = 85, ConcentrationUnit = "%" },
                        new() { Name = "Huile essentielle de rose", Amount = 12, AmountUnit = "mL", Concentration = 1.2, ConcentrationUnit = "%" },
                        new() { Name = "Absolu de jasmin", Amount = 8, AmountUnit = "mL", Concentration = 0.8, ConcentrationUnit = "%" },
                        new() { Name = "Fixateur (musc synthétique)", Amount = 5, AmountUnit = "mL", Concentration = 0.5, ConcentrationUnit = "%" }
                    ]
                },
                new RecipeStep
                {
                    Type = RecipeStepType.Melange,
                    SelectedIngredientNames =
                    [
                        "Éthanol 85%",
                        "Huile essentielle de rose",
                        "Absolu de jasmin",
                        "Fixateur (musc synthétique)"
                    ],
                    MixDurationMinutes = 45,
                    MixSpeedRpm = 150
                }
            ]
        }
    ];

    private static Recipe CloneRecipe(Recipe source) =>
        System.Text.Json.JsonSerializer.Deserialize<Recipe>(
            System.Text.Json.JsonSerializer.Serialize(source)) ?? new Recipe();

    public void Dispose() => _db.Dispose();
}
