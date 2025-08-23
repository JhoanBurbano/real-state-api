# 🚀 Guía de Despliegue - Million Real Estate API

## 📋 **Prerrequisitos**

### **Software Requerido:**
- ✅ **Docker** (versión 20.10+)
- ✅ **Docker Compose** (versión 2.0+)
- ✅ **Git** (para clonar el repositorio)

### **Recursos del Sistema:**
- 💾 **RAM**: Mínimo 4GB, Recomendado 8GB+
- 💿 **Disco**: Mínimo 10GB de espacio libre
- 🌐 **Puertos**: 5000, 5001, 27017, 8081

## 🎯 **Opciones de Despliegue**

### **1. 🐳 Despliegue Local con Docker (Recomendado)**

#### **Paso 1: Clonar el Repositorio**
```bash
git clone <tu-repositorio>
cd real-state-api
```

#### **Paso 2: Ejecutar el Script de Despliegue**
```bash
./deploy.sh
```

**¡Eso es todo!** El script automáticamente:
- ✅ Verifica Docker
- ✅ Genera claves JWT
- ✅ Crea archivo .env
- ✅ Construye y despliega todos los servicios

#### **Paso 3: Verificar el Despliegue**
```bash
# Ver estado de los servicios
docker-compose ps

# Ver logs en tiempo real
docker-compose logs -f

# Ver logs de un servicio específico
docker-compose logs -f million-api
```

### **2. 🖥️ Despliegue Manual**

#### **Construir la Imagen**
```bash
docker build -t million-api .
```

#### **Levantar los Servicios**
```bash
docker-compose up -d
```

#### **Verificar el Estado**
```bash
docker-compose ps
```

## 🌐 **Servicios Disponibles**

| Servicio | URL | Descripción |
|----------|-----|-------------|
| **🌐 API** | http://localhost:5000 | API principal |
| **📚 Swagger** | http://localhost:5000/swagger | Documentación de la API |
| **🏥 Health Check** | http://localhost:5000/health/live | Estado del servicio |
| **🗄️ MongoDB** | localhost:27017 | Base de datos |
| **🖥️ Mongo Express** | http://localhost:8081 | UI para MongoDB |

### **Credenciales por Defecto:**
- **MongoDB**: `admin` / `password123`
- **Mongo Express**: `admin` / `password123`

## ⚙️ **Configuración de Variables de Entorno**

### **Archivo .env**
El script crea automáticamente un archivo `.env`. **¡IMPORTANTE!** Configura estas variables:

```bash
# Vercel Blob (OBLIGATORIO para producción)
BLOB_READ_WRITE_TOKEN=tu_token_aqui

# JWT Keys (OBLIGATORIO para producción)
AUTH_JWT_PRIVATE_KEY=tu_clave_privada_aqui
AUTH_JWT_PUBLIC_KEY=tu_clave_publica_aqui

# Dominio (OBLIGATORIO para producción)
AUTH_JWT_ISSUER=https://tudominio.com
AUTH_JWT_AUDIENCE=https://tudominio.com
CORS_ORIGINS=https://tudominio.com
```

### **Generar Claves JWT**
```bash
# Clave privada
openssl genrsa -out keys/private_key.pem 2048

# Clave pública
openssl rsa -in keys/private_key.pem -pubout -out keys/public_key.pem
```

## 🔧 **Comandos Útiles**

### **Gestión de Servicios**
```bash
# Iniciar servicios
docker-compose up -d

# Detener servicios
docker-compose down

# Reiniciar servicios
docker-compose restart

# Ver logs
docker-compose logs -f

# Ver logs de un servicio específico
docker-compose logs -f million-api
```

### **Base de Datos**
```bash
# Conectar a MongoDB
docker exec -it million-mongodb mongosh -u admin -p password123

# Backup de la base de datos
docker exec million-mongodb mongodump --out /backup

# Restaurar base de datos
docker exec million-mongodb mongorestore /backup
```

### **Mantenimiento**
```bash
# Limpiar recursos Docker
docker system prune -f

# Ver uso de recursos
docker stats

# Actualizar imágenes
docker-compose pull
docker-compose up -d
```

## 🚨 **Solución de Problemas**

### **Problemas Comunes:**

#### **1. Puerto ya en uso**
```bash
# Ver qué está usando el puerto
lsof -i :5000

# Cambiar puerto en docker-compose.yml
ports:
  - "5001:80"  # Cambiar 5000 por 5001
```

#### **2. Error de permisos Docker**
```bash
# Agregar usuario al grupo docker
sudo usermod -aG docker $USER
# Reiniciar sesión
```

#### **3. MongoDB no se conecta**
```bash
# Verificar logs
docker-compose logs mongodb

# Reiniciar MongoDB
docker-compose restart mongodb
```

#### **4. API no responde**
```bash
# Verificar logs de la API
docker-compose logs million-api

# Verificar health check
curl http://localhost:5000/health/live
```

### **Logs de Debug**
```bash
# Ver logs detallados
docker-compose logs -f --tail=100

# Ver logs de un servicio específico
docker-compose logs -f million-api --tail=50
```

## 🚀 **Despliegue en Producción**

### **Recomendaciones de Seguridad:**
1. **🔐 Cambiar todas las contraseñas por defecto**
2. **🌐 Usar HTTPS con certificados SSL válidos**
3. **🔑 Generar claves JWT únicas y seguras**
4. **📊 Configurar monitoreo y alertas**
5. **💾 Configurar backups automáticos de MongoDB**

### **Variables de Entorno de Producción:**
```bash
ASPNETCORE_ENVIRONMENT=Production
LOG_LEVEL=Warning
RATE_LIMIT_PERMINUTE=60  # Más restrictivo en producción
AUTH_LOCKOUT_ATTEMPTS=3   # Más restrictivo en producción
```

### **Monitoreo:**
```bash
# Health checks
curl http://localhost:5000/health/live
curl http://localhost:5000/health/ready

# Métricas de la API
curl http://localhost:5000/health
```

## 📚 **Recursos Adicionales**

- **📖 [Documentación de la API](README.md)**
- **🐳 [Docker Documentation](https://docs.docker.com/)**
- **🍃 [MongoDB Documentation](https://docs.mongodb.com/)**
- **🌐 [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)**

## 🆘 **Soporte**

Si tienes problemas con el despliegue:

1. **📋 Verifica los prerrequisitos**
2. **🔍 Revisa los logs de Docker**
3. **📖 Consulta la documentación**
4. **🐛 Reporta issues en el repositorio**

---

**¡Feliz Despliegue! 🚀**

