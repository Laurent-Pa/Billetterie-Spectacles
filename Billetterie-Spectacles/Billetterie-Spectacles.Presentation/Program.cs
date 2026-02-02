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
// 1. Configuration de la base de donn�es
// ============================================
// test

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
// LOG TEMPORAIRE POUR DIAGNOSTIC
Console.WriteLine("===========================================");
Console.WriteLine($"CONNECTION STRING UTILIS�E : {connectionString}");
Console.WriteLine("===========================================");

// Enregistrement du DbContext dans le container d'injection de d�pendances
// Configure EF Core pour utiliser SQL Server
bool useSqlite = connectionString.TrimStart().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase)
    || connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<BilletterieDbContext>(options =>
{
    if (useSqlite)
        options.UseSqlite(connectionString);
    else
        options.UseSqlServer(connectionString);
});

Console.WriteLine("===========================================");
Console.WriteLine($"Base de données : {(useSqlite ? "SQLite" : "SQL Server")}");
Console.WriteLine("===========================================");

// LOG TEMPORAIRE POUR DIAGNOSTIC (supprimé - affichage ci-dessus - bloc supprimé - supprimé)

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
builder.Services.AddScoped<IEmailService, EmailService>();
//builder.Services.AddScoped<IPaymentService, MockPaymentService>();
// builder.Services.AddScoped<IPaymentService, StripePaymentService>();
builder.Services.AddHttpClient<IPaymentHttpService, PaymentHttpService>(client =>
{
    var paymentServiceUrl = builder.Configuration["PaymentService:BaseUrl"]
        ?? "https://localhost:7049"; //  URL locale par d�faut

    client.BaseAddress = new Uri(paymentServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

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
builder.Services.AddControllers();


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
// 8. Application des migrations et seeding
// ============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BilletterieDbContext>();

        // Appliquer automatiquement les migrations au d�marrage
        context.Database.Migrate();
        Console.WriteLine("Migrations appliqu�es avec succ�s");

        // Seeder les donn�es de test
        DatabaseSeeder.Seed(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Une erreur s'est produite lors de la migration ou du seeding de la base de donn�es");
    }
}


// ============================================
// 9. Configuration du pipeline HTTP
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

app.Run();
