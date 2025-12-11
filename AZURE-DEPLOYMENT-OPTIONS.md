# Azure Deployment Options - Campus Connect Hub

## Option 1: Two Separate Services (RECOMMENDED)

### Architecture
- **Frontend**: Azure Static Web Apps (FREE tier available)
- **Backend**: Azure App Service (Linux)

### What Goes Where

#### Frontend → Azure Static Web Apps
**Project Folder**: `CampusConnectHub.Client/`
- **What it contains**: Blazor WebAssembly application
- **What gets deployed**: The compiled `wwwroot` folder
- **Domain**: `https://your-app-name.azurestaticapps.net`
- **Workflow**: `.github/workflows/azure-static-web-apps.yml` (or auto-created by Azure)

**Files Deployed**:
- All files in `CampusConnectHub.Client/wwwroot/` (after build)
- Compiled Blazor DLLs and assets
- `index.html`
- CSS, JS, and other static assets

#### Backend → Azure App Service
**Project Folder**: `CampusConnectHub.Server/`
- **What it contains**: ASP.NET Core Web API
- **What gets deployed**: Published .NET application
- **Domain**: `https://your-api-app.azurewebsites.net`
- **Workflow**: `.github/workflows/azure-webapps-deploy.yml`

**Files Deployed**:
- Compiled `CampusConnectHub.Server.dll`
- All dependencies from `CampusConnectHub.Infrastructure/`
- All dependencies from `CampusConnectHub.Shared/`
- Runtime files and configuration

**Also Required** (referenced, not deployed):
- `CampusConnectHub.Infrastructure/` - Compiled into Server DLL
- `CampusConnectHub.Shared/` - Compiled into Server DLL

### Pros
- ✅ Static Web Apps has FREE tier
- ✅ Better separation of concerns
- ✅ Can scale independently
- ✅ Static Web Apps optimized for static content
- ✅ Automatic HTTPS and CDN

### Cons
- ❌ Need to manage 2 services
- ❌ Need to configure CORS between them
- ❌ Two separate domains

---

## Option 2: Single App Service (SIMPLE)

### Architecture
- **Both Frontend & Backend**: Single Azure App Service

### What Goes Where

#### Everything → Azure App Service
**Project Folders**: 
- `CampusConnectHub.Server/` (main app)
- `CampusConnectHub.Client/` (served as static files)

**How it works**:
1. Backend API runs on the App Service
2. Frontend static files are served from `wwwroot` folder
3. Both accessible from same domain: `https://your-app.azurewebsites.net`

**Configuration**:
- API endpoints: `https://your-app.azurewebsites.net/api/...`
- Frontend: `https://your-app.azurewebsites.net/` (serves index.html)

### Setup Steps

1. **Modify Server to serve static files**:
   ```csharp
   // In Program.cs, add:
   app.UseDefaultFiles();
   app.UseStaticFiles();
   ```

2. **Copy Client build output to Server**:
   - Build Client: `dotnet publish CampusConnectHub.Client`
   - Copy `CampusConnectHub.Client/bin/Release/net8.0/publish/wwwroot/` 
   - To `CampusConnectHub.Server/wwwroot/`

3. **Update Client Program.cs**:
   ```csharp
   var apiBaseUrl = builder.HostEnvironment.IsDevelopment() 
       ? "https://localhost:7126" 
       : builder.HostEnvironment.BaseAddress; // Same domain!
   ```

### Pros
- ✅ Single service to manage
- ✅ Same domain (no CORS issues)
- ✅ Simpler deployment
- ✅ Lower cost (one App Service)

### Cons
- ❌ No free tier (App Service costs money)
- ❌ Less optimal for static content
- ❌ Frontend and backend scale together

---

## Option 3: Static Web Apps + Azure Functions (NOT RECOMMENDED FOR THIS PROJECT)

This would require rewriting the API as Azure Functions, which is not suitable for your existing ASP.NET Core API.

---

## RECOMMENDATION: Option 1 (Two Services)

For your project, I recommend **Option 1** because:
1. **Cost-effective**: Static Web Apps has a free tier
2. **Better performance**: Static content served from CDN
3. **Scalability**: Can scale frontend and backend independently
4. **Already configured**: Your workflows are set up for this

---

## What Gets Deployed - Detailed Breakdown

### Frontend Deployment (Static Web Apps)

**Source**: `CampusConnectHub.Client/`
**Build Process**:
```bash
cd CampusConnectHub.Client
dotnet publish -c Release
```
**Output**: `CampusConnectHub.Client/bin/Release/net8.0/publish/wwwroot/`

**What's in wwwroot**:
- `index.html` - Entry point
- `_framework/` - Blazor runtime and your compiled DLLs
- `css/` - Stylesheets
- `favicon.png`, `icon-192.png` - Assets
- `sample-data/` - Sample JSON files

**Deployment**: GitHub Actions copies `wwwroot` to Static Web Apps

### Backend Deployment (App Service)

**Source**: `CampusConnectHub.Server/`
**Build Process**:
```bash
cd CampusConnectHub.Server
dotnet publish -c Release -o ./publish
```
**Output**: `CampusConnectHub.Server/publish/`

**What's in publish**:
- `CampusConnectHub.Server.dll` - Your API
- `CampusConnectHub.Infrastructure.dll` - Data layer (compiled)
- `CampusConnectHub.Shared.dll` - DTOs (compiled)
- `appsettings.json` - Configuration
- `.NET runtime files` - Required to run
- `All NuGet package DLLs` - Dependencies

**Deployment**: GitHub Actions zips and deploys `publish/` folder to App Service

---

## Quick Setup Guide

### For Option 1 (Two Services):

1. **Create Static Web App** (Frontend):
   - Azure Portal → Create Resource → Static Web App
   - Name: `campus-connect-hub`
   - Get deployment token → Add to GitHub Secrets as `AZURE_STATIC_WEB_APPS_API_TOKEN`

2. **Create App Service** (Backend):
   - Azure Portal → Create Resource → Web App
   - Name: `campus-connect-hub-api`
   - Runtime: .NET 8 (Linux)
   - Get publish profile → Add to GitHub Secrets as `AZURE_WEBAPP_PUBLISH_PROFILE`

3. **Configure**:
   - Set `ApiBaseUrl` in Static Web App = `https://campus-connect-hub-api.azurewebsites.net`
   - Set `StaticWebAppUrl` in App Service = `https://campus-connect-hub.azurestaticapps.net`

### For Option 2 (Single Service):

1. **Create App Service**:
   - Azure Portal → Create Resource → Web App
   - Name: `campus-connect-hub`
   - Runtime: .NET 8 (Linux)

2. **Modify code** to serve static files (see Option 2 setup above)

3. **Deploy**: Use the backend workflow, but include frontend files

---

## Which Option Should You Choose?

**Choose Option 1 if**:
- You want to minimize costs (Static Web Apps free tier)
- You want optimal performance
- You don't mind managing 2 services

**Choose Option 2 if**:
- You want simplicity (one service)
- Cost is not a concern
- You prefer same-domain deployment

For most projects, **Option 1 is recommended**.

