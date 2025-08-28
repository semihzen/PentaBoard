using DotNetEnv;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using PentaBoard.Api.Features.Authentication.Common;      // AddJwtAuth()
using PentaBoard.Api.Features.Authentication.LoginUser;   // MediatR assembly marker
using PentaBoard.Api.Infrastructure;                      // AppDbContext
using PentaBoard.Api.Infrastructure.Email;                // IEmailSender, SmtpEmailSender, SmtpOptions
using PentaBoard.Api.Infrastructure.Security;             // IPasswordHasher, BcryptPasswordHasher

// Projects
using PentaBoard.Api.Features.Projects.GetProjects;
using PentaBoard.Api.Features.Projects.CreateProject;
using PentaBoard.Api.Features.Projects.DeleteProject;
using PentaBoard.Api.Features.Projects.UpdateProject;

// Members
using PentaBoard.Api.Features.Members.AddProjectMember;
using PentaBoard.Api.Features.Members.GetMember;
using PentaBoard.Api.Features.Members.RemoveProjectMember;
using PentaBoard.Api.Features.Members.SetMemberRole;

// Boards
using PentaBoard.Api.Features.Boards.GetBoard;
using PentaBoard.Api.Features.Boards.AddBoardColumn;
using PentaBoard.Api.Features.Boards.MoveBoardColumn;
using PentaBoard.Api.Features.Boards.RenameBoardColumn;
using PentaBoard.Api.Features.Boards.DeleteBoardColumn;

// WorkItems
using PentaBoard.Api.Features.WorkItems.Create;
using PentaBoard.Api.Features.WorkItems.Delete;
using PentaBoard.Api.Features.WorkItems.Move;

var builder = WebApplication.CreateBuilder(args);

// 1) .env
Env.Load();

// 2) extra configuration
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

// SMTP
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

// CORS
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

// MediatR (handler’ları bu assembly’den tara)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<LoginUserHandler>();
});

// JWT
builder.Services.AddJwtAuth(builder.Configuration);

// Authorization
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
else
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

// ---- Endpoints ----

// Projects
app.MapCreateProjectEndpoint();
app.MapGetProjectsEndpoint();
app.MapDeleteProjectEndpoint();
app.MapUpdateProjectEndpoint();

// Members
AddProjectMemberEndpoint.Map(app);
app.MapGetProjectMembers();
RemoveProjectMemberEndpoint.Map(app);
SetMemberRoleEndpoint.Map(app);

// Boards
app.MapGetBoard();
app.MapAddBoardColumn();
app.MapMoveBoardColumn();
app.MapRenameBoardColumn();
app.MapDeleteBoardColumn();

// Work Items
app.MapCreateWorkItem();
app.MapDeleteWorkItem();
app.MapMoveWorkItemEndpoint();

// NOT: MoveWorkItem endpoint’i henüz extension olarak yoksa çağırma.
// Eğer daha sonra eklersen: app.MapMoveWorkItemEndpoint();

app.MapControllers();

app.Run();
