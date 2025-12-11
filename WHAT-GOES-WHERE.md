# What Goes Where - Azure Deployment Guide

## Quick Answer

**YES, you need 2 separate Azure services** (recommended approach):

1. **Azure Static Web App** → For Frontend (`CampusConnectHub.Client/`)
2. **Azure App Service** → For Backend (`CampusConnectHub.Server/`)

---

## Detailed Breakdown

### Service 1: Azure Static Web App (Frontend)

**What to Create**: Azure Static Web App
**Name Example**: `campus-connect-hub` (or your choice)
**Domain**: `https://campus-connect-hub.azurestaticapps.net`

**What Gets Deployed**:
- **Source Folder**: `CampusConnectHub.Client/`
- **Build Output**: `CampusConnectHub.Client/bin/Release/net8.0/publish/wwwroot/`
- **Contains**:
  - `index.html` - Entry point
  - `_framework/` - Blazor runtime and compiled DLLs
  - `css/` - All stylesheets
  - Static assets (images, icons, etc.)

**How It Works**:
1. GitHub Actions builds the Blazor WebAssembly app
2. Outputs to `wwwroot` folder
3. Deploys `wwwroot` contents to Static Web App
4. Users access: `https://campus-connect-hub.azurestaticapps.net`

**Workflow File**: `.github/workflows/azure-static-web-apps.yml` (or auto-created by Azure)

---

### Service 2: Azure App Service (Backend)

**What to Create**: Azure App Service (Linux, .NET 8)
**Name Example**: `campus-connect-hub-api` (or your choice)
**Domain**: `https://campus-connect-hub-api.azurewebsites.net`

**What Gets Deployed**:
- **Source Folder**: `CampusConnectHub.Server/`
- **Build Output**: `CampusConnectHub.Server/publish/`
- **Contains**:
  - `CampusConnectHub.Server.dll` - Your API
  - `CampusConnectHub.Infrastructure.dll` - Data layer (automatically included)
  - `CampusConnectHub.Shared.dll` - DTOs (automatically included)
  - `appsettings.json` - Configuration
  - All .NET runtime files
  - All NuGet package dependencies

**How It Works**:
1. GitHub Actions builds the .NET API project
2. Publishes to `publish/` folder
3. Deploys entire `publish/` folder to App Service
4. API accessible at: `https://campus-connect-hub-api.azurewebsites.net/api/...`

**Workflow File**: `.github/workflows/azure-webapps-deploy.yml`

**Note**: `CampusConnectHub.Infrastructure/` and `CampusConnectHub.Shared/` are **NOT deployed separately** - they are compiled into the Server DLL during build.

---

## Project Structure Reference

```
Campus Connect Hub/
├── CampusConnectHub.Client/          → Deploy to Static Web App
│   ├── Pages/                        → Compiled into Blazor DLL
│   ├── Components/                    → Compiled into Blazor DLL
│   ├── Services/                     → Compiled into Blazor DLL
│   └── wwwroot/                      → Deployed as static files
│
├── CampusConnectHub.Server/          → Deploy to App Service
│   ├── Controllers/                  → Compiled into Server DLL
│   ├── Services/                     → Compiled into Server DLL
│   └── Program.cs                    → Entry point
│
├── CampusConnectHub.Infrastructure/  → Compiled into Server DLL (not deployed separately)
│   ├── Data/                         → Included in Server build
│   └── Entities/                    → Included in Server build
│
└── CampusConnectHub.Shared/         → Compiled into both Client and Server DLLs
    └── DTOs/                         → Included in both builds
```

---

## Step-by-Step: What Happens During Deployment

### Frontend Deployment (Static Web App)

```bash
# 1. GitHub Actions runs this:
cd CampusConnectHub.Client
dotnet publish -c Release

# 2. This creates:
CampusConnectHub.Client/bin/Release/net8.0/publish/wwwroot/
  ├── index.html
  ├── _framework/
  │   ├── blazor.webassembly.js
  │   ├── CampusConnectHub.Client.dll
  │   └── ... (other DLLs)
  ├── css/
  └── ...

# 3. GitHub Actions uploads wwwroot/ to Static Web App
# 4. Users can now access: https://campus-connect-hub.azurestaticapps.net
```

### Backend Deployment (App Service)

```bash
# 1. GitHub Actions runs this:
cd CampusConnectHub.Server
dotnet publish -c Release -o ./publish

# 2. This creates:
CampusConnectHub.Server/publish/
  ├── CampusConnectHub.Server.dll          (includes Infrastructure & Shared)
  ├── CampusConnectHub.Infrastructure.dll  (referenced by Server)
  ├── CampusConnectHub.Shared.dll          (referenced by Server)
  ├── appsettings.json
  ├── appsettings.Production.json
  └── ... (runtime files, dependencies)

# 3. GitHub Actions zips and deploys publish/ to App Service
# 4. API accessible at: https://campus-connect-hub-api.azurewebsites.net/api/...
```

---

## Alternative: Single App Service (If You Prefer Simplicity)

If you want **ONE service instead of TWO**, you can:

1. **Create only Azure App Service**
2. **Modify Server to serve static files** (add to Program.cs):
   ```csharp
   app.UseDefaultFiles();
   app.UseStaticFiles();
   ```
3. **Copy Client build to Server's wwwroot** during deployment
4. **Update Client Program.cs** to use same domain for API

**Pros**: One service, same domain, simpler
**Cons**: Costs money (no free tier), less optimal for static content

---

## Current Workflow Status

I see you have:
- ✅ `.github/workflows/azure-webapps-deploy.yml` - For backend (App Service)
- ✅ `.github/workflows/main_campus-connect-hub.yml` - Auto-created by Azure (also for App Service)

**You still need**:
- ⚠️ Frontend workflow for Static Web App (or create Static Web App in Azure Portal and it will auto-create the workflow)

---

## Recommendation

**Use 2 services** (Static Web App + App Service):
- Static Web App has **FREE tier**
- Better performance for static content
- Already configured in your workflows

**What to create in Azure**:
1. **Static Web App** named `campus-connect-hub` (or your choice)
2. **App Service** named `campus-connect-hub-api` (or your choice)

The workflows will automatically deploy the right parts to each service!

