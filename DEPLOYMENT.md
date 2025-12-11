# Azure Deployment Guide - Campus Connect Hub

## Architecture Overview

When deployed to Azure, the application uses a **two-tier architecture**:

### Frontend (Blazor WebAssembly)
- **Service**: Azure Static Web Apps
- **Domain**: `https://your-app-name.azurestaticapps.net`
- **Deployment**: Automatic via GitHub Actions on push to `main` branch
- **Workflow**: `.github/workflows/azure-static-web-apps.yml`

### Backend (ASP.NET Core Web API)
- **Service**: Azure App Service (Linux)
- **Domain**: `https://your-api-app.azurewebsites.net`
- **Deployment**: Automatic via GitHub Actions on push to `main` branch
- **Workflow**: `.github/workflows/azure-webapps-deploy.yml`

### Database
- **Service**: Neon PostgreSQL (or Azure Database for PostgreSQL)
- **Connection**: Configured via connection string in App Service settings

## Domain Configuration

### Frontend Domain
- **URL Format**: `https://<your-app-name>.azurestaticapps.net`
- **Example**: `https://campus-connect-hub.azurestaticapps.net`
- **Purpose**: Serves the Blazor WebAssembly application
- **Configuration**: Set in Azure Static Web Apps portal

### Backend Domain
- **URL Format**: `https://<your-api-app>.azurewebsites.net`
- **Example**: `https://campus-connect-hub-api.azurewebsites.net`
- **Purpose**: Hosts the REST API endpoints
- **Configuration**: Set in Azure App Service portal

## How They Work Together

1. **User visits**: `https://campus-connect-hub.azurestaticapps.net`
2. **Frontend loads**: Blazor WebAssembly app downloads and runs in browser
3. **API calls**: Frontend makes HTTP requests to `https://campus-connect-hub-api.azurewebsites.net/api/...`
4. **CORS**: Backend allows requests from the Static Web App domain
5. **Authentication**: JWT tokens stored in browser localStorage

## CI/CD Pipeline

### Frontend Deployment (Static Web Apps)
- **Trigger**: Push to `main` branch
- **Workflow**: `azure-static-web-apps.yml`
- **Builds**: Blazor WebAssembly app in `CampusConnectHub.Client`
- **Output**: Deploys `wwwroot` folder to Static Web Apps
- **Automatic**: Yes, via GitHub Actions

### Backend Deployment (App Service)
- **Trigger**: Push to `main` branch (when Server/Infrastructure/Shared changes)
- **Workflow**: `azure-webapps-deploy.yml`
- **Builds**: .NET 8 API in `CampusConnectHub.Server`
- **Output**: Publishes and deploys to App Service
- **Automatic**: Yes, via GitHub Actions

## Setup Instructions

### 1. Create Azure Resources

#### Backend - App Service
```bash
# Create resource group
az group create --name campus-connect-hub-rg --location eastus

# Create App Service plan
az appservice plan create --name campus-connect-hub-plan \
  --resource-group campus-connect-hub-rg \
  --sku B1 --is-linux

# Create Web App
az webapp create --name campus-connect-hub-api \
  --resource-group campus-connect-hub-rg \
  --plan campus-connect-hub-plan \
  --runtime "DOTNET|8.0"
```

#### Frontend - Static Web App
```bash
# Create Static Web App (via Azure Portal or CLI)
az staticwebapp create \
  --name campus-connect-hub \
  --resource-group campus-connect-hub-rg \
  --location eastus2
```

### 2. Configure Backend (App Service)

#### Application Settings
Add these in Azure Portal → App Service → Configuration → Application settings:

```
ASPNETCORE_ENVIRONMENT = Production
ConnectionStrings__DefaultConnection = <Your Neon PostgreSQL connection string>
JwtSettings__SecretKey = <Generate a secure 32+ character key>
JwtSettings__Issuer = CampusConnectHub
JwtSettings__Audience = CampusConnectHub
JwtSettings__ExpirationInMinutes = 60
StaticWebAppUrl = https://campus-connect-hub.azurestaticapps.net
AllowedOrigins__0 = https://campus-connect-hub.azurestaticapps.net
```

#### Get Publish Profile
1. Go to Azure Portal → App Service → Get publish profile
2. Download the `.PublishSettings` file
3. Add to GitHub Secrets as `AZURE_WEBAPP_PUBLISH_PROFILE`

### 3. Configure Frontend (Static Web App)

#### Build Configuration
In Azure Portal → Static Web App → Configuration:
- **App location**: `CampusConnectHub.Client`
- **Output location**: `wwwroot`
- **API location**: (leave empty)

#### Environment Variables
Add in Azure Portal → Static Web App → Configuration → Application settings:

```
ApiBaseUrl = https://campus-connect-hub-api.azurewebsites.net
```

#### Get Deployment Token
1. Go to Azure Portal → Static Web App → Manage deployment token
2. Copy the token
3. Add to GitHub Secrets as `AZURE_STATIC_WEB_APPS_API_TOKEN`

### 4. Configure GitHub Secrets

Go to your GitHub repository → Settings → Secrets and variables → Actions:

Add these secrets:
- `AZURE_WEBAPP_PUBLISH_PROFILE` - From App Service publish profile
- `AZURE_STATIC_WEB_APPS_API_TOKEN` - From Static Web App deployment token

### 5. Update Workflow Files

#### Backend Workflow (`.github/workflows/azure-webapps-deploy.yml`)
Update the `AZURE_WEBAPP_NAME` environment variable:
```yaml
env:
  AZURE_WEBAPP_NAME: campus-connect-hub-api  # Your App Service name
```

#### Frontend Workflow (`.github/workflows/azure-static-web-apps.yml`)
Already configured, but verify the `app_location` matches your project structure.

### 6. Update Client Configuration

The `Program.cs` in the Client project is already configured to:
- Use `https://localhost:7126` in development
- Use `ApiBaseUrl` from configuration in production
- Fallback to a default API URL if not configured

Make sure to set `ApiBaseUrl` in Static Web App configuration.

## Deployment Flow

### On Push to Main Branch:

1. **GitHub Actions triggers both workflows**
2. **Backend workflow**:
   - Builds the .NET API
   - Publishes to `./publish` folder
   - Deploys to App Service
   - API available at: `https://campus-connect-hub-api.azurewebsites.net`

3. **Frontend workflow**:
   - Builds Blazor WebAssembly app
   - Deploys `wwwroot` to Static Web Apps
   - Frontend available at: `https://campus-connect-hub.azurestaticapps.net`

## Testing the Deployment

1. **Access Frontend**: `https://campus-connect-hub.azurestaticapps.net`
2. **Check API**: `https://campus-connect-hub-api.azurewebsites.net/swagger`
3. **Test Login**: Use `admin@campus.edu` / `Admin123!`
4. **Verify CORS**: Check browser console for CORS errors

## Troubleshooting

### CORS Errors
- Verify `StaticWebAppUrl` is set correctly in App Service
- Check that the Static Web App URL matches exactly (including https://)
- Ensure CORS policy allows credentials if using cookies

### API Not Found
- Verify `ApiBaseUrl` is set in Static Web App configuration
- Check that the App Service is running
- Verify the API URL is accessible: `https://campus-connect-hub-api.azurewebsites.net/swagger`

### Database Connection Issues
- Verify connection string in App Service configuration
- Check that Neon PostgreSQL allows connections from Azure IPs
- Ensure SSL mode is set correctly in connection string

## Custom Domains (Optional)

### Frontend Custom Domain
1. Go to Static Web App → Custom domains
2. Add your domain (e.g., `app.yourdomain.com`)
3. Configure DNS records as instructed

### Backend Custom Domain
1. Go to App Service → Custom domains
2. Add your domain (e.g., `api.yourdomain.com`)
3. Configure DNS records
4. Update `ApiBaseUrl` in Static Web App configuration

## Security Best Practices

1. **Use Azure Key Vault** for sensitive configuration
2. **Enable HTTPS only** on both services
3. **Configure firewall rules** for PostgreSQL
4. **Rotate JWT secret keys** regularly
5. **Use managed identity** for Azure resource access
6. **Enable Application Insights** for monitoring

## Cost Optimization

- **Static Web Apps**: Free tier available (with limitations)
- **App Service**: Use B1 (Basic) tier for development, scale up for production
- **PostgreSQL**: Neon free tier or Azure Database for PostgreSQL Basic tier

## Monitoring

- **Application Insights**: Enable for both services
- **Logs**: View in Azure Portal → App Service → Log stream
- **Metrics**: Monitor in Azure Portal → App Service → Metrics

