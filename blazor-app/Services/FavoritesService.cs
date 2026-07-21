using System.Text.Json;
using BlazorApp.Models;

namespace BlazorApp.Services;

public class FavoritesService
{
    private readonly string _filePath;
    private readonly ILogger<FavoritesService> _logger;
    private readonly object _lock = new();
    private List<FavoriteRecipe> _favorites = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public FavoritesService(ILogger<FavoritesService> logger, IWebHostEnvironment env)
    {
        _logger = logger;

        var dataDirectory = Path.Combine(env.ContentRootPath, "App_Data");
        if (!Directory.Exists(dataDirectory))
        {
            Directory.CreateDirectory(dataDirectory);
        }

        _filePath = Path.Combine(dataDirectory, "favorites.json");
        Load();
    }

    public List<FavoriteRecipe> GetFavorites()
    {
        lock (_lock)
        {
            return _favorites.OrderByDescending(f => f.SavedAt).ToList();
        }
    }

    public FavoriteRecipe? GetFavorite(string id)
    {
        lock (_lock)
        {
            return _favorites.FirstOrDefault(f => f.Id == id);
        }
    }

    public FavoriteRecipe AddFavorite(Recipe recipe)
    {
        var favorite = new FavoriteRecipe { Recipe = recipe };

        lock (_lock)
        {
            _favorites.Add(favorite);
            Save();
        }

        _logger.LogInformation($"Favorite recipe saved: {recipe.Name}");
        return favorite;
    }

    public void DeleteFavorite(string id)
    {
        lock (_lock)
        {
            _favorites.RemoveAll(f => f.Id == id);
            Save();
        }
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _favorites = JsonSerializer.Deserialize<List<FavoriteRecipe>>(json, JsonOptions) ?? new();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading favorite recipes");
            _favorites = new();
        }
    }

    private void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_favorites, JsonOptions);
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving favorite recipes");
        }
    }
}
