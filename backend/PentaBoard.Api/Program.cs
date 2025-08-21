using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using PentaBoard.Api.Infrastructure;                     // AppDbContext
using PentaBoard.Api.Features.Authentication.Common;     // AddJwtAuth extension
using MediatR;
using PentaBoard.Api.Features.Authentication.LoginUser; 
// .env dosyasını yükle (.env -> Environment Variables)
Env.Load();

var builder = WebApplication.CreateBuilder(args);
// Ortam değişkenlerini konfige ekle
builder.Configuration.AddEnvironmentVariables();

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS: frontend originlerini izinli yap
const string CorsPolicy = "frontend";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(CorsPolicy, p =>
        p.WithOrigins(
            "http://localhost:3000", // CRA
            "http://localhost:5173"  // Vite
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        // .AllowCredentials() // cookie tabanlı auth kullanırsan aç
    );
});
// ✅ MediatR: handler’ları kaydet
builder.Services.AddMediatR(cfg =>
{
    // Proje tek assembly olduğu için handler'ın bulunduğu assembly'den tara
    cfg.RegisterServicesFromAssemblyContaining<LoginUserHandler>();
});
// JWT kurulumunu feature içindeki extension yapıyor
builder.Services.AddJwtAuth(builder.Configuration);

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Dev'de HTTP kullanıyorsun; https'e zorlamayı prod'a bırak
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicy);     // ⬅️ CORS'u auth/route'lardan ÖNCE koy
app.UseAuthentication();     // JWT middleware
app.UseAuthorization();

app.MapControllers();

app.Run();
