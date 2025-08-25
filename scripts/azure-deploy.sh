#!/bin/bash

# Million Real Estate API - Azure Deployment Script
# This script deploys the API to Azure App Service with Container Registry

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Load environment variables
if [ -f "../azure.env.template" ]; then
    echo -e "${YELLOW}Loading environment variables...${NC}"
    export $(cat ../azure.env.template | grep -v '^#' | xargs)
fi

# Default values
RESOURCE_GROUP=${AZURE_RESOURCE_GROUP:-"million-real-estate-rg"}
LOCATION=${AZURE_LOCATION:-"eastus"}
APP_NAME=${AZURE_APP_NAME:-"million-real-estate-api"}
APP_SERVICE_PLAN=${AZURE_APP_SERVICE_PLAN:-"million-asp"}
ACR_NAME=${ACR_NAME:-"millionrealestateacr"}
SKU=${SKU:-"B1"}
DOCKER_IMAGE="million-api:latest"

echo -e "${BLUE}🚀 Million Real Estate API - Azure Deployment${NC}"
echo -e "${BLUE}=============================================${NC}"

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo -e "${RED}❌ Azure CLI is not installed. Please install it first.${NC}"
    echo "Visit: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

# Check if logged in to Azure
if ! az account show &> /dev/null; then
    echo -e "${YELLOW}⚠️  Not logged in to Azure. Please login first.${NC}"
    az login
fi

echo -e "${GREEN}✅ Azure CLI is ready${NC}"

# Function to create resource group
create_resource_group() {
    echo -e "${BLUE}📦 Creating Resource Group: ${RESOURCE_GROUP}${NC}"
    
    if az group show --name $RESOURCE_GROUP &> /dev/null; then
        echo -e "${YELLOW}⚠️  Resource Group ${RESOURCE_GROUP} already exists${NC}"
    else
        az group create --name $RESOURCE_GROUP --location $LOCATION
        echo -e "${GREEN}✅ Resource Group created successfully${NC}"
    fi
}

# Function to create Azure Container Registry
create_acr() {
    echo -e "${BLUE}🐳 Creating Azure Container Registry: ${ACR_NAME}${NC}"
    
    if az acr show --name $ACR_NAME --resource-group $RESOURCE_GROUP &> /dev/null; then
        echo -e "${YELLOW}⚠️  ACR ${ACR_NAME} already exists${NC}"
    else
        az acr create \
            --resource-group $RESOURCE_GROUP \
            --name $ACR_NAME \
            --sku Basic \
            --admin-enabled true
        echo -e "${GREEN}✅ ACR created successfully${NC}"
    fi
    
    # Get ACR login server
    ACR_LOGIN_SERVER=$(az acr show --name $ACR_NAME --resource-group $RESOURCE_GROUP --query loginServer --output tsv)
    echo -e "${GREEN}📍 ACR Login Server: ${ACR_LOGIN_SERVER}${NC}"
}

# Function to create App Service Plan
create_app_service_plan() {
    echo -e "${BLUE}📋 Creating App Service Plan: ${APP_SERVICE_PLAN}${NC}"
    
    if az appservice plan show --name $APP_SERVICE_PLAN --resource-group $RESOURCE_GROUP &> /dev/null; then
        echo -e "${YELLOW}⚠️  App Service Plan ${APP_SERVICE_PLAN} already exists${NC}"
    else
        az appservice plan create \
            --name $APP_SERVICE_PLAN \
            --resource-group $RESOURCE_GROUP \
            --location $LOCATION \
            --sku $SKU \
            --is-linux
        echo -e "${GREEN}✅ App Service Plan created successfully${NC}"
    fi
}

# Function to build and push Docker image
build_and_push_image() {
    echo -e "${BLUE}🔨 Building Docker image...${NC}"
    
    # Build the image
    docker build -t $DOCKER_IMAGE .
    
    # Tag for ACR
    docker tag $DOCKER_IMAGE $ACR_LOGIN_SERVER/$DOCKER_IMAGE
    
    echo -e "${BLUE}📤 Pushing image to ACR...${NC}"
    
    # Login to ACR
    az acr login --name $ACR_NAME
    
    # Push the image
    docker push $ACR_LOGIN_SERVER/$DOCKER_IMAGE
    
    echo -e "${GREEN}✅ Docker image pushed successfully${NC}"
}

# Function to create Web App
create_web_app() {
    echo -e "${BLUE}🌐 Creating Web App: ${APP_NAME}${NC}"
    
    if az webapp show --name $APP_NAME --resource-group $RESOURCE_GROUP &> /dev/null; then
        echo -e "${YELLOW}⚠️  Web App ${APP_NAME} already exists${NC}"
    else
        az webapp create \
            --resource-group $RESOURCE_GROUP \
            --plan $APP_SERVICE_PLAN \
            --name $APP_NAME \
            --deployment-local-git
        echo -e "${GREEN}✅ Web App created successfully${NC}"
    fi
    
    # Configure the web app to use Docker
    echo -e "${BLUE}⚙️  Configuring Web App for Docker...${NC}"
    
    az webapp config container set \
        --resource-group $RESOURCE_GROUP \
        --name $APP_NAME \
        --docker-custom-image-name $ACR_LOGIN_SERVER/$DOCKER_IMAGE
    
    # Enable continuous deployment
    az webapp deployment container config \
        --enable-cd true \
        --resource-group $RESOURCE_GROUP \
        --name $APP_NAME
    
    echo -e "${GREEN}✅ Web App configured successfully${NC}"
}

# Function to configure environment variables
configure_environment() {
    echo -e "${BLUE}🔧 Configuring environment variables...${NC}"
    
    # Set environment variables
    az webapp config appsettings set \
        --resource-group $RESOURCE_GROUP \
        --name $APP_NAME \
        --settings \
        ASPNETCORE_ENVIRONMENT=Production \
        MONGO__URI="$MONGO_URI" \
        MONGO__DATABASE="$MONGO_DATABASE" \
        JWT__SECRET="$JWT_SECRET" \
        JWT__ISSUER="$JWT_ISSUER" \
        JWT__AUDIENCE="$JWT_AUDIENCE" \
        RATE_LIMITING__MAX_REQUESTS="$RATE_LIMITING_MAX_REQUESTS" \
        RATE_LIMITING__WINDOW_MINUTES="$RATE_LIMITING_WINDOW_MINUTES" \
        LOG_LEVEL="$LOG_LEVEL" \
        LOG_LEVEL_ASPNET="$LOG_LEVEL_ASPNET"
    
    echo -e "${GREEN}✅ Environment variables configured${NC}"
}

# Function to create Application Insights
create_app_insights() {
    echo -e "${BLUE}📊 Creating Application Insights...${NC}"
    
    APP_INSIGHTS_NAME="${APP_NAME}-insights"
    
    if az monitor app-insights component show --app $APP_INSIGHTS_NAME --resource-group $RESOURCE_GROUP &> /dev/null; then
        echo -e "${YELLOW}⚠️  Application Insights ${APP_INSIGHTS_NAME} already exists${NC}"
    else
        az monitor app-insights component create \
            --app $APP_INSIGHTS_NAME \
            --location $LOCATION \
            --resource-group $RESOURCE_GROUP \
            --kind web
        echo -e "${GREEN}✅ Application Insights created successfully${NC}"
    fi
    
    # Get connection string
    CONNECTION_STRING=$(az monitor app-insights component show \
        --app $APP_INSIGHTS_NAME \
        --resource-group $RESOURCE_GROUP \
        --query connectionString \
        --output tsv)
    
    # Set connection string in web app
    az webapp config appsettings set \
        --resource-group $RESOURCE_GROUP \
        --name $APP_NAME \
        --settings APPINSIGHTS_CONNECTION_STRING="$CONNECTION_STRING"
    
    echo -e "${GREEN}✅ Application Insights configured${NC}"
}

# Function to create Azure Storage Account (for blob storage)
create_storage_account() {
    echo -e "${BLUE}💾 Creating Storage Account...${NC}"
    
    STORAGE_ACCOUNT_NAME="millionrealestate$(date +%s | tail -c 5)"
    
    if az storage account show --name $STORAGE_ACCOUNT_NAME --resource-group $RESOURCE_GROUP &> /dev/null; then
        echo -e "${YELLOW}⚠️  Storage Account ${STORAGE_ACCOUNT_NAME} already exists${NC}"
    else
        az storage account create \
            --name $STORAGE_ACCOUNT_NAME \
            --resource-group $RESOURCE_GROUP \
            --location $LOCATION \
            --sku Standard_LRS \
            --encryption-services blob
        
        # Create container for property images
        az storage container create \
            --name properties-images \
            --account-name $STORAGE_ACCOUNT_NAME \
            --public-access blob
        
        echo -e "${GREEN}✅ Storage Account created successfully${NC}"
    fi
    
    # Get connection string
    STORAGE_CONNECTION_STRING=$(az storage account show-connection-string \
        --name $STORAGE_ACCOUNT_NAME \
        --resource-group $RESOURCE_GROUP \
        --query connectionString \
        --output tsv)
    
    # Set in web app
    az webapp config appsettings set \
        --resource-group $RESOURCE_GROUP \
        --name $APP_NAME \
        --settings AZURE_STORAGE_CONNECTION_STRING="$STORAGE_CONNECTION_STRING"
    
    echo -e "${GREEN}✅ Storage Account configured${NC}"
}

# Function to show deployment summary
show_summary() {
    echo -e "${BLUE}📋 Deployment Summary${NC}"
    echo -e "${BLUE}===================${NC}"
    echo -e "${GREEN}✅ Resource Group: ${RESOURCE_GROUP}${NC}"
    echo -e "${GREEN}✅ Container Registry: ${ACR_NAME}${NC}"
    echo -e "${GREEN}✅ App Service Plan: ${APP_SERVICE_PLAN}${NC}"
    echo -e "${GREEN}✅ Web App: ${APP_NAME}${NC}"
    echo -e "${GREEN}✅ Application Insights: ${APP_NAME}-insights${NC}"
    echo ""
    echo -e "${BLUE}🌐 Your API is now available at:${NC}"
    echo -e "${GREEN}https://${APP_NAME}.azurewebsites.net${NC}"
    echo ""
    echo -e "${BLUE}🔧 Next steps:${NC}"
    echo -e "${YELLOW}1. Configure your domain (if needed)${NC}"
    echo -e "${YELLOW}2. Set up CI/CD pipeline${NC}"
    echo -e "${YELLOW}3. Configure monitoring and alerts${NC}"
    echo -e "${YELLOW}4. Test your API endpoints${NC}"
}

# Main deployment function
main() {
    echo -e "${BLUE}🚀 Starting Azure deployment...${NC}"
    
    create_resource_group
    create_acr
    create_app_service_plan
    build_and_push_image
    create_web_app
    configure_environment
    create_app_insights
    create_storage_account
    
    echo -e "${GREEN}🎉 Deployment completed successfully!${NC}"
    echo ""
    show_summary
}

# Run main function
main "$@"
