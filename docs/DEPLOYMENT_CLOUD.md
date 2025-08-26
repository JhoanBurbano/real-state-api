# ☁️ **Guía de Despliegue en la Nube - Million Real Estate API**

## 🎯 **Opciones de Despliegue Disponibles**

### **1. 🐳 Docker Hub (Registro de Imágenes)**
- ✅ **Ventajas**: Fácil distribución, versionado, CI/CD
- ✅ **Casos de uso**: Equipos de desarrollo, despliegues múltiples
- ✅ **Costo**: Gratis para repositorios públicos, $5/mes para privados

### **2. ☁️ Google Cloud Platform (GCP)**
- ✅ **Ventajas**: Escalabilidad automática, SSL gratuito, monitoreo
- ✅ **Casos de uso**: Producción, alta disponibilidad
- ✅ **Costo**: Pay-per-use, ~$0.00002400/100ms para Cloud Run

### **3. 🌍 Despliegue Híbrido**
- ✅ **Ventajas**: Máxima flexibilidad, redundancia
- ✅ **Casos de uso**: Multi-cloud, disaster recovery

## 🐳 **Despliegue en Docker Hub**

### **Prerrequisitos:**
1. **Cuenta en Docker Hub** (https://hub.docker.com)
2. **Docker instalado** y funcionando
3. **Logueado en Docker Hub**: `docker login`

### **Paso 1: Configurar Variables de Entorno**
```bash
# En tu terminal o .bashrc
export DOCKER_USERNAME="tu-usuario-dockerhub"
```

### **Paso 2: Ejecutar Despliegue**
```bash
# Opción 1: Script automático
./scripts/docker-hub-deploy.sh v1.0.0

# Opción 2: Script maestro
./scripts/deploy-all.sh
# Selecciona opción 2: "Subir a Docker Hub"
```

### **Paso 3: Usar la Imagen**
```bash
# Descargar imagen
docker pull tu-usuario/million-real-estate-api:latest

# Ejecutar con docker-compose
docker-compose -f docker-compose.dockerhub.yml up -d
```

### **URLs Disponibles:**
- **🌐 API**: http://localhost:5000
- **📚 Swagger**: http://localhost:5000/swagger
- **🏥 Health**: http://localhost:5000/health/live

## ☁️ **Despliegue en Google Cloud Platform**

### **Prerrequisitos:**
1. **Cuenta de GCP** con facturación habilitada
2. **Google Cloud CLI** instalado: `gcloud init`
3. **Proyecto GCP** creado
4. **APIs habilitadas**: Cloud Build, Cloud Run, Container Registry

### **Paso 1: Instalar Google Cloud CLI**
```bash
# macOS
brew install google-cloud-sdk

# Linux
curl https://sdk.cloud.google.com | bash
exec -l $SHELL
```

### **Paso 2: Configurar GCP**
```bash
# Inicializar
gcloud init

# Seleccionar proyecto
gcloud config set project TU_PROJECT_ID

# Habilitar APIs
gcloud services enable cloudbuild.googleapis.com
gcloud services enable run.googleapis.com
gcloud services enable containerregistry.googleapis.com
```

### **Paso 3: Configurar Variables de Entorno**
```bash
export GCP_PROJECT_ID="tu-proyecto-id"
export GCP_REGION="us-central1"
export MONGO_URI="mongodb+srv://user:pass@cluster.mongodb.net/million"
export BLOB_READ_WRITE_TOKEN="tu_token_vercel"
export AUTH_JWT_PRIVATE_KEY="tu_clave_privada"
export AUTH_JWT_PUBLIC_KEY="tu_clave_publica"
export AUTH_JWT_ISSUER="https://tudominio.com"
export AUTH_JWT_AUDIENCE="https://tudominio.com"
export CORS_ORIGINS="https://tudominio.com"
```

### **Paso 4: Ejecutar Despliegue**
```bash
# Opción 1: Script automático
./scripts/gcp-deploy.sh tu-proyecto-id us-central1

# Opción 2: Script maestro
./scripts/deploy-all.sh
# Selecciona opción 3: "Desplegar en Google Cloud Platform"
```

### **Paso 5: Verificar Despliegue**
```bash
# Listar servicios
gcloud run services list --region=us-central1

# Ver logs
gcloud logs read --service=million-api --limit=50
```

## 🌍 **Despliegue Híbrido (Docker Hub + GCP)**

### **Ejecutar Despliegue Completo:**
```bash
./scripts/deploy-all.sh
# Selecciona opción 4: "Despliegue Completo"
```

### **Flujo del Despliegue Híbrido:**
1. **🏗️ Construir** imagen localmente
2. **🏢 Subir** a Docker Hub (versionado)
3. **☁️ Desplegar** en GCP Cloud Run
4. **✅ Verificar** funcionamiento
5. **📊 Monitorear** logs y métricas

## 🔧 **Configuración Avanzada**

### **Variables de Entorno por Entorno:**

#### **Desarrollo Local (.env)**
```bash
ASPNETCORE_ENVIRONMENT=Development
MONGO_URI=mongodb://localhost:27017
LOG_LEVEL=Debug
RATE_LIMIT_PERMINUTE=1000
```

#### **Docker Hub (.env)**
```bash
ASPNETCORE_ENVIRONMENT=Staging
MONGO_URI=mongodb://mongodb:27017
LOG_LEVEL=Information
RATE_LIMIT_PERMINUTE=500
```

#### **GCP Cloud Run**
```bash
ASPNETCORE_ENVIRONMENT=Production
MONGO_URI=mongodb+srv://user:pass@cluster.mongodb.net/million
LOG_LEVEL=Warning
RATE_LIMIT_PERMINUTE=120
```

### **Configuración de MongoDB:**

#### **Opción 1: MongoDB Local (Docker)**
```yaml
# docker-compose.yml
mongodb:
  image: mongo:7.0
  environment:
    MONGO_INITDB_ROOT_USERNAME: admin
    MONGO_INITDB_ROOT_PASSWORD: password123
```

#### **Opción 2: MongoDB Atlas (GCP)**
```bash
# Connection string
MONGO_URI=mongodb+srv://username:password@cluster.mongodb.net/million?retryWrites=true&w=majority
```

## 📊 **Monitoreo y Logs**

### **Docker Hub:**
```bash
# Ver logs de contenedor
docker logs million-api

# Ver logs en tiempo real
docker logs -f million-api

# Estadísticas del contenedor
docker stats million-api
```

### **GCP Cloud Run:**
```bash
# Ver logs del servicio
gcloud logs read --service=million-api --limit=100

# Ver logs en tiempo real
gcloud logs tail --service=million-api

# Métricas del servicio
gcloud run services describe million-api --region=us-central1
```

### **Health Checks:**
```bash
# Health check local
curl http://localhost:5000/health/live

# Health check GCP
curl https://tu-servicio-abc123.run.app/health/live
```

## 🚨 **Solución de Problemas**

### **Problemas Comunes en Docker Hub:**

#### **1. Error de Autenticación**
```bash
# Solución: Loguearse en Docker Hub
docker login
# Ingresa tu username y password
```

#### **2. Imagen no se sube**
```bash
# Verificar que estés logueado
docker info | grep Username

# Forzar push
docker push --all-tags tu-usuario/million-real-estate-api
```

### **Problemas Comunes en GCP:**

#### **1. Error de permisos**
```bash
# Verificar permisos
gcloud auth list

# Configurar cuenta de servicio
gcloud auth application-default login
```

#### **2. API no habilitada**
```bash
# Habilitar APIs necesarias
gcloud services enable cloudbuild.googleapis.com
gcloud services enable run.googleapis.com
```

#### **3. Error de facturación**
```bash
# Verificar facturación
gcloud billing accounts list

# Habilitar facturación en el proyecto
gcloud billing projects link TU_PROJECT_ID --billing-account=ACCOUNT_ID
```

## 💰 **Estimación de Costos**

### **Docker Hub:**
- **Repositorio público**: Gratis
- **Repositorio privado**: $5/mes
- **Pull requests**: Gratis (hasta 200/día)

### **GCP Cloud Run:**
- **CPU**: $0.00002400/100ms
- **Memoria**: $0.00000250/GB-segundo
- **Requests**: $0.40/millón
- **Ejemplo mensual**: ~$15-30 para tráfico moderado

### **MongoDB Atlas:**
- **Free Tier**: 512MB, gratis
- **Shared Cluster**: $9/mes
- **Dedicated Cluster**: $57/mes+

## 🔒 **Seguridad y Mejores Prácticas**

### **Docker Hub:**
1. **🔐 Usar repositorios privados** para código sensible
2. **🏷️ Versionar imágenes** con tags semánticos
3. **🔍 Escanear imágenes** en busca de vulnerabilidades
4. **🗑️ Limpiar imágenes antiguas** regularmente

### **GCP:**
1. **🔐 Usar IAM** para control de acceso granular
2. **🌐 Configurar VPC** para aislamiento de red
3. **🔑 Rotar claves** regularmente
4. **📊 Habilitar auditoría** y logging

### **General:**
1. **🔑 Nunca commitear** secretos en el código
2. **🌐 Usar HTTPS** en producción
3. **📝 Documentar** cambios y configuraciones
4. **🧪 Probar** en staging antes de producción

## 📚 **Recursos Adicionales**

- **🐳 [Docker Hub Documentation](https://docs.docker.com/docker-hub/)**
- **☁️ [Google Cloud Run Documentation](https://cloud.google.com/run/docs)**
- **🍃 [MongoDB Atlas Documentation](https://docs.atlas.mongodb.com/)**
- **🔐 [GCP Security Best Practices](https://cloud.google.com/security/best-practices)**

## 🆘 **Soporte**

### **Para problemas con Docker Hub:**
- 📧 Support: https://hub.docker.com/support/
- 💬 Community: https://forums.docker.com/

### **Para problemas con GCP:**
- 📧 Support: https://cloud.google.com/support
- 💬 Community: https://stackoverflow.com/questions/tagged/google-cloud-platform

---

**¡Feliz Despliegue en la Nube! 🚀☁️**


