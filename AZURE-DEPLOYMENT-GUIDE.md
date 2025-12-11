# Complete Azure Deployment Guide for Campus Connect Hub

This guide provides step-by-step instructions to deploy your Campus Connect Hub application to Azure with a live URL and a running PostgreSQL database.

## Prerequisites

- Azure account with active subscription
- Azure CLI installed (optional but recommended)
- .NET 8 SDK installed locally
- Git repository (for GitHub Actions deployment)

## Step 1: Create Azure PostgreSQL Database

### Option A: Using Azure Portal

1. **Navigate to Azure Portal** (portal.azure.com)
2. **Create PostgreSQL Database:**
   - Click "Create a resource"
   - Search for "Azure Database for PostgreSQL"
   - Click "Create"
   - Choose "Flexible server" (recommended for cost-effectiveness)
   - Fill in the form:
     - **Subscription**: Your subscription
     - **Resource Group**: Create new (e.g., "campus-connect-hub-rg")
     - **Server name**: `campus-connect-hub-db` (must be globally unique)
     - **Region**: Choose closest to you (e.g., "East US")
     - **PostgreSQL version**: 15
     - **Workload type**: Development
     - **Compute + storage**: Basic, 1 vCore, 32 GB storage (minimum)
     - **Administrator username**: `postgresadmin` (or your choice)
     - **Password**: Create a strong password (save this!)
   - Click "Review + create" then "Create"
   - Wait 5-10 minutes for deployment

3. **Configure Firewall Rules:**
   - Once created, go to your PostgreSQL server
   - Click "Networking" in the left menu
   - Click "Add current client IP address" (for your local testing)
   - Click "Add 0.0.0.0 - 255.255.255.255" (to allow Azure services)
   - Click "Save"

4. **Get Connection String:**
   - Go to "Connection strings" in the left menu
   - Copy the "ADO.NET" connection string
   - It will look like:
     ```
     Host=campus-connect-hub-db.postgres.database.azure.com;Database=postgres;Username=postgresadmin;Password=YourPassword;SSL Mode=Require;
     ```
   - Replace `postgres` with `campusconnecthub` in the Database part
   - Save this connection string securely

### Option B: Using Azure CLI

```bash
# Login to Azure
az login

# Create resource group
az group create --name campus-connect-hub-rg --location eastus

# Create PostgreSQL flexible server
az postgres flexible-server create \
  --resource-group campus-connect-hub-rg \
  --name campus-connect-hub-db \
  --location eastus \
  --admin-user postgresadmin \
  --admin-password YourStrongPassword123! \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --version 15 \
  --storage-size 32

# Create database
az postgres flexible-server db create \
  --resource-group campus-connect-hub-rg \
  --server-name campus-connect-hub-db \
  --database-name campusconnecthub

# Configure firewall (allow Azure services)
az postgres flexible-server firewall-rule create \
  --resource-group campus-connect-hub-rg \
  --name campus-connect-hub-db \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

## Step 2: Deploy Backend API to Azure App Service

### 2.1 Create App Service

1. **In Azure Portal:**
   - Click "Create a resource"
   - Search for "Web App"
   - Click "Create"
   - Fill in:
     - **Subscription**: Your subscription
     - **Resource Group**: Same as database (campus-connect-hub-rg)
     - **Name**: `campus-connect-hub-api` (must be globally unique)
     - **Publish**: Code
     - **Runtime stack**: .NET 8
     - **Operating System**: Linux
     - **Region**: Same as database
     - **App Service Plan**: Create new (Basic B1 - $13/month, or Free F1 for testing)
   - Click "Review + create" then "Create"
   - Wait 2-3 minutes

### 2.2 Configure App Settings

1. **Go to your App Service** in Azure Portal
2. **Click "Configuration"** in the left menu
3. **Add Application Settings:**

   Click "New application setting" for each:

   ```
   Name: ConnectionStrings__DefaultConnection
   Value: Host=campus-connect-hub-db.postgres.database.azure.com;Database=campusconnecthub;Username=postgresadmin;Password=YourPassword;SSL Mode=Require;
   ```

   ```
   Name: JwtSettings__SecretKey
   Value: YourSuperSecretKeyThatShouldBeAtLeast32CharactersLongForProduction!
   ```

   ```
   Name: JwtSettings__Issuer
   Value: CampusConnectHub
   ```

   ```
   Name: JwtSettings__Audience
   Value: CampusConnectHub
   ```

   ```
   Name: JwtSettings__ExpirationInMinutes
   Value: 60
   ```

   ```
   Name: ASPNETCORE_ENVIRONMENT
   Value: Production
   ```

4. **Click "Save"** (takes 1-2 minutes to apply)

### 2.3 Update CORS Configuration

We need to update the backend to allow your Static Web App URL. Update `Program.cs`:

```csharp
// Configure CORS for Blazor WebAssembly
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
            ?? new[] { "https://localhost:5001", "http://localhost:5000" };
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

Add this to App Settings:
```
Name: AllowedOrigins__0
Value: https://your-static-web-app-name.azurestaticapps.net
```

### 2.4 Deploy Backend Code

**Option A: Using Visual Studio (Easiest)**

1. Right-click `CampusConnectHub.Server` project
2. Select "Publish"
3. Choose "Azure" → "Azure App Service (Linux)"
4. Select your App Service
5. Click "Publish"
6. Wait for deployment to complete

**Option B: Using Azure CLI**

```bash
# Build and publish
cd CampusConnectHub.Server
dotnet publish -c Release -o ./publish

# Create deployment package
cd publish
zip -r ../deploy.zip .

# Deploy using Azure CLI
az webapp deployment source config-zip \
  --resource-group campus-connect-hub-rg \
  --name campus-connect-hub-api \
  --src ../deploy.zip
```

**Option C: Using GitHub Actions**

The workflow file `.github/workflows/azure-webapps-deploy.yml` is already configured. You just need to:
1. Push your code to GitHub
2. Add secret `AZURE_WEBAPP_PUBLISH_PROFILE` in GitHub repository settings
3. Get publish profile from Azure Portal → App Service → "Get publish profile"

### 2.5 Initialize Database

After deployment, the database will be created automatically on first API call (due to `EnsureCreated()`). However, for production, you should use migrations:

1. **SSH into App Service** (or use Azure Cloud Shell):
   ```bash
   az webapp ssh --resource-group campus-connect-hub-rg --name campus-connect-hub-api
   ```

2. **Or run migrations locally** pointing to Azure database:
   ```bash
   # Update connection string temporarily in appsettings.json
   cd CampusConnectHub.Server
   dotnet ef database update --project ../CampusConnectHub.Infrastructure
   ```

3. **Or create a migration endpoint** (temporary, remove after use):
   Add to `Program.cs`:
   ```csharp
   app.MapPost("/api/migrate", async (ApplicationDbContext db) =>
   {
       await db.Database.MigrateAsync();
       return Results.Ok("Database migrated");
   });
   ```
   Then call: `POST https://your-api.azurewebsites.net/api/migrate`

## Step 3: Deploy Frontend to Azure Static Web Apps

### 3.1 Create Static Web App

1. **In Azure Portal:**
   - Click "Create a resource"
   - Search for "Static Web App"
   - Click "Create"
   - Fill in:
     - **Subscription**: Your subscription
     - **Resource Group**: Same as others
     - **Name**: `campus-connect-hub` (must be globally unique)
     - **Plan type**: Free (or Standard for custom domain)
     - **Region**: Same as others
     - **Source**: GitHub (or other)
     - **GitHub account**: Connect if needed
     - **Organization**: Your GitHub org
     - **Repository**: Your repository
     - **Branch**: main
     - **Build Presets**: Custom
     - **App location**: `CampusConnectHub.Client`
     - **Api location**: (leave empty)
     - **Output location**: `wwwroot`
   - Click "Review + create" then "Create"

2. **Note the URL**: You'll get a URL like `https://campus-connect-hub.azurestaticapps.net`

### 3.2 Update Frontend API URL

1. **Update `CampusConnectHub.Client/Program.cs`:**
   ```csharp
   var apiBaseUrl = builder.HostEnvironment.IsDevelopment() 
       ? "https://localhost:7126" 
       : "https://campus-connect-hub-api.azurewebsites.net";
   ```

2. **Commit and push** to trigger deployment

### 3.3 Manual Deployment (Alternative)

If not using GitHub Actions:

```bash
cd CampusConnectHub.Client
dotnet publish -c Release

# Install Static Web Apps CLI
npm install -g @azure/static-web-apps-cli

# Deploy
swa deploy ./bin/Release/net8.0/publish/wwwroot \
  --deployment-token YOUR_DEPLOYMENT_TOKEN \
  --env production
```

Get deployment token from: Azure Portal → Static Web App → "Manage deployment token"

## Step 4: Verify Deployment

### 4.1 Test Backend API

1. Visit: `https://campus-connect-hub-api.azurewebsites.net/swagger`
2. Test the `/api/auth/register` endpoint
3. Check logs: Azure Portal → App Service → "Log stream"

### 4.2 Test Frontend

1. Visit your Static Web App URL
2. Register a new account
3. Test all features:
   - News feed
   - Events and RSVP
   - Resources
   - Admin dashboard

### 4.3 Check Database

1. **In Azure Portal:**
   - Go to your PostgreSQL server
   - Click "Query editor"
   - Login with your credentials
   - Run: `SELECT * FROM "Users";`
   - You should see the seeded admin and student users

## Step 5: Monitor and Troubleshoot

### 5.1 Application Insights (Recommended)

1. **Create Application Insights:**
   - In Azure Portal, create "Application Insights" resource
   - Link it to your App Service
   - View logs and metrics

### 5.2 Common Issues

**Issue: CORS errors**
- Solution: Ensure Static Web App URL is in `AllowedOrigins` in App Settings

**Issue: Database connection fails**
- Solution: Check firewall rules allow Azure services (0.0.0.0 - 255.255.255.255)

**Issue: 500 errors**
- Solution: Check App Service logs → "Log stream" or "Logs"

**Issue: Database not created**
- Solution: The `EnsureCreated()` should run on first API call. Check logs for errors.

### 5.3 View Logs

```bash
# Using Azure CLI
az webapp log tail --resource-group campus-connect-hub-rg --name campus-connect-hub-api

# Or in Portal: App Service → Log stream
```

## Cost Estimation

- **PostgreSQL Flexible Server (Basic)**: ~$13/month
- **App Service (Basic B1)**: ~$13/month
- **Static Web Apps (Free tier)**: $0/month
- **Total**: ~$26/month (or use Free tiers for testing)

## Security Best Practices

1. **Use Azure Key Vault** for secrets (instead of App Settings)
2. **Enable HTTPS only** on App Service
3. **Restrict PostgreSQL firewall** to only necessary IPs
4. **Use managed identity** for database connections
5. **Rotate JWT secrets** regularly
6. **Enable Application Insights** for monitoring

## Quick Reference: URLs

After deployment, you'll have:
- **Frontend**: `https://campus-connect-hub.azurestaticapps.net`
- **Backend API**: `https://campus-connect-hub-api.azurewebsites.net`
- **Swagger**: `https://campus-connect-hub-api.azurewebsites.net/swagger`
- **Database**: `campus-connect-hub-db.postgres.database.azure.com`

## Next Steps

1. Set up custom domain (optional)
2. Configure CI/CD pipelines
3. Set up staging environment
4. Enable monitoring and alerts
5. Configure backup for database

---

**Need Help?** Check Azure documentation or contact Azure support.

