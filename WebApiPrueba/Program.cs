using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebApiPrueba.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<EmpresaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();

builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);
var secret = builder.Configuration.GetValue<string>("settings:secretkey")
    ?? builder.Configuration.GetValue<string>("settings:secretKey") // por si está con K mayúscula
    ?? throw new InvalidOperationException("Falta settings:secretkey en la configuración.");

if (Encoding.UTF8.GetByteCount(secret) < 33) // > 256 bits
    throw new InvalidOperationException("settings:secretkey debe tener al menos 33 bytes (sugerido 64).");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
builder.Services.AddSingleton(signingKey);

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,   
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

builder.Services.AddAuthorization(options =>
{
   // Política general (Admin y Operador autenticados)
    options.AddPolicy("AdminUOperador", p => p.RequireRole("Administrador", "Operador"));

    // Políticas específicas
    options.AddPolicy("SoloAdmin", p => p.RequireRole("Administrador"));
    options.AddPolicy("PuedeEliminar", p => p.RequireRole("Administrador"));
    options.AddPolicy("PuedeVerReportes", p => p.RequireRole("Administrador"));

    // Fallback: exige token para toda la API (salvo [AllowAnonymous])
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WebApiPrueba",
        Version = "v1"
    });

    // Configuración para JWT Bearer en Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce el token JWT en el campo **Authorization** con el prefijo **Bearer**.\n\nEjemplo: `Bearer eyJhbGciOiJIUzI1NiIs...`"
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
            Array.Empty<string>()
        }
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
