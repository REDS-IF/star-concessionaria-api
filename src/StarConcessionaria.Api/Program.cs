using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using StarConcessionaria.Api.Extensions;
using StarConcessionaria.Application;
using StarConcessionaria.Infrastructure;
using StarConcessionaria.Infrastructure.Auth;
using StarConcessionaria.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddControllers();

// Habilita CORS amplo para desenvolvimento/testes e ferramentas de documentacao
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Suporte a reverse proxy do Render (Cloudflare/Nginx -> Kestrel)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseForwardedHeaders();

app.UseCors("AllowAll");

// Mapeia OpenAPI e Scalar UI
app.MapOpenApi();
app.MapScalarApiReference();

// Redireciona a raiz "/" para a interface interativa do Scalar
app.MapGet("/", () => Results.Redirect("/scalar/v1"));

// Semeia o administrador inicial a partir das variaveis de ambiente
await app.Services.SeedAdminAsync(app.Configuration);

// Executa o seed inicial de dados do catalogo (Lojas, Veiculos, Pecas, Clientes, Contatos)
// Possui checagem idempotente interna: se ja existirem registros, nao executa novamente.
await app.Services.SeedAsync();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
