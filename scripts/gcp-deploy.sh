#!/bin/bash

# ☁️ Script de Despliegue en Google Cloud Platform
# Uso: ./scripts/gcp-deploy.sh [project-id] [region]

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuración
PROJECT_ID=${1:-"tu-proyecto-id"}
REGION=${2:-"us-central1"}
SERVICE_NAME="million-api"
IMAGE_NAME="gcr.io/${PROJECT_ID}/million-real-estate-api"

log() {
    echo -e "${BLUE}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Verificar GCP CLI
check_gcp_cli() {
    log "Verificando Google Cloud CLI..."
    
    if ! command -v gcloud &> /dev/null; then
        error "Google Cloud CLI no está instalado."
        error "Instala desde: https://cloud.google.com/sdk/docs/install"
        exit 1
    fi
    
    success "Google Cloud CLI instalado"
}

# Configurar proyecto GCP
setup_gcp_project() {
    log "Configurando proyecto GCP: ${PROJECT_ID}"
    
    # Establecer proyecto
    gcloud config set project ${PROJECT_ID}
    
    # Habilitar APIs necesarias
    gcloud services enable cloudbuild.googleapis.com
    gcloud services enable run.googleapis.com
    gcloud services enable containerregistry.googleapis.com
    gcloud services enable compute.googleapis.com
    
    success "Proyecto configurado y APIs habilitadas"
}

# Construir y subir imagen a Google Container Registry
build_and_push_image() {
    log "Construyendo y subiendo imagen a Google Container Registry..."
    
    # Construir imagen usando Cloud Build
    gcloud builds submit \
        --tag ${IMAGE_NAME} \
        --project ${PROJECT_ID}
    
    success "Imagen subida: ${IMAGE_NAME}"
}

# Desplegar en Cloud Run
deploy_to_cloud_run() {
    log "Desplegando en Cloud Run..."
    
    # Desplegar servicio
    gcloud run deploy ${SERVICE_NAME} \
        --image ${IMAGE_NAME} \
        --platform managed \
        --region ${REGION} \
        --allow-unauthenticated \
        --port 80 \
        --memory 2Gi \
        --cpu 2 \
        --max-instances 10 \
        --set-env-vars="ASPNETCORE_ENVIRONMENT=Production" \
        --set-env-vars="MONGO_URI=${MONGO_URI}" \
        --set-env-vars="MONGO_DB=${MONGO_DB}" \
        --set-env-vars="CORS_ORIGINS=${CORS_ORIGINS}" \
        --set-env-vars="RATE_LIMIT_PERMINUTE=120" \
        --set-env-vars="LOG_LEVEL=Information" \
        --set-env-vars="BLOB_READ_WRITE_TOKEN=${BLOB_READ_WRITE_TOKEN}" \
        --set-env-vars="FEATURED_MEDIA_LIMIT=12" \
        --set-env-vars="MEDIA_LIBRARY_LIMIT=60" \
        --set-env-vars="MAX_UPLOAD_MB=25" \
        --set-env-vars="ENABLE_VIDEO=false" \
        --set-env-vars="AUTH_JWT_ISSUER=${AUTH_JWT_ISSUER}" \
        --set-env-vars="AUTH_JWT_AUDIENCE=${AUTH_JWT_AUDIENCE}" \
        --set-env-vars="AUTH_JWT_PRIVATE_KEY=${AUTH_JWT_PRIVATE_KEY}" \
        --set-env-vars="AUTH_JWT_PUBLIC_KEY=${AUTH_JWT_PUBLIC_KEY}" \
        --set-env-vars="AUTH_ACCESS_TTL_MIN=10" \
        --set-env-vars="AUTH_REFRESH_TTL_DAYS=14" \
        --set-env-vars="AUTH_LOCKOUT_ATTEMPTS=5" \
        --set-env-vars="AUTH_LOCKOUT_WINDOW_MIN=15"
    
    success "Servicio desplegado en Cloud Run"
}

# Configurar MongoDB Atlas (opcional)
setup_mongodb_atlas() {
    log "Configurando MongoDB Atlas..."
    
    warning "Para MongoDB Atlas, necesitas:"
    warning "1. Crear cluster en https://cloud.mongodb.com"
    warning "2. Configurar MONGO_URI con tu connection string"
    warning "3. Configurar variables de entorno"
    
    read -p "¿Quieres configurar MongoDB Atlas ahora? (y/n): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        log "Por favor configura MongoDB Atlas manualmente y actualiza las variables de entorno"
    fi
}

# Mostrar información del despliegue
show_deployment_info() {
    log "📊 Información del despliegue:"
    
    # Obtener URL del servicio
    SERVICE_URL=$(gcloud run services describe ${SERVICE_NAME} --region ${REGION} --format="value(status.url)")
    
    echo "   🌐 URL del servicio: ${SERVICE_URL}"
    echo "   📚 Swagger: ${SERVICE_URL}/swagger"
    echo "   🏥 Health Check: ${SERVICE_URL}/health/live"
    echo "   🗄️  Proyecto GCP: ${PROJECT_ID}"
    echo "   🌍 Región: ${REGION}"
    echo "   🐳 Imagen: ${IMAGE_NAME}"
}

# Función principal
main() {
    log "🚀 Iniciando despliegue en Google Cloud Platform..."
    
    check_gcp_cli
    setup_gcp_project
    build_and_push_image
    deploy_to_cloud_run
    setup_mongodb_atlas
    show_deployment_info
    
    success "🎉 ¡Despliegue en GCP completado!"
    
    log "📋 Próximos pasos:"
    log "   1. Configurar MongoDB Atlas o usar MongoDB local"
    log "   2. Configurar variables de entorno en Cloud Run"
    log "   3. Configurar dominio personalizado (opcional)"
    log "   4. Configurar SSL/TLS"
}

main "$@"


