using FitFusion.Api.Auth;
using FitFusion.Api.Data;
using FitFusion.Api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitFusion.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly FitFusionDbContext _db;

    public UsersController(FitFusionDbContext db) { _db = db; }

    public sealed record UserProfileDto(
        string Uid,
        string? DisplayName,
        string? Email,
        float? WeightKg,
        int? HeightCm,
        int? Age,
        string? Sex,
        string? ActivityLevel,
        int? TargetKcal,
        int? TargetProteinG,
        int? TargetCarbsG,
        int? TargetFatG);

    public sealed record UpdateUserProfileRequest(
        string? DisplayName,
        float? WeightKg,
        int? HeightCm,
        int? Age,
        string? Sex,
        string? ActivityLevel,
        int? TargetKcal,
        int? TargetProteinG,
        int? TargetCarbsG,
        int? TargetFatG);

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> Get(CancellationToken ct)
    {
        var uid = User.RequireUid();
        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.Uid == uid, ct);
        if (profile == null)
        {
            profile = new UserProfile
            {
                Uid   = uid,
                Email = User.FindFirst("email")?.Value,
            };
            _db.UserProfiles.Add(profile);
            await _db.SaveChangesAsync(ct);
        }
        return Ok(ToDto(profile));
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> Put(UpdateUserProfileRequest req, CancellationToken ct)
    {
        var uid = User.RequireUid();
        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.Uid == uid, ct);
        if (profile == null)
        {
            profile = new UserProfile { Uid = uid, Email = User.FindFirst("email")?.Value };
            _db.UserProfiles.Add(profile);
        }

        profile.DisplayName    = req.DisplayName    ?? profile.DisplayName;
        profile.WeightKg       = req.WeightKg       ?? profile.WeightKg;
        profile.HeightCm       = req.HeightCm       ?? profile.HeightCm;
        profile.Age            = req.Age            ?? profile.Age;
        profile.Sex            = req.Sex            ?? profile.Sex;
        profile.ActivityLevel  = req.ActivityLevel  ?? profile.ActivityLevel;
        profile.TargetKcal     = req.TargetKcal     ?? profile.TargetKcal;
        profile.TargetProteinG = req.TargetProteinG ?? profile.TargetProteinG;
        profile.TargetCarbsG   = req.TargetCarbsG   ?? profile.TargetCarbsG;
        profile.TargetFatG     = req.TargetFatG     ?? profile.TargetFatG;
        profile.UpdatedAt      = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Ok(ToDto(profile));
    }

    private static UserProfileDto ToDto(UserProfile p) => new(
        p.Uid, p.DisplayName, p.Email, p.WeightKg, p.HeightCm, p.Age, p.Sex, p.ActivityLevel,
        p.TargetKcal, p.TargetProteinG, p.TargetCarbsG, p.TargetFatG);
}
