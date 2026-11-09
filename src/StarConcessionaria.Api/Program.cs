using Scalar.AspNetCore;
using StarConcessionaria.Api.Extensions;
using StarConcessionaria.Application;
using StarConcessionaria.Infrastructure;
using StarConcessionaria.Infrastructure.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Mapeia OpenAPI e Scalar UI
app.MapOpenApi();
app.MapScalarApiReference();

// Redireciona a raiz "/" para a interface interativa do Scalar
app.MapGet("/", () => Results.Redirect("/scalar/v1"));

// Semeia o administrador padrao tanto em Development quanto caso solicitado via variavel
await app.Services.SeedAdminAsync(app.Configuration);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
