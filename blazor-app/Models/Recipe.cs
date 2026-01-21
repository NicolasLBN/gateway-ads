namespace BlazorApp.Models;

public class Recipe
{
    public string Name { get; set; } = string.Empty;
    public List<Ingredient> Ingredients { get; set; } = new();
    public double PreparationVolume { get; set; }
    public double PreparationConcentration { get; set; }
}

public class Ingredient
{
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public double Volume { get; set; }
    public double MolarMass { get; set; }
}
