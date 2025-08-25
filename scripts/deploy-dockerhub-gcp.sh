#!/bin/bash

# Million Real Estate API - Complete Deployment Pipeline
# This script deploys to Docker Hub first, then to Google Cloud Platform

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

echo -e "${BLUE}🚀 Million Real Estate API - Complete Deployment Pipeline${NC}"
echo -e "${BLUE}=====================================================${NC}"
echo -e "${BLUE}📋 This will:${NC}"
echo -e "${BLUE}   1. Build and push Docker image to Docker Hub${NC}"
echo -e "${BLUE}   2. Deploy the image to Google Cloud Run${NC}"
echo -e "${BLUE}   3. Configure environment and health checks${NC}"
echo ""

# Check prerequisites
echo -e "${BLUE}🔍 Checking prerequisites...${NC}"

# Check Docker
if ! command -v docker &> /dev/null; then
    echo -e "${RED}❌ Docker is not installed${NC}"
    exit 1
fi

# Check if Docker is running
if ! docker info >/dev/null 2>&1; then
    echo -e "${RED}❌ Docker is not running${NC}"
    exit 1
fi

# Check gcloud CLI
if ! command -v gcloud &> /dev/null; then
    echo -e "${RED}❌ Google Cloud CLI (gcloud) is not installed${NC}"
    echo -e "${BLUE}💡 Install it from: https://cloud.google.com/sdk/docs/install${NC}"
    exit 1
fi

# Check if logged in to Docker Hub
if ! docker info | grep -q "Username"; then
    echo -e "${YELLOW}⚠ Not logged in to Docker Hub. Please run 'docker login' first.${NC}"
    exit 1
fi

# Check if logged in to GCP
if ! gcloud auth list --filter=status:ACTIVE --format="value(account)" | grep -q .; then
    echo -e "${YELLOW}⚠ Not logged in to Google Cloud. Please run 'gcloud auth login' first.${NC}"
    exit 1
fi

echo -e "${GREEN}✓ All prerequisites are met${NC}"

# Load environment variables
if [ -f "${PROJECT_DIR}/dockerhub-gcp.env" ]; then
    echo -e "${BLUE}📁 Loading environment variables...${NC}"
    export $(cat "${PROJECT_DIR}/dockerhub-gcp.env" | grep -v '^#' | xargs)
    echo -e "${GREEN}✓ Environment variables loaded${NC}"
else
    echo -e "${YELLOW}⚠ dockerhub-gcp.env not found, using defaults${NC}"
fi

# Default values
DOCKER_HUB_USERNAME=${DOCKER_HUB_USERNAME:-jsburbano07}
DOCKER_HUB_IMAGE_NAME=${DOCKER_HUB_IMAGE_NAME:-million-real-estate-api}
GCP_PROJECT_ID=${GCP_PROJECT_ID:-your-gcp-project-id}

# Validate required variables
if [ "$GCP_PROJECT_ID" = "your-gcp-project-id" ]; then
    echo -e "${RED}❌ Please set GCP_PROJECT_ID in dockerhub-gcp.env${NC}"
    exit 1
fi

echo -e "${BLUE}📋 Configuration:${NC}"
echo -e "${BLUE}   Docker Hub: ${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}${NC}"
echo -e "${BLUE}   GCP Project: ${GCP_PROJECT_ID}${NC}"
echo ""

# Ask for confirmation
read -p "Do you want to continue with the deployment? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo -e "${YELLOW}⚠ Deployment cancelled${NC}"
    exit 0
fi

# Step 1: Deploy to Docker Hub
echo -e "${BLUE}🔄 Step 1: Deploying to Docker Hub...${NC}"
echo -e "${BLUE}=====================================${NC}"

if [ -f "${SCRIPT_DIR}/dockerhub-deploy.sh" ]; then
    bash "${SCRIPT_DIR}/dockerhub-deploy.sh"
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Docker Hub deployment completed successfully${NC}"
    else
        echo -e "${RED}❌ Docker Hub deployment failed${NC}"
        exit 1
    fi
else
    echo -e "${RED}❌ dockerhub-deploy.sh not found${NC}"
    exit 1
fi

echo ""

# Step 2: Deploy to Google Cloud Platform
echo -e "${BLUE}🔄 Step 2: Deploying to Google Cloud Platform...${NC}"
echo -e "${BLUE}===============================================${NC}"

if [ -f "${SCRIPT_DIR}/gcp-deploy.sh" ]; then
    bash "${SCRIPT_DIR}/gcp-deploy.sh"
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Google Cloud Platform deployment completed successfully${NC}"
    else
        echo -e "${RED}❌ Google Cloud Platform deployment failed${NC}"
        exit 1
    fi
else
    echo -e "${RED}❌ gcp-deploy.sh not found${NC}"
    exit 1
fi

echo ""

# Final summary
echo -e "${GREEN}🎉 Complete deployment pipeline finished successfully!${NC}"
echo -e "${BLUE}=================================================${NC}"
echo -e "${BLUE}📋 Summary:${NC}"
echo -e "${BLUE}   ✅ Docker image built and pushed to Docker Hub${NC}"
echo -e "${BLUE}   ✅ API deployed to Google Cloud Run${NC}"
echo -e "${BLUE}   ✅ Environment variables configured${NC}"
echo -e "${BLUE}   ✅ Health checks performed${NC}"
echo ""

# Display next steps
echo -e "${BLUE}📋 Next Steps:${NC}"
echo -e "${BLUE}   1. Configure MongoDB connection (Cloud SQL or external)${NC}"
echo -e "${BLUE}   2. Set up custom domain (optional)${NC}"
echo -e "${BLUE}   3. Configure monitoring and logging${NC}"
echo -e "${BLUE}   4. Set up CI/CD pipeline with GitHub Actions${NC}"
echo ""

# Display useful commands
echo -e "${BLUE}🔧 Useful Commands:${NC}"
echo -e "${BLUE}   View logs: gcloud logs read --project=${GCP_PROJECT_ID} --limit=50${NC}"
echo -e "${BLUE}   Update service: bash ${SCRIPT_DIR}/gcp-deploy.sh${NC}"
echo -e "${BLUE}   View service: gcloud run services describe million-real-estate-api --region=us-central1${NC}"
echo ""

echo -e "${GREEN}🚀 Your Million Real Estate API is now running in the cloud!${NC}"
