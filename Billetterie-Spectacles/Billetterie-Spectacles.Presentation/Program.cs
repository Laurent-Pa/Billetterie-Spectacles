using Billetterie_Spectacles.Application.Helpers;
using Billetterie_Spectacles.Application.Interfaces;
using Billetterie_Spectacles.Application.Services.Implementations;
using Billetterie_Spectacles.Application.Services.Interfaces;
using Billetterie_Spectacles.Infrastructure.Data;
using Billetterie_Spectacles.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. Configuration de la base de données
// ============================================


String connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
// LOG TEMPORAIRE POUR DIAGNOSTIC
Console.WriteLine("===========================================");
Console.WriteLine($"CONNECTION STRING UTILISÉE : {connectionString}");
Console.WriteLine("===========================================");

// Enregistrement du DbContext dans le container d'injection de dépendances
// Configure EF Core pour utiliser SQL Server
builder.Services.AddDbContext<BilletterieDbContext>(options =>
    options.UseSqlServer(connectionString));

// LOG TEMPORAIRE POUR DIAGNOSTIC
Console.WriteLine("===========================================");
Console.WriteLine($"CONNECTION STRING UTILISÉE : {connectionString}");
Console.WriteLine("===========================================");

// Pour afficher le log des requetes faites par EF Core en console Visual Studio (aide au debug)
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);

// ============================================
// 2. Enregistrement des Repositories (pour l'injection de dépendance DI)
// ============================================

// Une instance par requête
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISpectacleRepository, SpectacleRepository>();
builder.Services.AddScoped<IPerformanceRepository, PerformanceRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();


// ============================================
// 3. Enregistrement des Services (pour l'injection de dépendance DI)
// ============================================
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ISpectacleService, SpectacleService>();
builder.Services.AddScoped<IPerformanceService, PerformanceService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, MockPaymentService>();
// builder.Services.AddScoped<IPaymentService, StripePaymentService>();

// ============================================
// 4. Configuration de l'authentification JWT (Json Web Token)
// ============================================
builder.Services.AddScoped<JwtTokenHelper>();

IConfigurationSection jwtSettings = builder.Configuration.GetSection("JwtSettings");
string secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero  // Pas de tolérance sur l'expiration
    };
});

builder.Services.AddAuthorization();

// ============================================
// 5. Configuration des Controllers
// ============================================
builder.Services.AddControllers();


// ============================================
// 6. Configuration de Swagger avec Swashbuckle (documentation API)
// ============================================
// Version permettant de gérer l'authentification (présence du cadena pour Bearer Token)
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Billetterie Spectacles API",
        Version = "v1"
    });

    // Configuration JWT dans Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme.",
        In = ParameterLocation.Header,
        Name = "Authorization"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ============================================
// 7. Configuration CORS (si frontend séparé)
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")  // React, Vue, etc.
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


var app = builder.Build();


// ============================================
// 8. Configuration du pipeline HTTP
// ============================================
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();         // Désactivé, OpenApi ne gère pas l'authentification par Token pour l'instant
    app.UseSwagger();           // On utilise SwashBuckle à la place d'OpenApi en attendant que l'authent soit résolue (déc 2025)

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Billetterie API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowFrontend");   // Pour pouvoir travailler avec une appli front en local

app.UseAuthentication();        // Authentification avant de vérifier l'autorisation

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
