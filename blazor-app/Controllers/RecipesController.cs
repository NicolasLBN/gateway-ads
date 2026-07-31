using BlazorApp.Models;
using BlazorApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorApp.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class RecipesController : ControllerBase
{
    private readonly FavoritesService _favorites;

    public RecipesController(FavoritesService favorites)
    {
        _favorites = favorites;
    }

    /// <summary>Library of recipes (from favorites catalog).</summary>
    [HttpGet("recipes")]
    public ActionResult<IEnumerable<object>> GetRecipes()
    {
        var items = _favorites.GetFavorites().Select(f => new
        {
            id = f.Id,
            name = f.Recipe.Name,
            stepCount = f.Recipe.ProcessSteps.Count,
            ingredientCount = f.Recipe.TotalIngredientCount,
            steps = f.Recipe.ProcessSteps.Select(s => new
            {
                type = s.TypeLabel,
                details = s.DetailSummary
            }),
            savedAt = f.SavedAt
        });
        return Ok(items);
    }

    [HttpGet("favorites")]
    public ActionResult<IEnumerable<object>> GetFavorites()
    {
        var items = _favorites.GetFavorites().Select(f => new
        {
            id = f.Id,
            savedAt = f.SavedAt,
            recipe = new
            {
                name = f.Recipe.Name,
                processSteps = f.Recipe.ProcessSteps,
                ingredients = f.Recipe.Ingredients.Select(i => new
                {
                    i.Name,
                    i.Amount,
                    i.AmountUnit,
                    i.Concentration,
                    i.ConcentrationUnit
                })
            }
        });
        return Ok(items);
    }
}
