using LiteDB;

namespace BlazorApp.Models;

public class FavoriteRecipe
{
    [BsonId]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime SavedAt { get; set; } = DateTime.Now;
    public Recipe Recipe { get; set; } = new();
}
