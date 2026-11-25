using System.Text;
using _13Serpentarios.Datos.Models;
using _13Serpentarios.Datos.Repositorios;
using _13Serpentarios.Negocio.Mappings;
using _13Serpentarios.Negocio.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Controladores
builder.Services.AddControllers();

// Base de datos (Entity Framework Core + SQL Server)
builder.Services.AddDbContext<SerpentarioContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositorios (Capa de Datos)
builder.Services.AddScoped<IEspecieSerpienteRepositorio, EspecieSerpienteRepositorio>();
builder.Services.AddScoped<ILoteVenenoRepositorio, LoteVenenoRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

// Servicios (Capa de Negocio)
builder.Services.AddScoped<IEspecieSerpienteServicio, EspecieSerpienteServicio>();
builder.Services.AddScoped<ILoteVenenoServicio, LoteVenenoServicio>();
builder.Services.AddScoped<IAuthServicio, AuthServicio>();

// AutoMapper (único MappingProfile en Negocio)
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);

// Autenticación con JWT Bearer
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]
    ?? throw new InvalidOperationException("La clave secreta JWT no está configurada."));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// OpenAPI con botón Authorize (esquema Bearer)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Ingresa tu token JWT para autenticarte (ejemplo: Bearer {token})"
        };
        document.Components.SecuritySchemes["Bearer"] = scheme;

        var requirement = new OpenApiSecurityRequirement();
        requirement.Add(new OpenApiSecuritySchemeReference("Bearer", document), new List<string>());
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(requirement);

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Swagger / OpenAPI UI
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "13Serpentarios - Banco de Venenos API v1");
});

app.UseHttpsRedirection();

// El middleware de autenticación SIEMPRE debe ir antes del de autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Redirección de la raíz al Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
