# 🏠 **Million Real Estate API**

A modern, scalable real estate API built with .NET 9, MongoDB, and deployed on Google Cloud Run.

## 🚀 **Quick Start**

### **API Base URL**
```
https://million-real-estate-api-sh25jnp3aa-uc.a.run.app
```

### **Health Check**
```bash
curl "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/health/live"
```

### **Get Properties**
```bash
curl "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/properties?page=1&pageSize=5"
```

## 📚 **Documentation**

- **[API Integration Guide](docs/API_INTEGRATION_GUIDE.md)** - Complete guide for client integration
- **[API Routes](docs/API_ROUTES.md)** - All available endpoints and parameters
- **[Architecture](docs/ARCHITECTURE.md)** - System design and architecture
- **[Error Handling](docs/ERROR_HANDLING.md)** - Error codes and handling
- **[Database Seeding](docs/SEED_DATABASE.md)** - How to seed the database with test data
- **[Changelog](docs/CHANGELOG.md)** - Complete history of changes and new features

## 🏗️ **Architecture**

- **Framework**: .NET 9.0
- **Database**: MongoDB Atlas
- **Authentication**: JWT with refresh tokens
- **Deployment**: Google Cloud Run
- **Container**: Docker
- **CI/CD**: GitHub Actions

## 🚀 **Deployment**

### **Prerequisites**
- Docker
- Google Cloud CLI (`gcloud`)
- Docker Hub account

### **Quick Deploy to Google Cloud Run**

1. **Build and push to Docker Hub:**
```bash
# Login to Docker Hub
docker login

# Build and push
docker build -t jsburbano07/million-real-estate-api:latest .
docker push jsburbano07/million-real-estate-api:latest
```

2. **Deploy to Google Cloud Run:**
```bash
# Set your project ID
export GCP_PROJECT_ID="your-project-id"

# Run deployment script
./scripts/gcp-deploy.sh
```

### **Full CI/CD Pipeline**

Use the GitHub Actions workflow for automated deployment:

```bash
# Push to main branch triggers automatic deployment
git push origin main
```

## 🔧 **Development**

### **Local Development**
```bash
# Run with Docker Compose
docker-compose up -d

# Run tests
dotnet test

# Run API locally
cd src/Million.Web
dotnet run
```

### **Environment Variables**
```bash
MONGO_URI=mongodb://localhost:27017
MONGO_DB=million
JWT_SECRET=your-secret-key
JWT_ISSUER=http://localhost:5000
JWT_AUDIENCE=http://localhost:3000
```

## 📊 **Features**

### **🏠 Property Management**
- ✅ **CRUD Operations** - Full Create, Read, Update, Delete for properties
- ✅ **Rich Descriptions** - Detailed property descriptions (up to 1000 characters)
- ✅ **Advanced Search** - Full-text search across name, description, and address
- ✅ **Comprehensive Filtering** - Price, location, amenities, and more
- ✅ **Media Support** - Image and video galleries with featured media

### **🔐 Authentication & Security**
- ✅ **JWT Authentication** - Secure token-based authentication
- ✅ **Refresh Tokens** - Automatic token renewal
- ✅ **Owner Profiles** - Authenticated user profile management (`/owners/profile`)
- ✅ **Rate Limiting** - IP-based protection (100 req/min)
- ✅ **Input Validation** - Comprehensive FluentValidation rules

### **📱 API Features**
- ✅ **RESTful Design** - Standard HTTP methods and status codes
- ✅ **Pagination** - Efficient data pagination with metadata
- ✅ **Sorting** - Multiple sort options for all endpoints
- ✅ **Error Handling** - RFC 7807 Problem Details format
- ✅ **Health Monitoring** - Live and ready health check endpoints

### **🌐 Infrastructure**
- ✅ **MongoDB Integration** - Document-based data storage
- ✅ **Docker Support** - Containerized deployment
- ✅ **Google Cloud Run** - Scalable cloud deployment
- ✅ **CORS Support** - Configurable cross-origin requests
- ✅ **Structured Logging** - Serilog with correlation IDs

## 🧪 **Testing**

### **Run Tests**
```bash
# Unit tests
dotnet test tests/Million.Tests/

# E2E tests
dotnet test tests/Million.E2E.Tests/
```

### **Test Data**
The API includes test properties and owners for development and testing.

#### **Seed the Database**
```bash
# Install Node.js dependencies
npm install

# Run the seed script
npm run seed
```

This will populate the database with 3 sample properties and 3 owners for testing.

## 📈 **Monitoring**

- **Health Checks**: `/health/live` and `/health/ready`
- **Logs**: Google Cloud Logging
- **Metrics**: Prometheus-compatible endpoints
- **Tracing**: Distributed tracing support

## 🔒 **Security**

- JWT authentication with refresh tokens
- Rate limiting (100 requests/minute per IP)
- Input validation and sanitization
- HTTPS enforcement
- CORS configuration

## 📝 **API Examples**

### **Authentication**
```bash
# Login
curl -X POST "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "carlos.rodriguez@million.com", "password": "password"}'
```

### **Create Property**
```bash
curl -X POST "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/properties" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "New Property",
    "description": "Beautiful property with modern amenities and excellent location. Features premium finishes and quality construction.",
    "address": "123 Main St",
    "city": "Miami",
    "price": 500000,
    "propertyType": "Apartment",
    "bedrooms": 2,
    "bathrooms": 2
  }'
```

### **Get Owner Profile**
```bash
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/owners/profile"
```

## 🤝 **Contributing**

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📄 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 **Support**

- **Documentation**: [docs/](docs/) directory
- **Issues**: [GitHub Issues](https://github.com/your-repo/issues)
- **Email**: support@million.com

---

**Built with ❤️ using .NET 9 and MongoDB**

