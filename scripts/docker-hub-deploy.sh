#!/bin/bash

# 🐳 Script de Despliegue en Docker Hub
# Uso: ./scripts/docker-hub-deploy.sh [version]

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuración
DOCKER_USERNAME=${DOCKER_USERNAME:-"tu-usuario"}
IMAGE_NAME="million-real-estate-api"
VERSION=${1:-"latest"}

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

# Verificar Docker Hub login
check_dockerhub_login() {
    log "Verificando login en Docker Hub..."
    
    if ! docker info | grep -q "Username"; then
        error "No estás logueado en Docker Hub. Ejecuta: docker login"
        exit 1
    fi
    
    success "Logueado en Docker Hub como: $(docker info | grep Username | awk '{print $2}')"
}

# Construir imagen
build_image() {
    log "Construyendo imagen Docker..."
    
    docker build \
        -t ${DOCKER_USERNAME}/${IMAGE_NAME}:${VERSION} \
        -t ${DOCKER_USERNAME}/${IMAGE_NAME}:latest \
        --build-arg BUILD_DATE=$(date -u +'%Y-%m-%dT%H:%M:%SZ') \
        --build-arg VERSION=${VERSION} \
        .
    
    success "Imagen construida: ${DOCKER_USERNAME}/${IMAGE_NAME}:${VERSION}"
}

# Subir imagen a Docker Hub
push_image() {
    log "Subiendo imagen a Docker Hub..."
    
    docker push ${DOCKER_USERNAME}/${IMAGE_NAME}:${VERSION}
    docker push ${DOCKER_USERNAME}/${IMAGE_NAME}:latest
    
    success "Imagen subida exitosamente a Docker Hub"
}

# Crear tag de release
create_release_tag() {
    if [ "$VERSION" != "latest" ]; then
        log "Creando tag de release..."
        git tag -a "v${VERSION}" -m "Release version ${VERSION}"
        git push origin "v${VERSION}"
        success "Tag v${VERSION} creado y subido"
    fi
}

# Función principal
main() {
    log "🚀 Iniciando despliegue en Docker Hub..."
    
    check_dockerhub_login
    build_image
    push_image
    create_release_tag
    
    success "🎉 ¡Despliegue en Docker Hub completado!"
    
    log "📊 Imagen disponible en:"
    log "   https://hub.docker.com/r/${DOCKER_USERNAME}/${IMAGE_NAME}"
    log "   docker pull ${DOCKER_USERNAME}/${IMAGE_NAME}:${VERSION}"
}

main "$@"


