# Quick Start: Deploy to Azure in 30 Minutes

## Prerequisites Checklist
- [ ] Azure account (free tier works)
- [ ] .NET 8 SDK installed
- [ ] Git repository (optional, for GitHub Actions)

## Step-by-Step Deployment

### 1. Create PostgreSQL Database (10 minutes)

**Via Azure Portal:**
1. Go to [portal.azure.com](https://portal.azure.com)
2. Click "Create a resource" → Search "Azure Database for PostgreSQL"
3. Choose "Flexible server"
4. Fill in:
   - Server name: `campus-connect-hub-db` (add random numbers if taken)
   - Region: East US
   - PostgreSQL version: 15
   - Admin: `postgresadmin`
   - Password: Create strong password (save it!)
   - Compute: Basic, 1 vCore
5. Click "Review + create" → "Create"
6. Wait 5-10 minutes
7. Go to "Networking" → Add firewall rule: `0.0.0.0 - 255.255.255.255` (Allow Azure)
8. Go to "Connection strings" → Copy the connection string
9. Replace `Database=postgres` with `Database=campusconnecthub`

### 2. Deploy Backend API (10 minutes)

**Via Azure Portal:**
1. Click "Create a resource" → Search "Web App"
2. Fill in:
   - Name: `campus-connect-hub-api` (add random numbers if taken)
   - Runtime: .NET 8 (Linux)
   - Region: Same as database
   - Plan: Create new (Basic B1 - $13/month, or Free F1 for testing)
3. Click "Review + create" → "Create"
4. Wait 2-3 minutes

**Configure Settings:**
1. Go to your App Service → "Configuration"
2. Add these Application Settings (click "New application setting" for each):

```
ConnectionStrings__DefaultConnection = [Your PostgreSQL connection string from step 1]
JwtSettings__SecretKey = YourSuperSecretKeyThatShouldBeAtLeast32CharactersLongForProduction!
JwtSettings__Issuer = CampusConnectHub
JwtSettings__Audience = CampusConnectHub
JwtSettings__ExpirationInMinutes = 60
ASPNETCORE_ENVIRONMENT = Production
```

3. Click "Save" (takes 1-2 minutes)

**Deploy Code:**
- **Option A (Easiest):** Use Visual Studio
  - Right-click `CampusConnectHub.Server` → "Publish"
  - Choose "Azure" → Select your App Service → "Publish"

- **Option B:** Use Azure CLI
  ```bash
  cd CampusConnectHub.Server
  dotnet publish -c Release -o ./publish
  cd publish
  zip -r ../deploy.zip .
  
  az webapp deployment source config-zip \
    --resource-group [your-resource-group] \
    --name campus-connect-hub-api \
    --src ../deploy.zip
  ```

**Test Backend:**
- Visit: `https://campus-connect-hub-api.azurewebsites.net/swagger`
- You should see the Swagger UI

### 3. Deploy Frontend (10 minutes)

**Via Azure Portal:**
1. Click "Create a resource" → Search "Static Web App"
2. Fill in:
   - Name: `campus-connect-hub` (add random numbers if taken)
   - Plan: Free
   - Source: GitHub (or "Other" for manual)
   - App location: `CampusConnectHub.Client`
   - Output location: `wwwroot`
3. Click "Review + create" → "Create"

**Update Frontend Code:**
1. Edit `CampusConnectHub.Client/Program.cs`:
   ```csharp
   var apiBaseUrl = builder.HostEnvironment.IsDevelopment() 
       ? "https://localhost:7126" 
       : "https://campus-connect-hub-api.azurewebsites.net";
   ```

2. Commit and push (if using GitHub), or deploy manually

**Update CORS in Backend:**
1. Go to App Service → "Configuration"
2. Add:
   ```
   AllowedOrigins__0 = https://campus-connect-hub.azurestaticapps.net
   ```
3. Save

### 4. Initialize Database

The database will be created automatically on first API call. To verify:
1. Visit your API Swagger page
2. Try the `/api/auth/register` endpoint
3. Check PostgreSQL → "Query editor" → Run: `SELECT * FROM "Users";`

## Your Live URLs

- **Frontend**: `https://campus-connect-hub.azurestaticapps.net`
- **Backend API**: `https://campus-connect-hub-api.azurewebsites.net`
- **Swagger**: `https://campus-connect-hub-api.azurewebsites.net/swagger`

## Test Your Deployment

1. Visit your Static Web App URL
2. Register a new account
3. Login
4. Test features:
   - View news feed
   - Browse events
   - RSVP to an event
   - View resources
   - Login as admin (admin@campus.edu / Admin123!) to see admin dashboard

## Troubleshooting

**CORS Errors:**
- Make sure Static Web App URL is in `AllowedOrigins__0` in App Settings

**Database Connection Fails:**
- Check firewall allows Azure services (0.0.0.0 - 255.255.255.255)
- Verify connection string is correct

**500 Errors:**
- Check App Service → "Log stream" for errors
- Verify all App Settings are set correctly

**Database Not Created:**
- The `EnsureCreated()` runs on first API call
- Check logs for database errors

## Cost

- **Free Tier**: $0/month (limited resources, good for testing)
- **Basic Tier**: ~$26/month (PostgreSQL + App Service)
- Static Web Apps: Free tier available

## Next Steps

1. Set up custom domain (optional)
2. Configure CI/CD with GitHub Actions
3. Enable Application Insights for monitoring
4. Set up database backups

---

**Need Help?** See `AZURE-DEPLOYMENT-GUIDE.md` for detailed instructions.

