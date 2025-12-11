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

// Configure CORS - Allow all origins, headers, and methods
// This ensures no CORS errors regardless of where requests come from
builder.Services.AddCors(options =>
{
    // Default policy - Allow everything
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()      // Allow requests from any origin
              .AllowAnyHeader()      // Allow any HTTP headers (including Authorization, Content-Type, etc.)
              .AllowAnyMethod();     // Allow any HTTP methods (GET, POST, PUT, DELETE, OPTIONS, etc.)
    });
    
    // Named policy for explicit use
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
    
    // Policy with credentials for development
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins("https://localhost:5001", "http://localhost:5121", "https://localhost:7126", "http://localhost:5121")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
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

// Use CORS FIRST - before any other middleware
// This ensures preflight OPTIONS requests are handled correctly
app.UseCors();

app.UseHttpsRedirection();

// Serve static files from wwwroot (for Blazor WebAssembly frontend)
// MUST be before authentication/authorization to allow access to static files
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Map API controllers - these take precedence over static files for /api/* routes
app.MapControllers();

// Fallback to index.html for client-side routing (SPA)
// This must be last to catch all non-API routes and serve the Blazor app
app.MapFallbackToFile("index.html");

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
    CampusConnectHub.Infrastructure.Data.DatabaseSeeder.Seed(dbContext);
}

app.Run();
