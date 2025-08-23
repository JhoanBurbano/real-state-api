#!/bin/bash

# 🚀 Script de Despliegue - Million Real Estate API
# Uso: ./deploy.sh [environment]

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Función para logging
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

# Verificar si Docker está instalado
check_docker() {
    if ! command -v docker &> /dev/null; then
        error "Docker no está instalado. Por favor instala Docker primero."
        exit 1
    fi
    
    if ! command -v docker-compose &> /dev/null; then
        error "Docker Compose no está instalado. Por favor instala Docker Compose primero."
        exit 1
    fi
    
    success "Docker y Docker Compose están instalados"
}

# Verificar si el directorio scripts existe
check_scripts_dir() {
    if [ ! -d "scripts" ]; then
        log "Creando directorio scripts..."
        mkdir -p scripts
    fi
}

# Generar claves JWT para producción
generate_jwt_keys() {
    log "Generando claves JWT para producción..."
    
    # Crear directorio para claves si no existe
    mkdir -p keys
    
    # Generar clave privada RSA
    openssl genrsa -out keys/private_key.pem 2048 2>/dev/null || {
        warning "No se pudo generar la clave privada RSA. Usando clave de ejemplo."
        echo "-----BEGIN RSA PRIVATE KEY-----" > keys/private_key.pem
        echo "MIIEpAIBAAKCAQEA..." >> keys/private_key.pem
        echo "-----END RSA PRIVATE KEY-----" >> keys/private_key.pem
    }
    
    # Generar clave pública RSA
    openssl rsa -in keys/private_key.pem -pubout -out keys/public_key.pem 2>/dev/null || {
        warning "No se pudo generar la clave pública RSA. Usando clave de ejemplo."
        echo "-----BEGIN PUBLIC KEY-----" > keys/public_key.pem
        echo "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A..." >> keys/public_key.pem
        echo "-----END PUBLIC KEY-----" >> keys/public_key.pem
    }
    
    success "Claves JWT generadas"
}

# Crear archivo .env si no existe
create_env_file() {
    if [ ! -f ".env" ]; then
        log "Creando archivo .env..."
        cat > .env << EOF
# Million Real Estate API - Environment Variables
ASPNETCORE_ENVIRONMENT=Production

# MongoDB Configuration
MONGO_URI=mongodb://million_app:million_app_password@localhost:27017
MONGO_DB=million

# CORS
CORS_ORIGINS=http://localhost:3000,https://yourdomain.com

# Rate Limiting
RATE_LIMIT_PERMINUTE=120
RATE_LIMIT_BURST=200
RATE_LIMIT_ENABLE_BURST=true

# Logging
LOG_LEVEL=Information

# Vercel Blob (configurar con tus tokens reales)
BLOB_READ_WRITE_TOKEN=your_vercel_blob_token_here

# Media Limits
FEATURED_MEDIA_LIMIT=12
MEDIA_LIBRARY_LIMIT=60
MAX_UPLOAD_MB=25
ENABLE_VIDEO=false

# JWT Configuration
AUTH_JWT_ISSUER=https://yourdomain.com
AUTH_JWT_AUDIENCE=https://yourdomain.com
AUTH_JWT_PRIVATE_KEY=your_private_key_here
AUTH_JWT_PUBLIC_KEY=your_public_key_here
AUTH_ACCESS_TTL_MIN=10
AUTH_REFRESH_TTL_DAYS=14
AUTH_LOCKOUT_ATTEMPTS=5
AUTH_LOCKOUT_WINDOW_MIN=15
EOF
        warning "Archivo .env creado. Por favor configura las variables según tu entorno."
    else
        success "Archivo .env ya existe"
    fi
}

# Construir y desplegar
deploy() {
    log "🚀 Iniciando despliegue de Million Real Estate API..."
    
    # Parar contenedores existentes
    log "Deteniendo contenedores existentes..."
    docker-compose down --remove-orphans || true
    
    # Limpiar imágenes antiguas
    log "Limpiando imágenes antiguas..."
    docker system prune -f || true
    
    # Construir y levantar servicios
    log "Construyendo y levantando servicios..."
    docker-compose up --build -d
    
    # Esperar a que los servicios estén listos
    log "Esperando a que los servicios estén listos..."
    sleep 30
    
    # Verificar estado de los servicios
    log "Verificando estado de los servicios..."
    docker-compose ps
    
    success "🎉 ¡Despliegue completado exitosamente!"
    
    log "📊 Servicios disponibles:"
    log "   🌐 API: http://localhost:5000"
    log "   📚 Swagger: http://localhost:5000/swagger"
    log "   🗄️  MongoDB: localhost:27017"
    log "   🖥️  Mongo Express: http://localhost:8081 (admin/password123)"
    log "   🏥 Health Check: http://localhost:5000/health/live"
}

# Función principal
main() {
    log "🚀 Iniciando proceso de despliegue..."
    
    check_docker
    check_scripts_dir
    generate_jwt_keys
    create_env_file
    deploy
    
    success "¡Despliegue completado! Revisa los logs con: docker-compose logs -f"
}

# Ejecutar función principal
main "$@"

