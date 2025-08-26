#!/bin/bash

# Million Real Estate API - Azure Bicep Deployment Script
# This script deploys the infrastructure using Bicep templates

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default values
RESOURCE_GROUP=${AZURE_RESOURCE_GROUP:-"million-real-estate-rg"}
LOCATION=${AZURE_LOCATION:-"eastus"}
DEPLOYMENT_NAME="million-deployment-$(date +%Y%m%d-%H%M%S)"

echo -e "${BLUE}🚀 Million Real Estate API - Azure Bicep Deployment${NC}"
echo -e "${BLUE}=================================================${NC}"

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo -e "${RED}❌ Azure CLI is not installed. Please install it first.${NC}"
    echo "Visit: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

# Check if Bicep is installed
if ! command -v bicep &> /dev/null; then
    echo -e "${YELLOW}⚠️  Bicep is not installed. Installing Bicep...${NC}"
    az bicep install
fi

# Check if logged in to Azure
if ! az account show &> /dev/null; then
    echo -e "${YELLOW}⚠️  Not logged in to Azure. Please login first.${NC}"
    az login
fi

echo -e "${GREEN}✅ Azure CLI and Bicep are ready${NC}"

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

# Function to deploy Bicep template
deploy_bicep() {
    echo -e "${BLUE}🏗️  Deploying infrastructure with Bicep...${NC}"
    
    # Navigate to infrastructure directory
    cd infrastructure
    
    # Deploy the Bicep template
    az deployment group create \
        --resource-group $RESOURCE_GROUP \
        --template-file main.bicep \
        --name $DEPLOYMENT_NAME \
        --parameters \
            location=$LOCATION \
            webAppName=${AZURE_APP_NAME:-"million-real-estate-api"} \
            appServicePlanName=${AZURE_APP_SERVICE_PLAN:-"million-asp"} \
            acrName=${ACR_NAME:-"millionrealestateacr"} \
            appServicePlanSku=${SKU:-"B1"} \
            acrSku=${ACR_SKU:-"Basic"} \
            storageAccountName=${STORAGE_ACCOUNT_NAME:-"millionrealestate"} \
            appInsightsName=${APP_INSIGHTS_NAME:-"million-real-estate-insights"} \
            keyVaultName=${KEY_VAULT_NAME:-"million-key-vault"} \
            cosmosDbAccountName=${COSMOS_DB_ACCOUNT_NAME:-"million-cosmos-db"} \
            cosmosDbDatabaseName=${COSMOS_DB_DATABASE_NAME:-"million"} \
            cosmosDbContainerName=${COSMOS_DB_CONTAINER_NAME:-"properties"}
    
    echo -e "${GREEN}✅ Bicep deployment completed successfully${NC}"
    
    # Go back to root directory
    cd ..
}

# Function to show deployment outputs
show_outputs() {
    echo -e "${BLUE}📋 Getting deployment outputs...${NC}"
    
    # Get the outputs from the deployment
    OUTPUTS=$(az deployment group show \
        --resource-group $RESOURCE_GROUP \
        --name $DEPLOYMENT_NAME \
        --query properties.outputs \
        --output json)
    
    echo -e "${GREEN}✅ Deployment outputs:${NC}"
    echo "$OUTPUTS" | jq '.'
}

# Function to configure web app
configure_web_app() {
    echo -e "${BLUE}⚙️  Configuring Web App...${NC}"
    
    WEB_APP_NAME=${AZURE_APP_NAME:-"million-real-estate-api"}
    
    # Get the ACR login server from outputs
    ACR_LOGIN_SERVER=$(az deployment group show \
        --resource-group $RESOURCE_GROUP \
        --name $DEPLOYMENT_NAME \
        --query "properties.outputs.acrLoginServer.value" \
        --output tsv)
    
    echo -e "${GREEN}📍 ACR Login Server: ${ACR_LOGIN_SERVER}${NC}"
    
    # Configure the web app to use Docker
    az webapp config container set \
        --resource-group $RESOURCE_GROUP \
        --name $WEB_APP_NAME \
        --docker-custom-image-name $ACR_LOGIN_SERVER/million-api:latest
    
    # Enable continuous deployment
    az webapp deployment container config \
        --enable-cd true \
        --resource-group $RESOURCE_GROUP \
        --name $WEB_APP_NAME
    
    echo -e "${GREEN}✅ Web App configured successfully${NC}"
}

# Function to build and push Docker image
build_and_push_image() {
    echo -e "${BLUE}🔨 Building and pushing Docker image...${NC}"
    
    # Get the ACR login server from outputs
    ACR_LOGIN_SERVER=$(az deployment group show \
        --resource-group $RESOURCE_GROUP \
        --name $DEPLOYMENT_NAME \
        --query "properties.outputs.acrLoginServer.value" \
        --output tsv)
    
    ACR_NAME=${ACR_NAME:-"millionrealestateacr"}
    
    # Build the image
    docker build -t million-api:latest .
    
    # Tag for ACR
    docker tag million-api:latest $ACR_LOGIN_SERVER/million-api:latest
    
    # Login to ACR
    az acr login --name $ACR_NAME
    
    # Push the image
    docker push $ACR_LOGIN_SERVER/million-api:latest
    
    echo -e "${GREEN}✅ Docker image pushed successfully${NC}"
}

# Function to show deployment summary
show_summary() {
    echo -e "${BLUE}📋 Deployment Summary${NC}"
    echo -e "${BLUE}===================${NC}"
    echo -e "${GREEN}✅ Resource Group: ${RESOURCE_GROUP}${NC}"
    echo -e "${GREEN}✅ Deployment Name: ${DEPLOYMENT_NAME}${NC}"
    echo -e "${GREEN}✅ Location: ${LOCATION}${NC}"
    echo ""
    
    # Get outputs
    WEB_APP_URL=$(az deployment group show \
        --resource-group $RESOURCE_GROUP \
        --name $DEPLOYMENT_NAME \
        --query "properties.outputs.webAppUrl.value" \
        --output tsv)
    
    ACR_LOGIN_SERVER=$(az deployment group show \
        --resource-group $RESOURCE_GROUP \
        --name $DEPLOYMENT_NAME \
        --query "properties.outputs.acrLoginServer.value" \
        --output tsv)
    
    STORAGE_ACCOUNT=$(az deployment group show \
        --resource-group $RESOURCE_GROUP \
        --name $DEPLOYMENT_NAME \
        --query "properties.outputs.storageAccountName.value" \
        --output tsv)
    
    echo -e "${BLUE}🌐 Your API is now available at:${NC}"
    echo -e "${GREEN}${WEB_APP_URL}${NC}"
    echo ""
    echo -e "${BLUE}🔧 Infrastructure Details:${NC}"
    echo -e "${GREEN}✅ Container Registry: ${ACR_LOGIN_SERVER}${NC}"
    echo -e "${GREEN}✅ Storage Account: ${STORAGE_ACCOUNT}${NC}"
    echo ""
    echo -e "${BLUE}🔧 Next steps:${NC}"
    echo -e "${YELLOW}1. Build and push your Docker image${NC}"
    echo -e "${YELLOW}2. Configure your domain (if needed)${NC}"
    echo -e "${YELLOW}3. Set up CI/CD pipeline${NC}"
    echo -e "${YELLOW}4. Configure monitoring and alerts${NC}"
    echo -e "${YELLOW}5. Test your API endpoints${NC}"
}

# Main deployment function
main() {
    echo -e "${BLUE}🚀 Starting Azure Bicep deployment...${NC}"
    
    create_resource_group
    deploy_bicep
    show_outputs
    configure_web_app
    build_and_push_image
    
    echo -e "${GREEN}🎉 Bicep deployment completed successfully!${NC}"
    echo ""
    show_summary
}

# Run main function
main "$@"
