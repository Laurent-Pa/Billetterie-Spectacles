using Billetterie_Spectacles.Application.Helpers;
using Billetterie_Spectacles.Application.Interfaces;
using Billetterie_Spectacles.Application.Services.Implementations;
using Billetterie_Spectacles.Application.Services.Interfaces;
using Billetterie_Spectacles.Infrastructure.Data;
using Billetterie_Spectacles.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. Configuration de la base de donn�es
// ============================================

// Configuration de la connexion � la base de donn�es
String connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

// Enregistrement du DbContext dans le container d'injection de d�pendances
// Configure EF Core pour utiliser SQLite (pour macOS) ou SQL Server (pour Windows)
// Détection automatique : SQLite si "Data Source=" sans "Server=", sinon SQL Server
if (connectionString.Contains("Data Source=") && !connectionString.Contains("Server="))
{
    // SQLite
    builder.Services.AddDbContext<BilletterieDbContext>(options =>
        options.UseSqlite(connectionString));
}
else
{
    // SQL Server
    builder.Services.AddDbContext<BilletterieDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// Pour afficher le log des requetes faites par EF Core en console Visual Studio (aide au debug)
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);

// ============================================
// 2. Enregistrement des Repositories (pour l'injection de d�pendance DI)
// ============================================

// Une instance par requ�te
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISpectacleRepository, SpectacleRepository>();
builder.Services.AddScoped<IPerformanceRepository, PerformanceRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();


// ============================================
// 3. Enregistrement des Services (pour l'injection de d�pendance DI)
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
        ClockSkew = TimeSpan.Zero  // Pas de tol�rance sur l'expiration
    };
});

builder.Services.AddAuthorization();

// ============================================
// 5. Configuration des Controllers
// ============================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configuration pour sérialiser en camelCase (compatible avec JavaScript/TypeScript)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });


// ============================================
// 6. Configuration de Swagger avec Swashbuckle (documentation API)
// ============================================
// Version permettant de g�rer l'authentification (pr�sence du cadena pour Bearer Token)
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Billetterie Spectacles API",
        Version = "v1",
        Description = "API de gestion de billetterie pour spectacles",
        Contact = new OpenApiContact
        {
            Name = "Laurent",
            Email = "laurent@example.com"
        }
    });

    // Configuration JWT dans Swagger
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});


// ============================================
// 7. Configuration CORS (si frontend s�par�)
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
    //app.MapOpenApi();         // D�sactiv�, OpenApi ne g�re pas l'authentification par Token pour l'instant
    app.UseSwagger();           // On utilise SwashBuckle � la place d'OpenApi en attendant que l'authent soit r�solue (d�c 2025)

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Billetterie API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowFrontend");   // Pour pouvoir travailler avec une appli front en local

app.UseAuthentication();        // Authentification avant de v�rifier l'autorisation

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// ============================================
// 9. Initialisation de la base de données (Seeding)
// ============================================
// Seeding désactivé - décommenter pour réactiver
/*
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<BilletterieDbContext>();
    
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Démarrage du seeding de la base de données...");
        await DatabaseSeeder.SeedAsync(context);
        logger.LogInformation("Seeding de la base de données terminé avec succès.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Une erreur s'est produite lors du seeding de la base de données.");
    }
}
*/

app.Run();
