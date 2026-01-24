namespace BlazorApp.Models;

public class FavoriteRecipe
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Ingredient> Ingredients { get; set; } = new();
    public double PreparationVolume { get; set; }
    public double PreparationConcentration { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
