#!/bin/bash

# 🚀 Script Maestro de Despliegue - Docker Hub + GCP
# Uso: ./scripts/deploy-all.sh [option]

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
NC='\033[0m'

# Configuración
DOCKER_USERNAME=${DOCKER_USERNAME:-"tu-usuario"}
GCP_PROJECT_ID=${GCP_PROJECT_ID:-"tu-proyecto-id"}
GCP_REGION=${GCP_REGION:-"us-central1"}

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

info() {
    echo -e "${PURPLE}[INFO]${NC} $1"
}

# Mostrar menú de opciones
show_menu() {
    echo -e "${BLUE}🚀 Million Real Estate API - Despliegue${NC}"
    echo ""
    echo "Selecciona una opción:"
    echo "1) 🐳 Despliegue Local con Docker"
    echo "2) 🏢 Subir a Docker Hub"
    echo "3) ☁️  Desplegar en Google Cloud Platform"
    echo "4) 🌍 Despliegue Completo (Docker Hub + GCP)"
    echo "5) 📋 Verificar estado de servicios"
    echo "6) 🧹 Limpiar recursos"
    echo "7) ❌ Salir"
    echo ""
}

# Despliegue local
deploy_local() {
    log "🐳 Iniciando despliegue local..."
    ./deploy.sh
}

# Despliegue en Docker Hub
deploy_dockerhub() {
    log "🏢 Iniciando despliegue en Docker Hub..."
    
    # Verificar variables de entorno
    if [ "$DOCKER_USERNAME" = "tu-usuario" ]; then
        error "Configura DOCKER_USERNAME en las variables de entorno"
        read -p "Ingresa tu usuario de Docker Hub: " DOCKER_USERNAME
        export DOCKER_USERNAME
    fi
    
    # Ejecutar script de Docker Hub
    ./scripts/docker-hub-deploy.sh
    
    success "✅ Imagen subida a Docker Hub"
    
    # Mostrar comandos para usar
    log "📋 Para usar la imagen desde Docker Hub:"
    echo "   docker pull ${DOCKER_USERNAME}/million-real-estate-api:latest"
    echo "   docker-compose -f docker-compose.dockerhub.yml up -d"
}

# Despliegue en GCP
deploy_gcp() {
    log "☁️  Iniciando despliegue en Google Cloud Platform..."
    
    # Verificar variables de entorno
    if [ "$GCP_PROJECT_ID" = "tu-proyecto-id" ]; then
        error "Configura GCP_PROJECT_ID en las variables de entorno"
        read -p "Ingresa tu Project ID de GCP: " GCP_PROJECT_ID
        export GCP_PROJECT_ID
    fi
    
    # Ejecutar script de GCP
    ./scripts/gcp-deploy.sh $GCP_PROJECT_ID $GCP_REGION
    
    success "✅ API desplegada en Google Cloud Platform"
}

# Despliegue completo
deploy_complete() {
    log "🌍 Iniciando despliegue completo..."
    
    deploy_dockerhub
    deploy_gcp
    
    success "🎉 ¡Despliegue completo realizado!"
    
    log "📊 Resumen del despliegue:"
    log "   🏢 Docker Hub: ${DOCKER_USERNAME}/million-real-estate-api"
    log "   ☁️  GCP: gcr.io/${GCP_PROJECT_ID}/million-real-estate-api"
}

# Verificar estado de servicios
check_status() {
    log "📋 Verificando estado de servicios..."
    
    echo ""
    echo "🐳 Docker Local:"
    if command -v docker-compose &> /dev/null; then
        docker-compose ps 2>/dev/null || echo "   No hay servicios corriendo"
    else
        echo "   Docker Compose no instalado"
    fi
    
    echo ""
    echo "🏢 Docker Hub:"
    if [ "$DOCKER_USERNAME" != "tu-usuario" ]; then
        echo "   Usuario: ${DOCKER_USERNAME}"
        echo "   Imagen: ${DOCKER_USERNAME}/million-real-estate-api"
    else
        echo "   Usuario no configurado"
    fi
    
    echo ""
    echo "☁️  Google Cloud Platform:"
    if [ "$GCP_PROJECT_ID" != "tu-proyecto-id" ]; then
        echo "   Proyecto: ${GCP_PROJECT_ID}"
        echo "   Región: ${GCP_REGION}"
        if command -v gcloud &> /dev/null; then
            gcloud run services list --region=$GCP_REGION --filter="metadata.name=million-api" --format="table(metadata.name,status.url,status.conditions[0].status)" 2>/dev/null || echo "   No hay servicios corriendo"
        else
            echo "   Google Cloud CLI no instalado"
        fi
    else
        echo "   Proyecto no configurado"
    fi
}

# Limpiar recursos
cleanup() {
    log "🧹 Limpiando recursos..."
    
    # Limpiar Docker local
    if command -v docker-compose &> /dev/null; then
        log "Deteniendo servicios locales..."
        docker-compose down --remove-orphans 2>/dev/null || true
    fi
    
    # Limpiar imágenes Docker
    log "Limpiando imágenes Docker..."
    docker system prune -f || true
    
    # Limpiar GCP (opcional)
    if [ "$GCP_PROJECT_ID" != "tu-proyecto-id" ] && command -v gcloud &> /dev/null; then
        read -p "¿Quieres eliminar el servicio de Cloud Run? (y/n): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            log "Eliminando servicio de Cloud Run..."
            gcloud run services delete million-api --region=$GCP_REGION --quiet 2>/dev/null || true
        fi
    fi
    
    success "✅ Limpieza completada"
}

# Función principal
main() {
    while true; do
        show_menu
        read -p "Selecciona una opción (1-7): " -n 1 -r
        echo ""
        echo ""
        
        case $REPLY in
            1)
                deploy_local
                ;;
            2)
                deploy_dockerhub
                ;;
            3)
                deploy_gcp
                ;;
            4)
                deploy_complete
                ;;
            5)
                check_status
                ;;
            6)
                cleanup
                ;;
            7)
                log "👋 ¡Hasta luego!"
                exit 0
                ;;
            *)
                error "Opción inválida. Selecciona 1-7."
                ;;
        esac
        
        echo ""
        read -p "Presiona Enter para continuar..."
        echo ""
    done
}

# Verificar si se ejecuta desde el directorio raíz
if [ ! -f "deploy.sh" ]; then
    error "Ejecuta este script desde el directorio raíz del proyecto"
    exit 1
fi

# Ejecutar función principal
main "$@"


