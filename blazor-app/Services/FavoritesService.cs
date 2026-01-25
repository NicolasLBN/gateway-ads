using BlazorApp.Models;
using LiteDB;

namespace BlazorApp.Services;

public class FavoritesService : IDisposable
{
    private readonly string _dbPath;
    private readonly LiteDatabase _db;
    private readonly object _lock = new object();
    public event Action? OnFavoritesChanged;

    public FavoritesService(IWebHostEnvironment env)
    {
        var dataPath = Path.Combine(env.ContentRootPath, "Data");
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }
        _dbPath = Path.Combine(dataPath, "favorites.db");
        
        // Create a shared connection to the database
        var connectionString = new ConnectionString
        {
            Filename = _dbPath,
            Connection = ConnectionType.Shared
        };
        _db = new LiteDatabase(connectionString);
    }

    public List<FavoriteRecipe> GetFavorites()
    {
        lock (_lock)
        {
            var favorites = _db.GetCollection<FavoriteRecipe>("favorites");
            return favorites.Query()
                .OrderByDescending(f => f.UpdatedAt)
                .ToList();
        }
    }

    public FavoriteRecipe? GetFavorite(int id)
    {
        lock (_lock)
        {
            var favorites = _db.GetCollection<FavoriteRecipe>("favorites");
            return favorites.FindById(id);
        }
    }

    public bool AddFavorite(FavoriteRecipe favorite)
    {
        if (string.IsNullOrWhiteSpace(favorite.Name))
        {
            return false;
        }

        lock (_lock)
        {
            var favorites = _db.GetCollection<FavoriteRecipe>("favorites");
            
            favorite.CreatedAt = DateTime.Now;
            favorite.UpdatedAt = DateTime.Now;
            
            favorites.Insert(favorite);
            OnFavoritesChanged?.Invoke();
            return true;
        }
    }

    public bool UpdateFavorite(FavoriteRecipe favorite)
    {
        if (string.IsNullOrWhiteSpace(favorite.Name))
        {
            return false;
        }

        lock (_lock)
        {
            var favorites = _db.GetCollection<FavoriteRecipe>("favorites");
            
            favorite.UpdatedAt = DateTime.Now;
            
            var success = favorites.Update(favorite);
            if (success)
            {
                OnFavoritesChanged?.Invoke();
            }
            return success;
        }
    }

    public bool DeleteFavorite(int id)
    {
        lock (_lock)
        {
            var favorites = _db.GetCollection<FavoriteRecipe>("favorites");
            
            var success = favorites.Delete(id);
            if (success)
            {
                OnFavoritesChanged?.Invoke();
            }
            return success;
        }
    }

    public void Dispose()
    {
        _db?.Dispose();
    }
}
