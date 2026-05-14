using FitFusion.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitFusion.Api.Data;

public class FitFusionDbContext : DbContext
{
    public FitFusionDbContext(DbContextOptions<FitFusionDbContext> options) : base(options) { }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<Dish> Dishes => Set<Dish>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<UserProfile>(e =>
        {
            e.Property(x => x.Uid).HasMaxLength(128);
            e.HasIndex(x => x.Email);
        });

        b.Entity<Workout>(e =>
        {
            e.Property(x => x.OwnerUid).HasMaxLength(128).IsRequired();
            e.HasIndex(x => new { x.OwnerUid, x.PerformedAt });
            e.HasMany(x => x.Exercises)
                .WithOne(x => x.Workout)
                .HasForeignKey(x => x.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<WorkoutExercise>(e =>
        {
            e.HasIndex(x => x.WorkoutId);
        });

        b.Entity<Dish>(e =>
        {
            e.Property(x => x.Id).HasMaxLength(96);
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.Description).HasMaxLength(160);
            e.Property(x => x.SuitableSlots).HasMaxLength(96).IsRequired();
            e.Property(x => x.Tags).HasMaxLength(256).IsRequired();
            e.HasIndex(x => x.Name);
        });

        base.OnModelCreating(b);
    }
}
