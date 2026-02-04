using System.IO;
using WpfApp.Models;
using LiteDB;

namespace WpfApp.Services;

public class FavoritesService : IDisposable
{
    private readonly string _dbPath;
    private readonly LiteDatabase _db;
    private readonly object _lock = new object();
    public event Action? OnFavoritesChanged;

    public FavoritesService()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WpfGatewayADS");
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }
        _dbPath = Path.Combine(appDataPath, "favorites.db");
        
        // Create a shared connection to the database
        var connectionString = new ConnectionString
        {
            Filename = _dbPath,
            Connection = ConnectionType.Shared
        };
        _db = new LiteDatabase(connectionString);
        
        // Seed with pre-existing favorites if database is empty
        SeedFavoritesIfEmpty();
    }

    private void SeedFavoritesIfEmpty()
    {
        lock (_lock)
        {
            var favorites = _db.GetCollection<FavoriteRecipe>("favorites");
            if (favorites.Count() == 0)
            {
                // Add pre-existing favorite recipes
                var defaultFavorites = new List<FavoriteRecipe>
                {
                    new FavoriteRecipe
                    {
                        Name = "Standard Buffer Solution",
                        PreparationVolume = 1.0,
                        PreparationConcentration = 0.1,
                        Ingredients = new List<Ingredient>
                        {
                            new Ingredient { Name = "Sodium Phosphate Dibasic", Volume = 100, MolarMass = 141.96 },
                            new Ingredient { Name = "Sodium Phosphate Monobasic", Volume = 50, MolarMass = 119.98 }
                        },
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new FavoriteRecipe
                    {
                        Name = "Saline Solution",
                        PreparationVolume = 1.0,
                        PreparationConcentration = 0.9,
                        Ingredients = new List<Ingredient>
                        {
                            new Ingredient { Name = "Sodium Chloride (NaCl)", Volume = 900, MolarMass = 58.44 }
                        },
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new FavoriteRecipe
                    {
                        Name = "Tris-HCl Buffer",
                        PreparationVolume = 0.5,
                        PreparationConcentration = 0.05,
                        Ingredients = new List<Ingredient>
                        {
                            new Ingredient { Name = "Tris Base", Volume = 250, MolarMass = 121.14 },
                            new Ingredient { Name = "Hydrochloric Acid", Volume = 50, MolarMass = 36.46 }
                        },
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }
                };

                foreach (var favorite in defaultFavorites)
                {
                    favorites.Insert(favorite);
                }
            }
        }
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
        _db.Dispose();
    }
}
