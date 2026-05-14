using FitFusion.Api.Auth;
using FitFusion.Api.Data;
using FitFusion.Api.Models.Ai;
using FitFusion.Api.Models.Met;
using FitFusion.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitFusion.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/met")]
public class MetController : ControllerBase
{
    private readonly MetCalculator _met;
    private readonly FitFusionDbContext _db;

    public MetController(MetCalculator met, FitFusionDbContext db)
    {
        _met = met;
        _db  = db;
    }

    [HttpPost("estimate")]
    public async Task<ActionResult<MetEstimateResponse>> Estimate(MetEstimateRequest req, CancellationToken ct)
    {
        if (req.DurationMin <= 0)
            return BadRequest(new ErrorResponse("durationMin debe ser mayor que 0"));

        float weight;
        if (req.WeightKgOverride is { } w && w > 0)
        {
            weight = w;
        }
        else
        {
            var uid = User.RequireUid();
            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.Uid == uid, ct);
            if (profile?.WeightKg is null || profile.WeightKg <= 0)
                return BadRequest(new ErrorResponse("Falta peso en el perfil; pásalo en weightKgOverride o completa el perfil."));
            weight = profile.WeightKg.Value;
        }

        var met  = _met.MetFor(req.ExerciseName, req.Intensity);
        var kcal = _met.Kcal(req.ExerciseName, req.Intensity, req.DurationMin, weight);
        return Ok(new MetEstimateResponse(met, kcal, weight));
    }
}
