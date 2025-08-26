#!/bin/bash

# Million Real Estate API - Google Cloud Platform Deployment Script
# This script deploys the API from Docker Hub to Google Cloud Run

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Load environment variables
if [ -f "../dockerhub-gcp.env" ]; then
    export $(cat ../dockerhub-gcp.env | grep -v '^#' | xargs)
    echo -e "${BLUE}✓ Environment variables loaded from dockerhub-gcp.env${NC}"
else
    echo -e "${YELLOW}⚠ dockerhub-gcp.env not found, using defaults${NC}"
fi

# Default values
DOCKER_HUB_USERNAME=${DOCKER_HUB_USERNAME:-jsburbano07}
DOCKER_HUB_IMAGE_NAME=${DOCKER_HUB_IMAGE_NAME:-million-real-estate-api}
DOCKER_HUB_TAG=${DOCKER_HUB_TAG:-latest}
GCP_PROJECT_ID=${GCP_PROJECT_ID:-your-gcp-project-id}
GCP_REGION=${GCP_REGION:-us-central1}
CLOUD_RUN_SERVICE_NAME=${CLOUD_RUN_SERVICE_NAME:-million-real-estate-api}
CLOUD_RUN_MEMORY=${CLOUD_RUN_MEMORY:-1Gi}
CLOUD_RUN_CPU=${CLOUD_RUN_CPU:-1}
CLOUD_RUN_MAX_INSTANCES=${CLOUD_RUN_MAX_INSTANCES:-10}
CLOUD_RUN_MIN_INSTANCES=${CLOUD_RUN_MIN_INSTANCES:-0}

# Check if gcloud CLI is installed
if ! command -v gcloud &> /dev/null; then
    echo -e "${RED}❌ Google Cloud CLI (gcloud) is not installed.${NC}"
    echo -e "${BLUE}💡 Install it from: https://cloud.google.com/sdk/docs/install${NC}"
    exit 1
fi

# Check if logged in to GCP
if ! gcloud auth list --filter=status:ACTIVE --format="value(account)" | grep -q .; then
    echo -e "${YELLOW}⚠ Not logged in to Google Cloud. Please run 'gcloud auth login' first.${NC}"
    exit 1
fi

echo -e "${BLUE}🚀 Starting Google Cloud Platform deployment...${NC}"
echo -e "${BLUE}📦 Image: ${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${DOCKER_HUB_TAG}${NC}"
echo -e "${BLUE}☁️  Project: ${GCP_PROJECT_ID}${NC}"
echo -e "${BLUE}🌍 Region: ${GCP_REGION}${NC}"
echo -e "${BLUE}🏃 Service: ${CLOUD_RUN_SERVICE_NAME}${NC}"

# Set the active project
echo -e "${BLUE}🔧 Setting active project...${NC}"
gcloud config set project "${GCP_PROJECT_ID}"

# Enable required APIs
echo -e "${BLUE}🔌 Enabling required APIs...${NC}"
gcloud services enable run.googleapis.com
gcloud services enable cloudbuild.googleapis.com
gcloud services enable containerregistry.googleapis.com
gcloud services enable cloudresourcemanager.googleapis.com

# Check if Cloud Run service exists
SERVICE_EXISTS=$(gcloud run services list --region="${GCP_REGION}" --filter="metadata.name=${CLOUD_RUN_SERVICE_NAME}" --format="value(metadata.name)")

if [ -n "${SERVICE_EXISTS}" ]; then
    echo -e "${BLUE}🔄 Updating existing Cloud Run service...${NC}"
    
    # Update the service with new image
    gcloud run services replace \
        --region="${GCP_REGION}" \
        --image="${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${DOCKER_HUB_TAG}" \
        --memory="${CLOUD_RUN_MEMORY}" \
        --cpu="${CLOUD_RUN_CPU}" \
        --max-instances="${CLOUD_RUN_MAX_INSTANCES}" \
        --min-instances="${CLOUD_RUN_MIN_INSTANCES}" \
        --allow-unauthenticated \
        --port=80 \
        --set-env-vars="ASPNETCORE_ENVIRONMENT=Production" \
        --set-env-vars="ASPNETCORE_URLS=http://+:80" \
        --set-env-vars="MONGO_URI=${MONGO_URI}" \
        --set-env-vars="MONGO_DB=${MONGO_DB}" \
        --set-env-vars="JWT_SECRET=${JWT_SECRET}" \
        --set-env-vars="JWT_ISSUER=${JWT_ISSUER}" \
        --set-env-vars="JWT_AUDIENCE=${JWT_AUDIENCE}" \
        --set-env-vars="RATE_LIMITING_MAX_REQUESTS=${RATE_LIMITING_MAX_REQUESTS}" \
        --set-env-vars="RATE_LIMITING_WINDOW_MINUTES=${RATE_LIMITING_WINDOW_MINUTES}" \
        --set-env-vars="LOG_LEVEL_DEFAULT=${LOG_LEVEL}" \
        --set-env-vars="LOG_LEVEL_MICROSOFT_ASPNETCORE=${LOG_LEVEL_ASPNET}" \
        "${CLOUD_RUN_SERVICE_NAME}"
else
    echo -e "${BLUE}🆕 Creating new Cloud Run service...${NC}"
    
    # Create the service
    gcloud run deploy "${CLOUD_RUN_SERVICE_NAME}" \
        --region="${GCP_REGION}" \
        --image="${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${DOCKER_HUB_TAG}" \
        --memory="${CLOUD_RUN_MEMORY}" \
        --cpu="${CLOUD_RUN_CPU}" \
        --max-instances="${CLOUD_RUN_MAX_INSTANCES}" \
        --min-instances="${CLOUD_RUN_MIN_INSTANCES}" \
        --allow-unauthenticated \
        --port=80 \
        --set-env-vars="ASPNETCORE_ENVIRONMENT=Production" \
        --set-env-vars="ASPNETCORE_URLS=http://+:80" \
        --set-env-vars="MONGO_URI=${MONGO_URI}" \
        --set-env-vars="MONGO_DB=${MONGO_DB}" \
        --set-env-vars="JWT_SECRET=${JWT_SECRET}" \
        --set-env-vars="JWT_ISSUER=${JWT_ISSUER}" \
        --set-env-vars="JWT_AUDIENCE=${JWT_AUDIENCE}" \
        --set-env-vars="RATE_LIMITING_MAX_REQUESTS=${RATE_LIMITING_MAX_REQUESTS}" \
        --set-env-vars="RATE_LIMITING_WINDOW_MINUTES=${RATE_LIMITING_WINDOW_MINUTES}" \
        --set-env-vars="LOG_LEVEL_DEFAULT=${LOG_LEVEL}" \
        --set-env-vars="LOG_LEVEL_MICROSOFT_ASPNETCORE=${LOG_LEVEL_ASPNET}"
fi

# Get the service URL
SERVICE_URL=$(gcloud run services describe "${CLOUD_RUN_SERVICE_NAME}" --region="${GCP_REGION}" --format="value(status.url)")

echo -e "${GREEN}✓ Cloud Run service deployed successfully!${NC}"
echo -e "${BLUE}🌐 Service URL: ${SERVICE_URL}${NC}"

# Health check
echo -e "${BLUE}🏥 Performing health check...${NC}"
sleep 10

if curl -f "${SERVICE_URL}/health" >/dev/null 2>&1; then
    echo -e "${GREEN}✓ Health check passed${NC}"
else
    echo -e "${YELLOW}⚠ Health check failed, but service is deployed${NC}"
fi

# Display service information
echo -e "${BLUE}📋 Service Information:${NC}"
gcloud run services describe "${CLOUD_RUN_SERVICE_NAME}" \
    --region="${GCP_REGION}" \
    --format="table(metadata.name,status.url,spec.template.spec.containers[0].image,spec.template.spec.containers[0].resources.limits.memory,spec.template.spec.containers[0].resources.limits.cpu)"

# Create deployment info file
cat > "../gcp-deployment-info.txt" << EOF
Google Cloud Platform Deployment Completed
=========================================
Timestamp: $(date)
Service Name: ${CLOUD_RUN_SERVICE_NAME}
Service URL: ${SERVICE_URL}
Region: ${GCP_REGION}
Project: ${GCP_PROJECT_ID}
Image: ${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${DOCKER_HUB_TAG}
Memory: ${CLOUD_RUN_MEMORY}
CPU: ${CLOUD_RUN_CPU}
Max Instances: ${CLOUD_RUN_MAX_INSTANCES}
Min Instances: ${CLOUD_RUN_MIN_INSTANCES}

Environment Variables:
- ASPNETCORE_ENVIRONMENT: Production
- ASPNETCORE_URLS: http://+:80
- MONGO__URI: ${MONGO_URI}
- MONGO__DATABASE: ${MONGO_DATABASE}
- JWT__SECRET: [CONFIGURED]
- JWT__ISSUER: ${JWT_ISSUER}
- JWT__AUDIENCE: ${JWT_AUDIENCE}
- RATE_LIMITING__MAX_REQUESTS: ${RATE_LIMITING_MAX_REQUESTS}
- RATE_LIMITING__WINDOW_MINUTES: ${RATE_LIMITING_WINDOW_MINUTES}
- LOG_LEVEL__DEFAULT: ${LOG_LEVEL}
- LOG_LEVEL__MICROSOFT_ASPNETCORE: ${LOG_LEVEL_ASPNET}

Next Steps:
1. Configure MongoDB connection (Cloud SQL or external)
2. Set up custom domain (optional)
3. Configure monitoring and logging
4. Set up CI/CD pipeline
EOF

echo -e "${GREEN}✓ Deployment info saved to gcp-deployment-info.txt${NC}"
echo -e "${GREEN}🎉 Google Cloud Platform deployment completed successfully!${NC}"
echo -e "${BLUE}💡 Your API is now running at: ${SERVICE_URL}${NC}"
echo -e "${BLUE}🔧 Manage your service at: https://console.cloud.google.com/run/detail/${GCP_REGION}/${CLOUD_RUN_SERVICE_NAME}${NC}"


