using System.Text;
using LaboratoriosApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configuración de JWT sincronizada con Microservicio 1 (S14) y Gateway
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"]
    ?? builder.Configuration["JWT_KEY"]
    ?? "SerpentarioVenomSecKey2026_UltraSecretKey32CharsMin!";
var issuer = jwtSettings["Issuer"] ?? builder.Configuration["JWT_ISSUER"] ?? "SerpentarioApi";
var audience = jwtSettings["Audience"] ?? builder.Configuration["JWT_AUDIENCE"] ?? "SerpentarioClients";

var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Datos en memoria (sin base de datos)
var laboratorios = new List<Laboratorio>
{
    new()
    {
        Id = 1,
        Nombre = "Instituto Butantan - Alianza Andina",
        Ruc = "20489123001",
        Pais = "Brasil / Perú",
        Especialidad = "Producción de Sueros Antiofídicos Polivalentes Bothrópicos",
        CorreoContacto = "investigacion@butantan-andina.org",
        Telefono = "+51 1 456-7890",
        ActivoConvenio = true
    },
    new()
    {
        Id = 2,
        Nombre = "Instituto Clodomiro Picado",
        Ruc = "20599182731",
        Pais = "Costa Rica",
        Especialidad = "Purificación de Inmunoglobulinas y Neutralización de Veneno Lachesis",
        CorreoContacto = "convenios@icp.ucr.ac.cr",
        Telefono = "+506 2511-7888",
        ActivoConvenio = true
    },
    new()
    {
        Id = 3,
        Nombre = "Laboratorios VenoTech Pharma",
        Ruc = "20601928374",
        Pais = "Perú",
        Especialidad = "Fraccionamiento de Péptidos Neurotóxicos Micrurus (Corales)",
        CorreoContacto = "contacto@venotech.pe",
        Telefono = "+51 1 789-1234",
        ActivoConvenio = true
    },
    new()
    {
        Id = 4,
        Nombre = "Centro Biofarma SnakeVenom Latam",
        Ruc = "20102938475",
        Pais = "Colombia",
        Especialidad = "Ensayos Preclínicos de Coagulotoxicidad Crotálica",
        CorreoContacto = "lab@snakevenom-latam.co",
        Telefono = "+57 601 321-4567",
        ActivoConvenio = false
    }
};

// Endpoint 1: Listado de laboratorios (exige autenticación JWT)
app.MapGet("/api/laboratorios", () => Results.Ok(laboratorios))
   .RequireAuthorization();

// Endpoint 2: Consulta por Id (exige autenticación JWT)
app.MapGet("/api/laboratorios/{id:int}", (int id) =>
{
    var lab = laboratorios.FirstOrDefault(l => l.Id == id);
    return lab is not null
        ? Results.Ok(lab)
        : Results.NotFound(new { mensaje = $"Laboratorio con ID {id} no encontrado en el sistema." });
}).RequireAuthorization();

app.Run();
