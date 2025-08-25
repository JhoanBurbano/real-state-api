# 🔒 **SECURITY GUIDE - Million Real Estate API**

## 🚨 **CRITICAL SECURITY ALERT**

**GitGuardian ha detectado credenciales reales de MongoDB expuestas en este repositorio.**

### **❌ Credenciales Expuestas:**
- **Usuario**: `jsburbano`
- **Contraseña**: `EmpanadasConAji123`
- **Cluster**: `pruebastecnicas.sm4lf1d.mongodb.net`

### **🚨 ACCIONES INMEDIATAS REQUERIDAS:**

#### **1. Cambiar Contraseña de MongoDB Atlas:**
- Ve a [MongoDB Atlas](https://cloud.mongodb.com)
- Accede a tu cluster `pruebastecnicas`
- Ve a Database Access → Usuario `jsburbano`
- **CAMBIA LA CONTRASEÑA INMEDIATAMENTE**
- Revoca cualquier token de acceso

#### **2. Verificar Acceso:**
- Revisa los logs de acceso de MongoDB Atlas
- Verifica si hay conexiones no autorizadas
- Monitorea el uso de la base de datos

#### **3. Rotar Credenciales:**
- Crea un nuevo usuario con permisos mínimos
- Usa contraseñas fuertes y únicas
- Implementa autenticación de dos factores

## 🔐 **BEST PRACTICES DE SEGURIDAD**

### **Variables de Entorno:**
```bash
# ✅ CORRECTO - Usar variables de entorno
MONGO__URI=mongodb+srv://username:password@cluster.mongodb.net

# ❌ INCORRECTO - Hardcodear en el código
const uri = "mongodb+srv://jsburbano:password123@cluster.mongodb.net";
```

### **Archivos de Configuración:**
```bash
# ✅ CORRECTO - Archivo de ejemplo
env.example

# ❌ INCORRECTO - Archivo con credenciales reales
.env
dockerhub-gcp.env
```

### **Git Ignore:**
```bash
# ✅ CORRECTO - Ignorar archivos sensibles
.env
*.env.local
*.env.production

# ❌ INCORRECTO - Committear credenciales
git add .env
git commit -m "Add database credentials"
```

## 🛡️ **MEDIDAS DE SEGURIDAD IMPLEMENTADAS**

### **1. Archivos Corregidos:**
- ✅ `cloud-run-service.yaml` - Credenciales reemplazadas
- ✅ `dockerhub-gcp.env` - Placeholders seguros
- ✅ `scripts/seed-mongodb.js` - Variables de entorno
- ✅ `scripts/seed-mongodb-fixed.js` - Variables de entorno
- ✅ `tests/Million.E2E.Tests/GlobalSetup.cs` - Variables de entorno
- ✅ `docs/SEED_DATABASE.md` - Sin credenciales reales

### **2. Archivos de Seguridad:**
- ✅ `.gitignore` - Reglas de seguridad agregadas
- ✅ `env.example` - Plantilla segura
- ✅ `SECURITY.md` - Guía de seguridad

### **3. Patrones Seguros:**
- ✅ Uso de variables de entorno
- ✅ Placeholders en lugar de credenciales reales
- ✅ Documentación sin información sensible
- ✅ Scripts con configuración segura

## 🔍 **VERIFICACIÓN DE SEGURIDAD**

### **Comandos de Verificación:**
```bash
# Buscar credenciales hardcodeadas
grep -r "mongodb+srv://" src/
grep -r "jsburbano:" src/
grep -r "EmpanadasConAji123" src/

# Verificar archivos ignorados
git status --ignored

# Verificar commits recientes
git log --oneline -10
```

### **Archivos a Verificar:**
- [ ] `src/Million.Web/appsettings.json`
- [ ] `src/Million.Infrastructure/Config/MongoOptions.cs`
- [ ] `docker-compose.yml`
- [ ] `docker-compose.*.yml`
- [ ] `*.env` files
- [ ] `*.env.*` files

## 🚀 **DESPLIEGUE SEGURO**

### **1. Variables de Entorno:**
```bash
# Desarrollo
MONGO__URI=mongodb://localhost:27017
JWT__SECRET=dev-secret-key

# Staging
MONGO__URI=mongodb+srv://staging_user:staging_pass@staging-cluster.mongodb.net
JWT__SECRET=staging-secret-key

# Producción
MONGO__URI=mongodb+srv://prod_user:prod_pass@prod-cluster.mongodb.net
JWT__SECRET=production-super-secure-key
```

### **2. Configuración de GitHub Actions:**
```yaml
# ✅ CORRECTO - Usar secrets
env:
  MONGO__URI: ${{ secrets.MONGO_URI }}
  JWT__SECRET: ${{ secrets.JWT_SECRET }}

# ❌ INCORRECTO - Hardcodear en workflow
env:
  MONGO__URI: "mongodb+srv://user:pass@cluster.mongodb.net"
```

### **3. Configuración de Docker:**
```dockerfile
# ✅ CORRECTO - Variables de entorno
ENV MONGO__URI=mongodb://localhost:27017

# ❌ INCORRECTO - Credenciales hardcodeadas
ENV MONGO__URI=mongodb+srv://user:pass@cluster.mongodb.net
```

## 📋 **CHECKLIST DE SEGURIDAD**

### **Antes de cada Commit:**
- [ ] No hay credenciales hardcodeadas
- [ ] Los archivos `.env` están en `.gitignore`
- [ ] Las variables de entorno usan placeholders
- [ ] La documentación no expone información sensible
- [ ] Los scripts usan variables de entorno

### **Antes de cada Despliegue:**
- [ ] Las credenciales están en variables de entorno
- [ ] Los secrets están configurados en la plataforma
- [ ] Las contraseñas son fuertes y únicas
- [ ] El acceso está limitado por IP si es posible
- [ ] Los logs de acceso están habilitados

### **Mensualmente:**
- [ ] Rotar contraseñas y tokens
- [ ] Revisar logs de acceso
- [ ] Verificar permisos de usuario
- [ ] Actualizar dependencias
- [ ] Revisar alertas de seguridad

## 🆘 **EN CASO DE COMPROMISO**

### **1. Inmediatamente:**
- Cambia todas las contraseñas
- Revoca todos los tokens
- Bloquea el acceso sospechoso
- Notifica al equipo de seguridad

### **2. Investigación:**
- Revisa logs de acceso
- Identifica el alcance del compromiso
- Documenta el incidente
- Implementa medidas correctivas

### **3. Recuperación:**
- Restaura desde backup limpio
- Implementa monitoreo adicional
- Revisa y fortalece la seguridad
- Entrena al equipo en mejores prácticas

## 📞 **CONTACTO DE SEGURIDAD**

### **Reportar Problemas:**
- **GitHub Issues**: Etiqueta `security`
- **Email**: [security@million.com](mailto:security@security.com)
- **Slack**: Canal `#security`

### **Emergencias:**
- **24/7**: [emergency@million.com](mailto:emergency@million.com)
- **Teléfono**: [+1-XXX-XXX-XXXX](tel:+1-XXX-XXX-XXXX)

---

**⚠️ RECUERDA: La seguridad es responsabilidad de todos. Si ves algo, di algo.**
