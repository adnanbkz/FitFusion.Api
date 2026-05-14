using FitFusion.Api.Models.Ai;
using FitFusion.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitFusion.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly AiService _ai;
    private readonly ILogger<AiController> _log;

    public AiController(AiService ai, ILogger<AiController> log)
    {
        _ai  = ai;
        _log = log;
    }

    [HttpPost("recipe/kcal")]
    public Task<IActionResult> RefineRecipe(RefineRecipeRequest req, CancellationToken ct) =>
        Run(() => _ai.RefineRecipeAsync(req, ct));

    [HttpPost("plate/estimate")]
    public Task<IActionResult> EstimatePlate(EstimatePlateRequest req, CancellationToken ct) =>
        Run(() => _ai.EstimatePlateAsync(req, ct));

    [HttpPost("routine/generate")]
    public Task<IActionResult> GenerateRoutine(GenerateRoutineRequest req, CancellationToken ct) =>
        Run(() => _ai.GenerateRoutineAsync(req, ct));

    [HttpPost("meal-plan/generate")]
    public Task<IActionResult> GenerateMealPlan(MealPlanRequest req, CancellationToken ct) =>
        Run(() => _ai.GenerateMealPlanAsync(req, ct));

    private async Task<IActionResult> Run<T>(Func<Task<T>> action)
    {
        try
        {
            var result = await action();
            return Ok(result);
        }
        catch (GeminiException e)
        {
            _log.LogWarning(e, "Gemini error");
            return StatusCode(502, new ErrorResponse(e.Message));
        }
        catch (InvalidOperationException e)
        {
            _log.LogWarning(e, "Invalid AI request");
            return BadRequest(new ErrorResponse(e.Message));
        }
    }
}
