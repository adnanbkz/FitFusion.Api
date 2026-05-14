using System.Text.Json;
using System.Text.Json.Serialization;
using FitFusion.Api.Data;
using FitFusion.Api.Models.Ai;
using FitFusion.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// JSON: camelCase + omit null. Casa con kotlinx.serialization del cliente Android.
// -----------------------------------------------------------------------------
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// -----------------------------------------------------------------------------
// Swagger con soporte para "Authorize" con Bearer.
// -----------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FitFusion.Api", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = SecuritySchemeType.Http,
        Scheme      = "bearer",
        BearerFormat = "JWT",
        In          = ParameterLocation.Header,
        Description = "Firebase ID Token",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// -----------------------------------------------------------------------------
// EF Core + SQLite
// -----------------------------------------------------------------------------
builder.Services.AddDbContext<FitFusionDbContext>(opts =>
    opts.UseSqlite(builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=fitfusion.db"));

// -----------------------------------------------------------------------------
// Servicios
// -----------------------------------------------------------------------------
builder.Services.AddHttpClient<IGeminiClient, GeminiClient>();
builder.Services.AddScoped<AiService>();
builder.Services.AddSingleton<MetCalculator>();

// -----------------------------------------------------------------------------
// Auth: Firebase ID Token.
// JwtBearer descarga las claves públicas de Google de Authority/.well-known/...
// automáticamente y valida iss/aud/exp/firma.
// -----------------------------------------------------------------------------
var firebaseProjectId = builder.Configuration["Firebase:ProjectId"]
    ?? throw new InvalidOperationException("Firebase:ProjectId no está configurado");
var issuer = $"https://securetoken.google.com/{firebaseProjectId}";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.Authority             = issuer;
        o.RequireHttpsMetadata  = !builder.Environment.IsDevelopment();
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer   = true,
            ValidIssuer      = issuer,
            ValidateAudience = true,
            ValidAudience    = firebaseProjectId,
            ValidateLifetime = true,
            ClockSkew        = TimeSpan.FromMinutes(2),
        };
        o.Events = new JwtBearerEvents
        {
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode  = StatusCodes.Status401Unauthorized;
                ctx.Response.ContentType = "application/json";
                var msg = string.IsNullOrEmpty(ctx.ErrorDescription) ? "No autenticado" : ctx.ErrorDescription;
                return ctx.Response.WriteAsJsonAsync(new ErrorResponse(msg));
            },
            OnForbidden = ctx =>
            {
                ctx.Response.StatusCode  = StatusCodes.Status403Forbidden;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsJsonAsync(new ErrorResponse("Acceso denegado"));
            },
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// -----------------------------------------------------------------------------
// Pipeline
// -----------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Convierte excepciones no controladas en {"error":"..."} para que cuadre con el
// envelope que espera el cliente Android.
app.UseExceptionHandler(branch => branch.Run(async ctx =>
{
    var feature = ctx.Features.Get<IExceptionHandlerFeature>();
    var msg = feature?.Error.Message ?? "Error interno";
    ctx.Response.StatusCode  = StatusCodes.Status500InternalServerError;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsJsonAsync(new ErrorResponse(msg));
}));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Aplica migraciones pendientes al arrancar (dev). En prod normalmente se hace
// con un job aparte, pero con SQLite local da igual.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FitFusionDbContext>();
    db.Database.Migrate();
    var seedLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DishSeeder");
    await DishSeeder.SeedAsync(db, seedLogger);
}

app.Run();
