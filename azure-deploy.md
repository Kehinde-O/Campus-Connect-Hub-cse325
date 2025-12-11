# Azure Deployment Guide

This guide provides instructions for deploying the Campus Connect Hub application to Microsoft Azure.

## Prerequisites

- Azure subscription
- Azure CLI installed
- PostgreSQL database (Azure Database for PostgreSQL or local)
- .NET 8 SDK installed

## Architecture

- **Frontend**: Azure Static Web Apps (Blazor WebAssembly)
- **Backend**: Azure App Service (Linux) - ASP.NET Core Web API
- **Database**: Azure Database for PostgreSQL

## Deployment Steps

### 1. Database Setup

1. Create an Azure Database for PostgreSQL server
2. Create a database named `campusconnecthub`
3. Note the connection string

### 2. Backend API Deployment

1. Create an Azure App Service (Linux) plan
2. Create a Web App for the API
3. Configure the connection string in App Settings:
   - `ConnectionStrings:DefaultConnection` = Your PostgreSQL connection string
   - `JwtSettings:SecretKey` = A secure random key (at least 32 characters)
   - `JwtSettings:Issuer` = CampusConnectHub
   - `JwtSettings:Audience` = CampusConnectHub
   - `JwtSettings:ExpirationInMinutes` = 60

4. Enable CORS for the Static Web App URL

5. Deploy using:
   ```bash
   cd CampusConnectHub.Server
   dotnet publish -c Release
   # Use Azure CLI or VS Code Azure extension to deploy
   ```

### 3. Frontend Deployment

1. Create an Azure Static Web App
2. Configure the build settings:
   - App location: `CampusConnectHub.Client`
   - Output location: `wwwroot`
   - API location: (leave empty)

3. Update the API base URL in `CampusConnectHub.Client/Program.cs` to point to your deployed API

4. Deploy using GitHub Actions (configured in `.github/workflows/azure-static-web-apps.yml`) or manually

### 4. Environment Configuration

#### Backend (App Service Configuration)

Add these Application Settings:
- `ASPNETCORE_ENVIRONMENT` = Production
- `ConnectionStrings__DefaultConnection` = Your PostgreSQL connection string
- `JwtSettings__SecretKey` = Your secret key
- `JwtSettings__Issuer` = CampusConnectHub
- `JwtSettings__Audience` = CampusConnectHub
- `JwtSettings__ExpirationInMinutes` = 60

#### Frontend (Static Web App Configuration)

Update `CampusConnectHub.Client/Program.cs`:
```csharp
var apiBaseUrl = builder.HostEnvironment.IsDevelopment() 
    ? "https://localhost:7126" 
    : "https://your-api-app.azurewebsites.net";
```

### 5. CORS Configuration

In the backend `Program.cs`, update CORS to allow your Static Web App URL:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://your-static-web-app.azurestaticapps.net")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

## Testing

1. Access the Static Web App URL
2. Register a new account
3. Test all features:
   - News feed
   - Events and RSVP
   - Resources
   - Admin dashboard (login as admin@campus.edu / Admin123!)

## Troubleshooting

- Check Application Insights for backend errors
- Verify connection strings are correct
- Ensure CORS is properly configured
- Check that the database migrations have run

## Security Notes

- Use Azure Key Vault for sensitive configuration
- Enable HTTPS only
- Configure firewall rules for PostgreSQL
- Use managed identity where possible
- Rotate JWT secret keys regularly

