# Single App Service Setup - Complete Guide

## Configuration Complete!

Your application is now configured for **Option 2: Single App Service** deployment.

### Your Azure Domain
**App Service URL**: `https://campus-connect-hub-akhrhqcrczggfdgk.canadacentral-01.azurewebsites.net`

Both frontend and backend will be accessible from this single domain:
- **Frontend**: `https://campus-connect-hub-akhrhqcrczggfdgk.canadacentral-01.azurewebsites.net/`
- **API**: `https://campus-connect-hub-akhrhqcrczggfdgk.canadacentral-01.azurewebsites.net/api/...`
- **Swagger**: `https://campus-connect-hub-akhrhqcrczggfdgk.canadacentral-01.azurewebsites.net/swagger`

## What Was Changed

### 1. Server Configuration (`CampusConnectHub.Server/Program.cs`)
- Added static file serving: `app.UseDefaultFiles()` and `app.UseStaticFiles()`
- Added SPA fallback: `app.MapFallbackToFile("index.html")` for client-side routing
- Simplified CORS (same domain, no CORS issues)

### 2. Client Configuration (`CampusConnectHub.Client/Program.cs`)
- Updated to use same domain for API calls in production
- Removed separate API URL configuration

### 3. Deployment Workflow (`.github/workflows/main_campus-connect-hub.yml`)
- Builds both Client and Server projects
- Publishes Client to get `wwwroot` folder
- Publishes Server
- Copies Client's `wwwroot` to Server's `wwwroot`
- Deploys combined package to App Service

## How It Works

### Build Process:
1. **Build Client**: Creates `client-publish/wwwroot/` with Blazor WebAssembly files
2. **Build Server**: Creates `server-publish/` with API and runtime
3. **Combine**: Copies `client-publish/wwwroot/` → `server-publish/wwwroot/`
4. **Deploy**: Uploads `server-publish/` to App Service

### Runtime:
- User visits: `https://campus-connect-hub-akhrhqcrczggfdgk.canadacentral-01.azurewebsites.net/`
- Server serves `index.html` from `wwwroot/`
- Blazor app loads and runs in browser
- API calls go to: `https://campus-connect-hub-akhrhqcrczggfdgk.canadacentral-01.azurewebsites.net/api/...`
- Same domain = No CORS issues!

## Deployment

### Automatic (via GitHub Actions)
When you push to `main` branch:
1. Workflow automatically builds both projects
2. Combines frontend and backend
3. Deploys to your App Service

### Manual Deployment
If you need to deploy manually:
```bash
# Build Client
dotnet publish CampusConnectHub.Client -c Release -o ./client-publish

# Build Server
dotnet publish CampusConnectHub.Server -c Release -o ./server-publish

# Copy Client wwwroot to Server
cp -r ./client-publish/wwwroot/* ./server-publish/wwwroot/

# Deploy server-publish folder to Azure
# (Use Azure CLI, VS Code extension, or Azure Portal)
```

## Azure App Service Configuration

### Required Application Settings:
Go to Azure Portal → App Service → Configuration → Application settings:

```
ASPNETCORE_ENVIRONMENT = Production
ConnectionStrings__DefaultConnection = <Your Neon PostgreSQL connection string>
JwtSettings__SecretKey = <Generate a secure 32+ character key>
JwtSettings__Issuer = CampusConnectHub
JwtSettings__Audience = CampusConnectHub
JwtSettings__ExpirationInMinutes = 60
```

### No CORS Configuration Needed!
Since frontend and backend are on the same domain, CORS is not required.

## Testing

1. **Access Application**: 
   `https://campus-connect-hub-akhrhqcrczggfdgk.canadacentral-01.azurewebsites.net/`

2. **Check API**: 
   `https://campus-connect-hub-akhrhqcrczggfdgk.canadacentral-01.azurewebsites.net/swagger`

3. **Test Login**:
   - Admin: `admin@campus.edu` / `Admin123!`
   - Student: `student@campus.edu` / `Student123!`

## Troubleshooting

### Frontend Not Loading
- Check that `wwwroot` folder exists in deployed package
- Verify `app.UseDefaultFiles()` and `app.UseStaticFiles()` are in Program.cs
- Check App Service logs in Azure Portal

### API Not Working
- Verify connection string is set correctly
- Check that database is accessible from Azure
- Review Application Insights for errors

### 404 Errors on Page Refresh
- Ensure `app.MapFallbackToFile("index.html")` is configured
- This handles client-side routing

## Benefits of This Setup

**Single Service**: One App Service to manage
**Same Domain**: No CORS configuration needed
**Simple Deployment**: One workflow deploys everything
**Cost Effective**: One service (though not free like Static Web Apps)

## Next Steps

1. **Push to GitHub**: Changes are ready to push
2. **Verify Workflow**: Check GitHub Actions tab after push
3. **Configure Azure**: Set application settings in Azure Portal
4. **Test**: Access your domain and verify everything works

Your application is now ready for single App Service deployment!

