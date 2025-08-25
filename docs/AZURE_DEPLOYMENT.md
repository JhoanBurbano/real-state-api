# 🚀 Despliegue en Azure - Million Real Estate API

Esta guía te ayudará a desplegar la Million Real Estate API en Azure usando diferentes métodos.

## 📋 Prerrequisitos

### 1. Herramientas Necesarias
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (versión 2.0+)
- [Docker](https://docs.docker.com/get-docker/) (versión 20.0+)
- [Bicep](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/install) (se instala automáticamente con Azure CLI)
- [jq](https://stedolan.github.io/jq/download/) (para procesar JSON)

### 2. Cuenta de Azure
- Suscripción activa de Azure
- Permisos de administrador o colaborador en la suscripción
- Acceso a Azure Container Registry

## 🎯 Opciones de Despliegue

### Opción 1: Despliegue Manual con Scripts
**Recomendado para:**
- Despliegues iniciales
- Aprendizaje y pruebas
- Configuraciones personalizadas

### Opción 2: Despliegue con Bicep (Infraestructura como Código)
**Recomendado para:**
- Despliegues en producción
- Entornos reproducibles
- CI/CD pipelines
- Gestión de infraestructura

### Opción 3: GitHub Actions (CI/CD Automático)
**Recomendado para:**
- Despliegues automáticos
- Integración continua
- Equipos de desarrollo

## 🚀 Opción 1: Despliegue Manual

### Paso 1: Preparar el Entorno
```bash
# Clonar el repositorio
git clone <your-repo-url>
cd real-state-api

# Hacer ejecutables los scripts
chmod +x scripts/azure-deploy.sh
chmod +x scripts/azure-bicep-deploy.sh

# Copiar y configurar variables de entorno
cp azure.env.template .env.azure
# Editar .env.azure con tus valores reales
```

### Paso 2: Configurar Variables de Entorno
Edita el archivo `.env.azure` con tus valores:

```bash
# MongoDB Configuration
MONGO_URI=mongodb://localhost:27017
MONGO_DATABASE=million
MONGO_ROOT_USERNAME=admin
MONGO_ROOT_PASSWORD=tu_password_seguro_aqui

# JWT Configuration
JWT_SECRET=tu_super_secret_jwt_key_aqui_minimo_32_caracteres
JWT_ISSUER=https://tu-app-azure.azurewebsites.net
JWT_AUDIENCE=https://tu-app-azure.azurewebsites.net

# Azure Configuration
AZURE_APP_NAME=million-real-estate-api
AZURE_RESOURCE_GROUP=million-real-estate-rg
AZURE_LOCATION=eastus
```

### Paso 3: Ejecutar el Despliegue
```bash
# Despliegue completo
./scripts/azure-deploy.sh

# O solo infraestructura con Bicep
./scripts/azure-bicep-deploy.sh
```

## 🏗️ Opción 2: Despliegue con Bicep

### Paso 1: Preparar la Infraestructura
```bash
# Crear grupo de recursos
az group create --name million-real-estate-rg --location eastus

# Desplegar infraestructura
az deployment group create \
  --resource-group million-real-estate-rg \
  --template-file infrastructure/main.bicep \
  --name million-deployment \
  --parameters location=eastus
```

### Paso 2: Configurar la Aplicación
```bash
# Obtener outputs del despliegue
az deployment group show \
  --resource-group million-real-estate-rg \
  --name million-deployment \
  --query properties.outputs

# Configurar variables de entorno
az webapp config appsettings set \
  --resource-group million-real-estate-rg \
  --name million-real-estate-api \
  --settings \
  ASPNETCORE_ENVIRONMENT=Production \
  MONGO__URI="tu_mongo_uri" \
  JWT__SECRET="tu_jwt_secret"
```

## 🔄 Opción 3: GitHub Actions (CI/CD)

### Paso 1: Configurar Secretos en GitHub
Ve a tu repositorio → Settings → Secrets and variables → Actions y agrega:

- `AZURE_CREDENTIALS`: Service Principal credentials
- `ACR_USERNAME`: Azure Container Registry username
- `ACR_PASSWORD`: Azure Container Registry password

### Paso 2: Crear Service Principal
```bash
# Crear service principal
az ad sp create-for-rbac \
  --name "million-api-sp" \
  --role contributor \
  --scopes /subscriptions/<subscription-id>/resourceGroups/million-real-estate-rg \
  --sdk-auth

# Copiar la salida JSON a AZURE_CREDENTIALS en GitHub
```

### Paso 3: Hacer Push al Branch Principal
```bash
git add .
git commit -m "Configure Azure deployment"
git push origin main
```

## 🔧 Configuración Post-Despliegue

### 1. Configurar Dominio Personalizado
```bash
# Agregar dominio personalizado
az webapp config hostname add \
  --webapp-name million-real-estate-api \
  --resource-group million-real-estate-rg \
  --hostname tu-dominio.com

# Configurar SSL
az webapp config ssl bind \
  --certificate-thumbprint <thumbprint> \
  --ssl-type SNI \
  --name million-real-estate-api \
  --resource-group million-real-estate-rg
```

### 2. Configurar Monitoreo
```bash
# Habilitar Application Insights
az webapp config appsettings set \
  --resource-group million-real-estate-rg \
  --name million-real-estate-api \
  --settings \
  APPINSIGHTS_INSTRUMENTATIONKEY="<key>"

# Configurar alertas
az monitor metrics alert create \
  --name "high-cpu-alert" \
  --resource-group million-real-estate-rg \
  --scopes /subscriptions/<subscription-id>/resourceGroups/million-real-estate-rg/providers/Microsoft.Web/sites/million-real-estate-api \
  --condition "avg Percentage CPU > 80" \
  --description "High CPU usage detected"
```

### 3. Configurar Escalado Automático
```bash
# Habilitar escalado automático
az monitor autoscale create \
  --resource-group million-real-estate-rg \
  --resource /subscriptions/<subscription-id>/resourceGroups/million-real-estate-rg/providers/Microsoft.Web/serverfarms/million-asp \
  --resource-type Microsoft.Web/serverfarms \
  --name million-asp-autoscale \
  --min-count 1 \
  --max-count 10 \
  --count 1
```

## 🧪 Pruebas Post-Despliegue

### 1. Verificar Salud de la API
```bash
# Health check
curl -f https://million-real-estate-api.azurewebsites.net/health

# Swagger/OpenAPI
curl -f https://million-real-estate-api.azurewebsites.net/swagger
```

### 2. Probar Endpoints
```bash
# Crear propiedad
curl -X POST https://million-real-estate-api.azurewebsites.net/api/properties \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Property","price":100000}'

# Listar propiedades
curl https://million-real-estate-api.azurewebsites.net/api/properties
```

## 🔍 Monitoreo y Troubleshooting

### 1. Logs de la Aplicación
```bash
# Ver logs en tiempo real
az webapp log tail \
  --name million-real-estate-api \
  --resource-group million-real-estate-rg

# Descargar logs
az webapp log download \
  --name million-real-estate-api \
  --resource-group million-real-estate-rg
```

### 2. Métricas y Performance
```bash
# Ver métricas de CPU
az monitor metrics list \
  --resource /subscriptions/<subscription-id>/resourceGroups/million-real-estate-rg/providers/Microsoft.Web/sites/million-real-estate-api \
  --metric "Percentage CPU" \
  --interval PT1M

# Ver métricas de memoria
az monitor metrics list \
  --resource /subscriptions/<subscription-id>/resourceGroups/million-real-estate-rg/providers/Microsoft.Web/sites/million-real-estate-api \
  --metric "Average Memory Working Set" \
  --interval PT1M
```

### 3. Problemas Comunes

#### Error: "Container failed to start"
```bash
# Verificar configuración del contenedor
az webapp config container show \
  --name million-real-estate-api \
  --resource-group million-real-estate-rg

# Verificar logs del contenedor
az webapp log container show \
  --name million-real-estate-api \
  --resource-group million-real-estate-rg
```

#### Error: "Connection string not found"
```bash
# Verificar variables de entorno
az webapp config appsettings list \
  --name million-real-estate-api \
  --resource-group million-real-estate-rg

# Agregar variable faltante
az webapp config appsettings set \
  --name million-real-estate-api \
  --resource-group million-real-estate-rg \
  --settings MONGO__URI="tu_mongo_uri"
```

## 💰 Costos y Optimización

### 1. SKUs Recomendados
- **Desarrollo/Pruebas**: B1 (Basic) - ~$13/mes
- **Producción**: S1 (Standard) - ~$73/mes
- **Alto Tráfico**: P1V2 (Premium) - ~$146/mes

### 2. Optimización de Costos
```bash
# Escalar a 0 en horarios no laborales (desarrollo)
az webapp config set \
  --name million-real-estate-api \
  --resource-group million-real-estate-rg \
  --min-tls-version 1.2 \
  --http20-enabled true

# Configurar auto-shutdown (desarrollo)
az webapp config set \
  --name million-real-estate-api \
  --resource-group million-real-estate-rg \
  --auto-heal-enabled true
```

## 🔐 Seguridad

### 1. Configurar Key Vault
```bash
# Crear Key Vault
az keyvault create \
  --name million-key-vault \
  --resource-group million-real-estate-rg \
  --location eastus

# Agregar secretos
az keyvault secret set \
  --vault-name million-key-vault \
  --name "JWT-Secret" \
  --value "tu_jwt_secret"

# Configurar acceso de la web app
az webapp config appsettings set \
  --name million-real-estate-api \
  --resource-group million-real-estate-rg \
  --settings \
  JWT__SECRET="@Microsoft.KeyVault(SecretUri=https://million-key-vault.vault.azure.net/secrets/JWT-Secret/)"
```

### 2. Configurar Network Security
```bash
# Restringir acceso por IP (opcional)
az webapp config access-restriction add \
  --resource-group million-real-estate-rg \
  --name million-real-estate-api \
  --rule-name "Allow My IP" \
  --ip-address "tu_ip_publica" \
  --priority 100
```

## 📚 Recursos Adicionales

- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure Container Registry](https://docs.microsoft.com/en-us/azure/container-registry/)
- [Azure Bicep](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/)
- [Azure CLI Reference](https://docs.microsoft.com/en-us/cli/azure/)
- [GitHub Actions for Azure](https://github.com/marketplace?type=actions&query=azure)

## 🆘 Soporte

Si encuentras problemas:

1. **Revisar logs**: Usa `az webapp log tail`
2. **Verificar configuración**: Usa `az webapp config show`
3. **Azure Portal**: Revisa la sección de diagnóstico
4. **GitHub Issues**: Reporta bugs en el repositorio
5. **Azure Support**: Para problemas críticos de infraestructura

---

**¡Tu Million Real Estate API está lista para producción en Azure! 🎉**
