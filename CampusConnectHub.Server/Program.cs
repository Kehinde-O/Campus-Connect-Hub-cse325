using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CampusConnectHub.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS for Blazor WebAssembly
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
        
        if (allowedOrigins == null || allowedOrigins.Length == 0)
        {
            if (builder.Environment.IsDevelopment())
            {
                // Default to localhost for development
                allowedOrigins = new[] { "https://localhost:5001", "http://localhost:5121", "https://localhost:7126" };
            }
            else
            {
                // In production, get from environment variable or configuration
                // Example: https://your-static-web-app.azurestaticapps.net
                var staticWebAppUrl = builder.Configuration["StaticWebAppUrl"] 
                    ?? Environment.GetEnvironmentVariable("STATIC_WEB_APP_URL");
                
                if (!string.IsNullOrEmpty(staticWebAppUrl))
                {
                    allowedOrigins = new[] { staticWebAppUrl };
                }
                else
                {
                    // Fallback - allow all origins in production (not recommended for production)
                    // You should set StaticWebAppUrl in App Service configuration
                    allowedOrigins = new[] { "*" };
                }
            }
        }
        
        if (allowedOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// Configure Entity Framework with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=ep-still-frog-a42kugyk-pooler.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_j4kwNiLaV9CI;SSL Mode=Require";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "CampusConnectHub",
        ValidAudience = jwtSettings["Audience"] ?? "CampusConnectHub",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<CampusConnectHub.Server.Services.JwtService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
    CampusConnectHub.Infrastructure.Data.DatabaseSeeder.Seed(dbContext);
}

app.Run();
