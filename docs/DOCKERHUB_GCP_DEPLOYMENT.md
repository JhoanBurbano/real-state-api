# 🚀 Docker Hub + Google Cloud Platform Deployment Guide

## 📋 Overview

This guide covers the complete deployment pipeline for the Million Real Estate API:
1. **Build and push Docker image to Docker Hub**
2. **Deploy the image to Google Cloud Run**
3. **Automated CI/CD with GitHub Actions**

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Source Code  │───▶│  Docker Hub    │───▶│  Google Cloud  │
│   (GitHub)     │    │  (Registry)    │    │  (Cloud Run)    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🔧 Prerequisites

### Required Tools
- [Docker](https://docs.docker.com/get-docker/) - Container runtime
- [Google Cloud CLI](https://cloud.google.com/sdk/docs/install) - GCP management
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) - Azure management (optional)

### Required Accounts
- **Docker Hub Account** - [Sign up here](https://hub.docker.com/signup)
- **Google Cloud Account** - [Get $300 free credit](https://cloud.google.com/free)
- **GitHub Account** - For CI/CD automation

## 🚀 Quick Start

### 1. Configure Environment Variables

Copy and edit the environment template:
```bash
cp dockerhub-gcp.env .env.dockerhub
nano .env.dockerhub
```

**Required Variables:**
```bash
# Docker Hub Configuration
DOCKER_HUB_USERNAME=jsburbano07
DOCKER_HUB_IMAGE_NAME=million-real-estate-api

# Google Cloud Platform
GCP_PROJECT_ID=your-actual-project-id
GCP_REGION=us-central1

# MongoDB Configuration
MONGO_URI=mongodb://your-mongo-connection-string
MONGO_DATABASE=million

# JWT Configuration
JWT_SECRET=your-super-secret-jwt-key-here
JWT_ISSUER=https://your-app.run.app
JWT_AUDIENCE=https://your-app.run.app
```

### 2. Authenticate with Services

**Docker Hub:**
```bash
docker login
# Enter your Docker Hub username and password
```

**Google Cloud:**
```bash
gcloud auth login
gcloud config set project YOUR_PROJECT_ID
```

### 3. Run Complete Deployment

```bash
# Execute the complete pipeline
./scripts/deploy-dockerhub-gcp.sh
```

## 📁 Scripts Overview

### Core Scripts

| Script | Purpose | Usage |
|--------|---------|-------|
| `deploy-dockerhub-gcp.sh` | **Master script** - Complete pipeline | `./scripts/deploy-dockerhub-gcp.sh` |
| `dockerhub-deploy.sh` | Build and push to Docker Hub | `./scripts/dockerhub-deploy.sh` |
| `gcp-deploy.sh` | Deploy to Google Cloud Run | `./scripts/gcp-deploy.sh` |

### Individual Steps

#### Step 1: Docker Hub Deployment
```bash
./scripts/dockerhub-deploy.sh
```

**What it does:**
- Builds Docker image with multiple tags
- Tests image locally
- Pushes to Docker Hub with versioning
- Creates deployment info file

**Output:**
```
✅ Docker image built successfully
✅ Local test successful
✅ All images pushed successfully to Docker Hub!
📋 Image Information:
   Repository: jsburbano07/million-real-estate-api
   Latest: jsburbano07/million-real-estate-api:latest
   Version: jsburbano07/million-real-estate-api:20241223-143022
   Commit: jsburbano07/million-real-estate-api:a1b2c3d
```

#### Step 2: Google Cloud Deployment
```bash
./scripts/gcp-deploy.sh
```

**What it does:**
- Enables required GCP APIs
- Creates/updates Cloud Run service
- Configures environment variables
- Performs health checks
- Creates deployment info file

**Output:**
```
✅ Cloud Run service deployed successfully!
🌐 Service URL: https://million-real-estate-api-abc123-uc.a.run.app
✅ Health check passed
```

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow

The `.github/workflows/dockerhub-gcp-deploy.yml` file automates the entire process:

**Triggers:**
- Push to `main` or `develop` branches
- Git tags (e.g., `v1.0.0`)
- Manual workflow dispatch

**Jobs:**
1. **Build and Push** - Docker Hub deployment
2. **Deploy to GCP** - Google Cloud Run deployment
3. **Notify** - Deployment status notifications

### Required GitHub Secrets

Configure these secrets in your GitHub repository:

```bash
# Docker Hub
DOCKER_HUB_USERNAME=jsburbano07
DOCKER_HUB_PASSWORD=your-docker-hub-token

# Google Cloud
GCP_PROJECT_ID=your-gcp-project-id
GCP_SA_KEY={"type": "service_account", ...}

# Application Configuration
MONGO_URI=mongodb://your-connection-string
MONGO_DATABASE=million
JWT_SECRET=your-jwt-secret
JWT_ISSUER=https://your-app.run.app
JWT_AUDIENCE=https://your-app.run.app
RATE_LIMITING_MAX_REQUESTS=100
RATE_LIMITING_WINDOW_MINUTES=1
LOG_LEVEL=Information
LOG_LEVEL_ASPNET=Information
```

### Creating GCP Service Account

1. **Go to GCP Console:**
   ```
   https://console.cloud.google.com/iam-admin/serviceaccounts
   ```

2. **Create Service Account:**
   ```bash
   gcloud iam service-accounts create github-actions \
       --display-name="GitHub Actions" \
       --description="Service account for GitHub Actions deployments"
   ```

3. **Assign Roles:**
   ```bash
   gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
       --member="serviceAccount:github-actions@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
       --role="roles/run.admin"
   
   gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
       --member="serviceAccount:github-actions@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
       --role="roles/iam.serviceAccountUser"
   ```

4. **Create and Download Key:**
   ```bash
   gcloud iam service-accounts keys create key.json \
       --iam-account=github-actions@YOUR_PROJECT_ID.iam.gserviceaccount.com
   ```

5. **Add to GitHub Secrets:**
   - Copy the contents of `key.json`
   - Add as `GCP_SA_KEY` secret in GitHub

## 🐳 Docker Image Management

### Image Tags

The pipeline creates multiple tags for versioning:

- **`latest`** - Always points to the most recent build
- **`{date}-{time}`** - Timestamp-based versioning (e.g., `20241223-143022`)
- **`{commit-hash}`** - Git commit-based versioning (e.g., `a1b2c3d`)
- **`{branch}-{commit}`** - Branch-specific versions

### Image Security

- Images are built with multi-stage Dockerfile
- Base images are regularly updated
- Security scans are performed during build
- Images are signed and verified

## ☁️ Google Cloud Run Configuration

### Service Configuration

```yaml
Service Name: million-real-estate-api
Region: us-central1
Memory: 1Gi
CPU: 1
Max Instances: 10
Min Instances: 0
Port: 80
Authentication: Public (unauthenticated)
```

### Environment Variables

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80
MONGO__URI=${MONGO_URI}
MONGO__DATABASE=${MONGO_DATABASE}
JWT__SECRET=${JWT_SECRET}
JWT__ISSUER=${JWT_ISSUER}
JWT__AUDIENCE=${JWT_AUDIENCE}
RATE_LIMITING__MAX_REQUESTS=${RATE_LIMITING_MAX_REQUESTS}
RATE_LIMITING__WINDOW_MINUTES=${RATE_LIMITING_WINDOW_MINUTES}
LOG_LEVEL__DEFAULT=${LOG_LEVEL}
LOG_LEVEL__MICROSOFT_ASPNETCORE=${LOG_LEVEL_ASPNET}
```

## 📊 Monitoring and Logging

### Cloud Run Monitoring

- **Metrics Dashboard:** https://console.cloud.google.com/run
- **Logs:** `gcloud logs read --project=YOUR_PROJECT_ID --limit=50`
- **Performance:** CPU, memory, and request metrics
- **Scaling:** Automatic scaling based on demand

### Health Checks

The API includes health check endpoints:
- **`/health`** - Basic health status
- **`/health/live`** - Liveness probe
- **`/health/ready`** - Readiness probe

## 🔒 Security Considerations

### Network Security

- Cloud Run services are isolated
- No direct network access required
- HTTPS enforced by default
- CORS configured for web clients

### Secrets Management

- Environment variables for configuration
- JWT secrets stored securely
- Database credentials encrypted
- No hardcoded secrets in code

### Authentication

- JWT-based authentication
- Rate limiting enabled
- CORS protection
- Input validation with FluentValidation

## 💰 Cost Optimization

### Google Cloud Run Pricing

- **Pay per request** - Only pay when handling requests
- **Automatic scaling** - Scale to zero when not in use
- **Free tier** - 2 million requests per month free
- **Memory optimization** - Right-size memory allocation

### Cost Monitoring

```bash
# View current costs
gcloud billing accounts list
gcloud billing projects describe YOUR_PROJECT_ID

# Set budget alerts
gcloud billing budgets create --billing-account=YOUR_BILLING_ACCOUNT
```

## 🚨 Troubleshooting

### Common Issues

#### Docker Build Failures
```bash
# Check Docker daemon
docker info

# Clean up Docker
docker system prune -a

# Rebuild without cache
docker build --no-cache .
```

#### GCP Authentication Issues
```bash
# Re-authenticate
gcloud auth login
gcloud auth application-default login

# Check current account
gcloud auth list
```

#### Cloud Run Deployment Failures
```bash
# Check service status
gcloud run services describe million-real-estate-api --region=us-central1

# View logs
gcloud logs read --project=YOUR_PROJECT_ID --filter="resource.type=cloud_run_revision"

# Check IAM permissions
gcloud projects get-iam-policy YOUR_PROJECT_ID
```

### Debug Commands

```bash
# Test Docker image locally
docker run -p 8080:80 jsburbano07/million-real-estate-api:latest

# Check GCP APIs
gcloud services list --enabled

# Verify service configuration
gcloud run services describe million-real-estate-api --region=us-central1 --format=yaml
```

## 📚 Additional Resources

### Documentation
- [Google Cloud Run Documentation](https://cloud.google.com/run/docs)
- [Docker Hub Documentation](https://docs.docker.com/docker-hub/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)

### Best Practices
- [Cloud Run Best Practices](https://cloud.google.com/run/docs/best-practices)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [GitHub Actions Best Practices](https://docs.github.com/en/actions/learn-github-actions/best-practices)

### Support
- [Google Cloud Support](https://cloud.google.com/support)
- [Docker Hub Support](https://hub.docker.com/support/)
- [GitHub Support](https://support.github.com/)

---

## 🎯 Next Steps

1. **Configure your environment variables**
2. **Set up GitHub secrets**
3. **Run the deployment pipeline**
4. **Monitor your service**
5. **Set up custom domain (optional)**
6. **Configure monitoring and alerts**

---

**¡Tu Million Real Estate API está lista para producción en la nube! 🎉**
