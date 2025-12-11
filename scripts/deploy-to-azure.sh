#!/bin/bash

# Azure Deployment Script for Campus Connect Hub
# This script helps automate the deployment process

set -e

echo "🚀 Campus Connect Hub - Azure Deployment Script"
echo "================================================"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo -e "${RED}❌ Azure CLI is not installed. Please install it first.${NC}"
    echo "Visit: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

# Check if logged in to Azure
echo -e "${YELLOW}Checking Azure login status...${NC}"
if ! az account show &> /dev/null; then
    echo -e "${YELLOW}Please login to Azure...${NC}"
    az login
fi

# Variables (update these)
RESOURCE_GROUP="campus-connect-hub-rg"
LOCATION="eastus"
APP_SERVICE_NAME="campus-connect-hub-api"
POSTGRES_SERVER="campus-connect-hub-db"
POSTGRES_ADMIN="postgresadmin"
POSTGRES_PASSWORD=""  # Set this
DATABASE_NAME="campusconnecthub"

echo -e "${GREEN}✅ Azure CLI is ready${NC}"

# Step 1: Create Resource Group
echo -e "\n${YELLOW}Step 1: Creating resource group...${NC}"
az group create --name $RESOURCE_GROUP --location $LOCATION || echo "Resource group already exists"

# Step 2: Create PostgreSQL Database
echo -e "\n${YELLOW}Step 2: Creating PostgreSQL database...${NC}"
if [ -z "$POSTGRES_PASSWORD" ]; then
    echo -e "${RED}Please set POSTGRES_PASSWORD in the script${NC}"
    exit 1
fi

az postgres flexible-server create \
  --resource-group $RESOURCE_GROUP \
  --name $POSTGRES_SERVER \
  --location $LOCATION \
  --admin-user $POSTGRES_ADMIN \
  --admin-password $POSTGRES_PASSWORD \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --version 15 \
  --storage-size 32 \
  --public-access 0.0.0.0 || echo "PostgreSQL server might already exist"

# Create database
az postgres flexible-server db create \
  --resource-group $RESOURCE_GROUP \
  --server-name $POSTGRES_SERVER \
  --database-name $DATABASE_NAME || echo "Database might already exist"

# Configure firewall
az postgres flexible-server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --name $POSTGRES_SERVER \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0 || echo "Firewall rule might already exist"

echo -e "${GREEN}✅ PostgreSQL database created${NC}"

# Step 3: Create App Service
echo -e "\n${YELLOW}Step 3: Creating App Service...${NC}"
az appservice plan create \
  --name "${APP_SERVICE_NAME}-plan" \
  --resource-group $RESOURCE_GROUP \
  --sku B1 \
  --is-linux || echo "App Service plan might already exist"

az webapp create \
  --resource-group $RESOURCE_GROUP \
  --plan "${APP_SERVICE_NAME}-plan" \
  --name $APP_SERVICE_NAME \
  --runtime "DOTNET|8.0" || echo "App Service might already exist"

echo -e "${GREEN}✅ App Service created${NC}"

# Step 4: Build and publish
echo -e "\n${YELLOW}Step 4: Building and publishing application...${NC}"
cd CampusConnectHub.Server
dotnet publish -c Release -o ./publish

# Create zip
cd publish
zip -r ../deploy.zip . > /dev/null
cd ../..

# Step 5: Deploy
echo -e "\n${YELLOW}Step 5: Deploying to Azure...${NC}"
az webapp deployment source config-zip \
  --resource-group $RESOURCE_GROUP \
  --name $APP_SERVICE_NAME \
  --src CampusConnectHub.Server/deploy.zip

echo -e "${GREEN}✅ Deployment complete!${NC}"

# Step 6: Configure App Settings
echo -e "\n${YELLOW}Step 6: Configuring App Settings...${NC}"
CONNECTION_STRING="Host=${POSTGRES_SERVER}.postgres.database.azure.com;Database=${DATABASE_NAME};Username=${POSTGRES_ADMIN};Password=${POSTGRES_PASSWORD};SSL Mode=Require;"

az webapp config appsettings set \
  --resource-group $RESOURCE_GROUP \
  --name $APP_SERVICE_NAME \
  --settings \
    "ConnectionStrings__DefaultConnection=$CONNECTION_STRING" \
    "JwtSettings__SecretKey=YourSuperSecretKeyThatShouldBeAtLeast32CharactersLongForProduction!" \
    "JwtSettings__Issuer=CampusConnectHub" \
    "JwtSettings__Audience=CampusConnectHub" \
    "JwtSettings__ExpirationInMinutes=60" \
    "ASPNETCORE_ENVIRONMENT=Production"

echo -e "${GREEN}✅ App Settings configured${NC}"

# Get URLs
API_URL=$(az webapp show --resource-group $RESOURCE_GROUP --name $APP_SERVICE_NAME --query defaultHostName -o tsv)

echo -e "\n${GREEN}================================================"
echo "✅ Deployment Complete!"
echo "================================================${NC}"
echo -e "API URL: ${GREEN}https://${API_URL}${NC}"
echo -e "Swagger: ${GREEN}https://${API_URL}/swagger${NC}"
echo -e "\n${YELLOW}Next steps:${NC}"
echo "1. Update CORS settings with your Static Web App URL"
echo "2. Deploy frontend to Azure Static Web Apps"
echo "3. Test the application"

