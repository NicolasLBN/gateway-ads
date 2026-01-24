using BlazorApp.Models;
using LiteDB;

namespace BlazorApp.Services;

public class FavoritesService
{
    private readonly string _dbPath;
    public event Action? OnFavoritesChanged;

    public FavoritesService(IWebHostEnvironment env)
    {
        var dataPath = Path.Combine(env.ContentRootPath, "Data");
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }
        _dbPath = Path.Combine(dataPath, "favorites.db");
    }

    public List<FavoriteRecipe> GetFavorites()
    {
        using var db = new LiteDatabase(_dbPath);
        var favorites = db.GetCollection<FavoriteRecipe>("favorites");
        return favorites.FindAll().OrderByDescending(f => f.UpdatedAt).ToList();
    }

    public FavoriteRecipe? GetFavorite(int id)
    {
        using var db = new LiteDatabase(_dbPath);
        var favorites = db.GetCollection<FavoriteRecipe>("favorites");
        return favorites.FindById(id);
    }

    public bool AddFavorite(FavoriteRecipe favorite)
    {
        if (string.IsNullOrWhiteSpace(favorite.Name))
        {
            return false;
        }

        using var db = new LiteDatabase(_dbPath);
        var favorites = db.GetCollection<FavoriteRecipe>("favorites");
        
        favorite.CreatedAt = DateTime.Now;
        favorite.UpdatedAt = DateTime.Now;
        
        favorites.Insert(favorite);
        OnFavoritesChanged?.Invoke();
        return true;
    }

    public bool UpdateFavorite(FavoriteRecipe favorite)
    {
        if (string.IsNullOrWhiteSpace(favorite.Name))
        {
            return false;
        }

        using var db = new LiteDatabase(_dbPath);
        var favorites = db.GetCollection<FavoriteRecipe>("favorites");
        
        favorite.UpdatedAt = DateTime.Now;
        
        var success = favorites.Update(favorite);
        if (success)
        {
            OnFavoritesChanged?.Invoke();
        }
        return success;
    }

    public bool DeleteFavorite(int id)
    {
        using var db = new LiteDatabase(_dbPath);
        var favorites = db.GetCollection<FavoriteRecipe>("favorites");
        
        var success = favorites.Delete(id);
        if (success)
        {
            OnFavoritesChanged?.Invoke();
        }
        return success;
    }
}
