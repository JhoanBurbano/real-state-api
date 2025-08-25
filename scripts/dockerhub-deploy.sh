#!/bin/bash

# Million Real Estate API - Docker Hub Deployment Script
# This script builds and pushes the API image to Docker Hub

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

# Generate version tag
VERSION_TAG=$(date +%Y%m%d-%H%M%S)
GIT_COMMIT=$(git rev-parse --short HEAD 2>/dev/null || echo "unknown")

echo -e "${BLUE}🚀 Starting Docker Hub deployment...${NC}"
echo -e "${BLUE}📦 Image: ${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}${NC}"
echo -e "${BLUE}🏷️  Tags: ${DOCKER_HUB_TAG}, ${VERSION_TAG}, ${GIT_COMMIT}${NC}"

# Check if Docker is running
if ! docker info >/dev/null 2>&1; then
    echo -e "${RED}❌ Docker is not running. Please start Docker and try again.${NC}"
    exit 1
fi

# Check if logged in to Docker Hub
if ! docker info | grep -q "Username"; then
    echo -e "${YELLOW}⚠ Not logged in to Docker Hub. Please run 'docker login' first.${NC}"
    exit 1
fi

# Build the Docker image
echo -e "${BLUE}🔨 Building Docker image...${NC}"
docker build \
    --tag "${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${DOCKER_HUB_TAG}" \
    --tag "${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${VERSION_TAG}" \
    --tag "${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${GIT_COMMIT}" \
    --file Dockerfile \
    .

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Docker image built successfully${NC}"
else
    echo -e "${RED}❌ Failed to build Docker image${NC}"
    exit 1
fi

# Test the image locally (optional)
echo -e "${BLUE}🧪 Testing image locally...${NC}"
docker run --rm -d --name million-test -p 8080:80 "${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${DOCKER_HUB_TAG}" >/dev/null 2>&1

# Wait a moment for the container to start
sleep 5

# Check if container is running
if docker ps | grep -q "million-test"; then
    echo -e "${GREEN}✓ Local test successful${NC}"
    # Stop and remove test container
    docker stop million-test >/dev/null 2>&1
    docker rm million-test >/dev/null 2>&1
else
    echo -e "${YELLOW}⚠ Local test failed, but continuing with push${NC}"
fi

# Push images to Docker Hub
echo -e "${BLUE}📤 Pushing images to Docker Hub...${NC}"

# Push latest tag
echo -e "${BLUE}📤 Pushing latest tag...${NC}"
docker push "${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${DOCKER_HUB_TAG}"

# Push version tag
echo -e "${BLUE}📤 Pushing version tag ${VERSION_TAG}...${NC}"
docker push "${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${VERSION_TAG}"

# Push git commit tag
echo -e "${BLUE}📤 Pushing git commit tag ${GIT_COMMIT}...${NC}"
docker push "${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${GIT_COMMIT}"

echo -e "${GREEN}✓ All images pushed successfully to Docker Hub!${NC}"

# Display image information
echo -e "${BLUE}📋 Image Information:${NC}"
echo -e "${BLUE}   Repository: ${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}${NC}"
echo -e "${BLUE}   Latest: ${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${DOCKER_HUB_TAG}${NC}"
echo -e "${BLUE}   Version: ${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${VERSION_TAG}${NC}"
echo -e "${BLUE}   Commit: ${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${GIT_COMMIT}${NC}"

# Create deployment info file
cat > "../deployment-info.txt" << EOF
Docker Hub Deployment Completed
===============================
Timestamp: $(date)
Repository: ${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}
Tags:
  - Latest: ${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${DOCKER_HUB_TAG}
  - Version: ${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${VERSION_TAG}
  - Commit: ${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${GIT_COMMIT}

Next Steps:
1. Update GCP deployment scripts with these image tags
2. Deploy to Google Cloud Run using: ${DOCKER_HUB_USERNAME}/${DOCKER_HUB_IMAGE_NAME}:${VERSION_TAG}
3. Monitor deployment in GCP Console
EOF

echo -e "${GREEN}✓ Deployment info saved to deployment-info.txt${NC}"
echo -e "${GREEN}🎉 Docker Hub deployment completed successfully!${NC}"
echo -e "${BLUE}💡 Next: Run ./scripts/gcp-deploy.sh to deploy to Google Cloud${NC}"
