using BlazorApp.Models;
using BlazorApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorApp.Controllers;

[ApiController]
[Authorize]
[Route("api/process")]
public class ProcessController : ControllerBase
{
    private readonly AppStateService _appState;

    public ProcessController(AppStateService appState)
    {
        _appState = appState;
    }

    [HttpGet("status")]
    public ActionResult<object> GetStatus()
    {
        var status = _appState.LatestProcessStatus;
        return Ok(new
        {
            connected = _appState.IsConnected,
            state = status?.State.ToString() ?? (_appState.IsConnected ? "Unknown" : "Offline"),
            stateCode = status?.State is PackMLState s ? (int)s : (int?)null,
            stateName = status?.StateName,
            currentStepIndex = status?.CurrentStepIndex ?? 0,
            currentStepName = status?.CurrentStepName,
            totalSteps = status?.TotalSteps ?? 0,
            progress = status?.Progress ?? 0,
            stepTimeRemaining = status?.StepTimeRemaining ?? 0,
            isHeld = status?.IsHeld ?? false,
            processDone = status?.ProcessDone ?? false,
            recipeName = _appState.CurrentRecipe?.Name
        });
    }
}
