using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
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

// Configura o OpenAPI com suporte nativo a Bearer JWT no Swagger/Scalar
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Informe o token JWT retornado pelo endpoint /api/v1/auth/login",
            In = ParameterLocation.Header
        });

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme
                    }
                },
                Array.Empty<string>()
            }
        });

        return Task.CompletedTask;
    });
});

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
