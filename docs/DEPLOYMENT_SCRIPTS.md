# 🚀 **Deployment Scripts - Million Real Estate API**

## 📋 **Available Scripts**

### **Primary Deployment Scripts**

#### **`scripts/deploy-dockerhub-gcp.sh`** ⭐ **MAIN SCRIPT**
- **Purpose**: Complete deployment pipeline from Docker Hub to Google Cloud Run
- **Usage**: `./scripts/deploy-dockerhub-gcp.sh`
- **What it does**:
  1. Checks prerequisites (Docker, gcloud, environment variables)
  2. Builds Docker image
  3. Pushes to Docker Hub
  4. Deploys to Google Cloud Run
- **Requirements**: 
  - Docker Hub credentials
  - Google Cloud CLI configured
  - `dockerhub-gcp.env` file with environment variables

#### **`scripts/dockerhub-deploy.sh`**
- **Purpose**: Build and push Docker image to Docker Hub
- **Usage**: `./scripts/dockerhub-deploy.sh`
- **What it does**:
  1. Builds Docker image with proper platform (linux/amd64)
  2. Tags image with version
  3. Pushes to Docker Hub
- **Requirements**: Docker Hub credentials

#### **`scripts/gcp-deploy.sh`**
- **Purpose**: Deploy Docker image from Docker Hub to Google Cloud Run
- **Usage**: `./scripts/gcp-deploy.sh`
- **What it does**:
  1. Authenticates with Google Cloud
  2. Creates/updates Cloud Run service
  3. Configures environment variables
  4. Sets resource limits and scaling
- **Requirements**: Google Cloud CLI configured

### **Environment Configuration Files**

#### **`dockerhub-gcp.env`** ⭐ **MAIN CONFIG**
- **Purpose**: Environment variables for Docker Hub and GCP deployment
- **Contains**:
  - MongoDB Atlas connection string
  - JWT configuration
  - Google Cloud project ID
  - Rate limiting settings
- **Usage**: Source this file before running deployment scripts

#### **`cloudrun-service.yaml`**
- **Purpose**: Cloud Run service configuration template
- **Usage**: Used by `gcp-deploy.sh` for service configuration
- **Contains**: Service specification with environment variables

### **CI/CD Workflows**

#### **`.github/workflows/dockerhub-gcp-deploy.yml`**
- **Purpose**: GitHub Actions workflow for automated deployment
- **Triggers**: Push to main branch
- **What it does**:
  1. Builds Docker image
  2. Pushes to Docker Hub
  3. Deploys to Google Cloud Run
- **Requirements**: GitHub repository with secrets configured

## 🚀 **Quick Deployment Guide**

### **1. One-Command Deployment**
```bash
# Make script executable
chmod +x scripts/deploy-dockerhub-gcp.sh

# Run complete deployment
./scripts/deploy-dockerhub-gcp.sh
```

### **2. Step-by-Step Deployment**
```bash
# Step 1: Build and push to Docker Hub
./scripts/dockerhub-deploy.sh

# Step 2: Deploy to Google Cloud Run
./scripts/gcp-deploy.sh
```

### **3. Manual Deployment**
```bash
# Build image
docker build --platform linux/amd64 -t jsburbano07/million-real-estate-api:latest .

# Push to Docker Hub
docker push jsburbano07/million-real-estate-api:latest

# Deploy to GCP
gcloud run deploy million-real-estate-api \
  --image jsburbano07/million-real-estate-api:latest \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated
```

## 🔧 **Prerequisites**

### **Required Tools**
- **Docker**: For building and running containers
- **Google Cloud CLI**: For GCP deployment
- **Git**: For version control

### **Required Accounts**
- **Docker Hub**: For storing Docker images
- **Google Cloud**: For hosting the API

### **Required Permissions**
- **Docker Hub**: Push access to repository
- **Google Cloud**: Cloud Run Admin, Service Account User

## 📝 **Environment Variables**

### **MongoDB Configuration**
```bash
MONGO_URI=mongodb+srv://username:password@cluster.mongodb.net/database
MONGO_DB=million
```

### **JWT Configuration**
```bash
JWT_SECRET=your-secret-key
JWT_ISSUER=https://your-domain.com
JWT_AUDIENCE=https://your-app.com
```

### **Google Cloud Configuration**
```bash
GCP_PROJECT_ID=your-project-id
```

### **API Configuration**
```bash
RATE_LIMITING_MAX_REQUESTS=100
RATE_LIMITING_WINDOW_MINUTES=1
LOG_LEVEL_DEFAULT=Information
LOG_LEVEL_MICROSOFT_ASPNETCORE=Information
```

## 🔍 **Troubleshooting**

### **Common Issues**

#### **Docker Build Fails**
```bash
# Check Docker is running
docker info

# Check Dockerfile syntax
docker build --no-cache .
```

#### **Docker Push Fails**
```bash
# Login to Docker Hub
docker login

# Check image exists
docker images | grep million-real-estate-api
```

#### **GCP Deployment Fails**
```bash
# Check authentication
gcloud auth list

# Check project
gcloud config get-value project

# Check permissions
gcloud projects get-iam-policy YOUR_PROJECT_ID
```

#### **API Not Responding**
```bash
# Check service status
gcloud run services describe million-real-estate-api --region=us-central1

# Check logs
gcloud logging read "resource.type=cloud_run_revision" --limit=20
```

### **Debug Commands**
```bash
# Test API locally
docker run -p 8080:80 jsburbano07/million-real-estate-api:latest

# Check container logs
docker logs CONTAINER_ID

# Test MongoDB connection
mongosh "YOUR_MONGO_URI"
```

## 📊 **Monitoring Deployment**

### **Health Checks**
```bash
# Liveness probe
curl "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/health/live"

# Readiness probe
curl "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/health/ready"
```

### **Service Status**
```bash
# Check service status
gcloud run services list --region=us-central1

# Check revisions
gcloud run revisions list --service=million-real-estate-api --region=us-central1
```

### **Logs and Metrics**
```bash
# View logs
gcloud logging read "resource.type=cloud_run_revision" --limit=50

# Check metrics
gcloud monitoring metrics list --filter="metric.type:run.googleapis.com"
```

## 🔄 **Updating Deployment**

### **Redeploy After Code Changes**
```bash
# Push code changes
git push origin main

# GitHub Actions will automatically deploy
# Or manually run:
./scripts/deploy-dockerhub-gcp.sh
```

### **Update Environment Variables**
```bash
# Edit dockerhub-gcp.env
nano dockerhub-gcp.env

# Redeploy
./scripts/gcp-deploy.sh
```

### **Rollback to Previous Version**
```bash
# List revisions
gcloud run revisions list --service=million-real-estate-api --region=us-central1

# Rollback to specific revision
gcloud run services update-traffic million-real-estate-api \
  --to-revisions=REVISION_NAME=100 \
  --region=us-central1
```

## 📚 **Additional Resources**

- **[API Integration Guide](API_INTEGRATION_GUIDE.md)** - Complete API documentation
- **[Architecture](ARCHITECTURE.md)** - System design and architecture
- **[Error Handling](ERROR_HANDLING.md)** - Troubleshooting and error codes
- **[Google Cloud Run Documentation](https://cloud.google.com/run/docs)** - Official GCP docs

---

**Last Updated**: January 2024  
**Scripts Version**: v1.0.0
