using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Cargar configuración de rutas de Ocelot según el entorno
var ocelotConfig = File.Exists($"ocelot.{builder.Environment.EnvironmentName}.json")
    ? $"ocelot.{builder.Environment.EnvironmentName}.json"
    : "ocelot.json";
builder.Configuration.AddJsonFile(ocelotConfig, optional: false, reloadOnChange: true);

// Configuración de autenticación JWT Bearer en el Gateway
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"]
    ?? builder.Configuration["JWT_KEY"]
    ?? "SerpentarioVenomSecKey2026_UltraSecretKey32CharsMin!";
var issuer = jwtSettings["Issuer"] ?? builder.Configuration["JWT_ISSUER"] ?? "SerpentarioApi";
var audience = jwtSettings["Audience"] ?? builder.Configuration["JWT_AUDIENCE"] ?? "SerpentarioClients";

var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

// Registrar Ocelot en el contenedor de dependencias
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Activar Ocelot
await app.UseOcelot();

app.Run();
