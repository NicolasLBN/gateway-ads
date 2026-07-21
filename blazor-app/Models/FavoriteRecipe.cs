namespace BlazorApp.Models;

public class FavoriteRecipe
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime SavedAt { get; set; } = DateTime.Now;
    public Recipe Recipe { get; set; } = new();
}
