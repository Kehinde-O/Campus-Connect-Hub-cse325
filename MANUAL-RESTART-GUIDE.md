# Manual Azure App Service Restart Guide

If the GitHub Actions workflow seems stuck, you can manually restart your Azure App Service.

## Option 1: Restart via Azure Portal (Easiest)

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to: **App Services** → **Campus-Connect-Hub**
3. Click **Restart** button in the top toolbar
4. Wait 1-2 minutes for the app to restart
5. Test your application again

## Option 2: Restart via Azure CLI

If you have Azure CLI installed:

```bash
az webapp restart --name Campus-Connect-Hub --resource-group <your-resource-group>
```

## Option 3: Force Redeploy via GitHub Actions

1. Go to your GitHub repository
2. Click **Actions** tab
3. Find the latest workflow run
4. Click **Re-run jobs** → **Re-run all jobs**

## Option 4: Manual Deployment via Azure Portal

1. Go to Azure Portal → App Service → **Campus-Connect-Hub**
2. Go to **Deployment Center**
3. Click **Sync** to trigger a new deployment
4. Or go to **Advanced Tools (Kudu)** → **Restart** from there

## Check Deployment Status

1. Azure Portal → App Service → **Deployment Center**
   - Check if latest deployment shows as "Active"
   
2. Azure Portal → App Service → **Log stream**
   - Watch for any errors during startup
   
3. Azure Portal → App Service → **Overview**
   - Check "Status" - should be "Running"

## Verify CORS is Applied

After restart, test:
1. Open browser DevTools (F12)
2. Go to Network tab
3. Try logging in
4. Check if CORS errors are gone

## If Still Stuck

1. Check GitHub Actions: Repository → Actions → Check for failed workflows
2. Check Azure App Service Logs: Portal → App Service → Log stream
3. Verify Application Settings are correct
4. Try stopping and starting the app (instead of just restarting)

