using FitFusion.Api.Models.Ai;
using FitFusion.Api.Models.Nutrition;
using FitFusion.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitFusion.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/nutrition")]
public class NutritionController : ControllerBase
{
    private readonly CalorieCalculator _calories;

    public NutritionController(CalorieCalculator calories)
    {
        _calories = calories;
    }

    /// <summary>
    /// Calcula el objetivo de calorías/macros diarias a partir de las métricas
    /// del usuario (Mifflin-St Jeor + nivel de actividad + objetivo).
    /// </summary>
    [HttpPost("calorie-goal")]
    public ActionResult<CalorieGoalResponse> CalorieGoal(CalorieGoalRequest req)
    {
        if (req.HeightCm <= 0 || req.WeightKg <= 0 || req.Age <= 0)
        {
            return BadRequest(new ErrorResponse("heightCm, weightKg y age deben ser mayores que 0."));
        }

        return Ok(_calories.Compute(req));
    }
}
