using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StarConcessionaria.Api.Middlewares;
using StarConcessionaria.Application.Interfaces;
using StarConcessionaria.Application.Services;
using StarConcessionaria.Infrastructure.Persistence;
using StarConcessionaria.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar DbContext (EF Core com PostgreSQL ou In-Memory para testes rápidos caso não haja container conectado)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=bd_star_concessionaria;Username=star_admin;Password=star_password_secret";

builder.Services.AddDbContext<StarConcessionariaDbContext>(options =>
{
    // Se configurado para PostgreSQL, usa Npgsql; caso contrário, suporta fallback
    try
    {
        options.UseNpgsql(connectionString);
    }
    catch
    {
        options.UseInMemoryDatabase("StarConcessionariaFallbackDb");
    }
});

// 2. Injeção de Dependências
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUnidadeService, UnidadeService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();
builder.Services.AddScoped<IFaqService, FaqService>();

// 3. Autenticação JWT
var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "StarConcessionaria_Super_Secret_Key_2026_Minimum_32_Chars";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "StarConcessionariaApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "StarConcessionariaWeb";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// 4. Pipeline HTTP
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

// Healthcheck
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.MapControllers();

// Auto-seed no ambiente de desenvolvimento se o banco estiver acessível
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<StarConcessionariaDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    if (db.Database.CanConnect())
    {
        await DatabaseSeeder.SeedAsync(db, hasher);
    }
}
catch
{
    // Ignora se o container PostgreSQL ainda não foi inicializado no ambiente local
}

app.Run();

public partial class Program { }
