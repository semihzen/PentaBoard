using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using DotNetEnv;
using PentaBoard.Api.Infrastructure;                         // AppDbContext
using PentaBoard.Api.Features.Authentication.Common;         // AddJwtAuth()
using PentaBoard.Api.Features.Authentication.LoginUser;      // MediatR assembly
using PentaBoard.Api.Infrastructure.Email;                   // IEmailSender, SmtpOptions
using PentaBoard.Api.Infrastructure.Security;                // IPasswordHasher
using PentaBoard.Api.Features.Projects;
using PentaBoard.Api.Features.Projects.GetProjects;
using PentaBoard.Api.Features.Projects.CreateProject;
using PentaBoard.Api.Features.Projects.DeleteProject;
// 1) .env -> Environment Variables
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// 2) appsettings + Environment Variables
builder.Configuration.AddEnvironmentVariables();

// 3) Services
builder.Services.AddControllers();

// Swagger + Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PentaBoard API", Version = "v1" });
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = new List<string>() });
});

// SMTP (Smtp__* env/appsettings)
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

// Password Hasher
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

// DbContext
var connStr =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"]
    ?? builder.Configuration["ConnectionStrings__DefaultConnection"]
    ?? builder.Configuration["DefaultConnection"];

if (string.IsNullOrWhiteSpace(connStr))
    throw new InvalidOperationException("Connection string 'DefaultConnection' bulunamadı.");

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connStr));

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// CORS (yalnızca frontend origin’lerini ekle)
const string CorsPolicy = "frontend";
var configuredOrigins = (builder.Configuration["Cors:Origins"] ?? "")
    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
var defaultOrigins = new[] { "http://localhost:3000", "http://localhost:5173" };
var allowedOrigins = configuredOrigins.Length > 0 ? configuredOrigins : defaultOrigins;

builder.Services.AddCors(opt =>
{
    opt.AddPolicy(CorsPolicy, p => p
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<LoginUserHandler>();
});

// JWT
builder.Services.AddJwtAuth(builder.Configuration);

// Admin policy (rol isimlerindeki küçük/büyük farkını tolere et)
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("AdminOnly", p => p.RequireRole("Admin", "System Admin", "admin"));
});

// 4) Pipeline
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapCreateProjectEndpoint();
app.MapGetProjectsEndpoint();
app.MapControllers();
app.MapDeleteProjectEndpoint();
app.Run();
