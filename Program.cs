using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using AsylumVagasAPI.Interfaces;
using AsylumVagasAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ─── DAPPER: convenções snake_case → PascalCase ───────────────────────────────
// Ativa o mapeamento automático para que as colunas do PostgreSQL (ex: asilo_id) 
// preencham corretamente as propriedades C# (ex: AsiloId).
DefaultTypeMap.MatchNamesWithUnderscores = true;

// ─── CONTROLLERS + JSON ───────────────────────────────────────────────────────
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Garante camelCase no JSON (ex: "nomeAsilo") para facilitar o uso no Angular
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        // Mantém o JSON limpo removendo campos nulos
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        // Transforma Enums em texto legível no JSON
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ─── INJEÇÃO DE DEPENDÊNCIA ────────────────────────────────────────────────────
builder.Services.AddScoped<IVagaRepository, VagaRepository>();

// ─── CORS ─────────────────────────────────────────────────────────────────────
const string CorsPolicyName = "AngularFrontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Permite qualquer origem local
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); 
    });
});

// ─── SWAGGER / OPENAPI ────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "Gestão de Vagas de Asilos — Criciúma/SC",
        Version     = "v1",
        Description = "API RESTful para gerenciamento de vagas em instituições de longa permanência."
    });
});

// ─── HEALTH CHECK ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

// envirement
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// ─── PIPELINE HTTP ────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vagas API v1");
        c.RoutePrefix = "swagger"; // Acessível em http://localhost:PORTA/swagger
    });
}

// O CORS deve ser aplicado ANTES da autorização e dos controllers
app.UseCors(CorsPolicyName);

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();