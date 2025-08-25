# 🌱 **Database Seeding - Million Real Estate API**

## 📋 **Overview**

This document explains how to seed the MongoDB Atlas database with test data for the Million Real Estate API.

## 🚀 **Quick Start**

### **1. Install Dependencies**
```bash
npm install
```

### **2. Run Seed Script**
```bash
npm run seed
```

Or directly:
```bash
node scripts/seed-mongodb.js
```

## 📊 **What Gets Seeded**

### **Owners (3)**
- **Carlos Rodriguez** - Owner of Oceanfront Villa and Garden Estate
- **Maria Gonzalez** - Owner of Downtown Penthouse  
- **System Administrator** - Admin user for system management

### **Properties (3)**
- **Oceanfront Villa** - $12.5M luxury villa in Miami Beach
- **Downtown Penthouse** - $8.5M penthouse in Miami's financial district
- **Garden Estate** - $9.8M family estate in Coral Gables

## 🔧 **Configuration**

### **Environment Variables**
The seed script uses these hardcoded values (can be modified in the script):

```javascript
const MONGODB_URI = "mongodb+srv://jsburbano:EmpanadasConAji123@pruebastecnicas.sm4lf1d.mongodb.net/?retryWrites=true&w=majority&appName=pruebastecnicas";
const DATABASE_NAME = "million";
```

### **Property Status Values**
The script uses the correct enum values that match the C# API:

```javascript
const PropertyStatus = {
  Active: 0,      // C# PropertyStatus.Active
  Sold: 1,        // C# PropertyStatus.Sold  
  OffMarket: 2    // C# PropertyStatus.OffMarket
};
```

## 📝 **Data Structure**

### **Property Document**
```json
{
  "_id": "prop-001",
  "ownerId": "owner-001",
  "name": "Oceanfront Villa",
  "address": "100 Ocean Dr, Miami Beach, FL",
  "city": "Miami Beach",
  "neighborhood": "South Beach",
  "propertyType": "Villa",
  "description": "Luxurious oceanfront villa...",
  "price": 12500000,
  "codeInternal": "MB001",
  "year": 2020,
  "size": 8500,
  "bedrooms": 5,
  "bathrooms": 6,
  "hasPool": true,
  "hasGarden": true,
  "hasParking": true,
  "isFurnished": true,
  "availableFrom": "2024-01-15T00:00:00Z",
  "availableTo": "2024-12-31T00:00:00Z",
  "status": 0,
  "cover": {
    "type": "Image",
    "url": "https://blob.vercel-storage.com/properties/mb001/cover.jpg",
    "index": 0
  },
  "media": [
    {
      "id": "media-001",
      "type": "Image",
      "url": "https://blob.vercel-storage.com/properties/mb001/1.jpg",
      "index": 1,
      "enabled": true,
      "featured": true
    }
  ],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z",
  "isActive": true
}
```

### **Owner Document**
```json
{
  "_id": "owner-001",
  "fullName": "Carlos Rodriguez",
  "email": "carlos.rodriguez@million.com",
  "phoneE164": "+13055551234",
  "photoUrl": "https://blob.vercel-storage.com/owners/carlos-rodriguez.jpg",
  "role": "Owner",
  "isActive": true,
  "passwordHash": "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

## 🔍 **Indexes Created**

The script creates these indexes for optimal performance:

- **Properties Collection**:
  - `ownerId` - For filtering by owner
  - `city` - For location-based queries
  - `propertyType` - For property type filtering
  - `price` - For price range queries
  - `status` - For status filtering

- **Owners Collection**:
  - `email` - Unique index for authentication

## 🧪 **Testing the Seed**

### **Verify Data in MongoDB Atlas**
```bash
# Connect to MongoDB Atlas
mongosh "mongodb+srv://jsburbano:EmpanadasConAji123@pruebastecnicas.sm4lf1d.mongodb.net/?retryWrites=true&w=majority&appName=pruebastecnicas"

# Switch to database
use million

# Check collections
show collections

# Count documents
db.owners.countDocuments()
db.properties.countDocuments()

# View sample data
db.owners.findOne()
db.properties.findOne()
```

### **Test API Endpoints**
```bash
# Health check
curl "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/health/live"

# Get properties (should return 3 properties)
curl "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/properties?page=1&pageSize=10"

# Get specific property
curl "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/properties/prop-001"
```

## 🔄 **Reseeding the Database**

### **Clear and Reseed**
The script automatically clears existing data before seeding:

```javascript
// Clear existing data
await db.collection('properties').deleteMany({});
await db.collection('owners').deleteMany({});
await db.collection('owner_sessions').deleteMany({});
```

### **Run Again**
```bash
npm run seed
```

## 🚨 **Troubleshooting**

### **Common Issues**

#### **Connection Failed**
```bash
# Check MongoDB URI
# Verify network access
# Check credentials
```

#### **Index Creation Failed**
```bash
# Script handles existing indexes gracefully
# Check MongoDB Atlas permissions
```

#### **Data Not Showing in API**
```bash
# Verify PropertyStatus values (should be 0, not "Active")
# Check isActive field (should be true)
# Verify MongoDB connection in API
```

### **Debug Commands**
```bash
# Test MongoDB connection
node -e "
const { MongoClient } = require('mongodb');
const client = new MongoClient('YOUR_MONGO_URI');
client.connect().then(() => console.log('Connected!')).catch(console.error);
"
```

## 📚 **Related Documentation**

- **[API Integration Guide](API_INTEGRATION_GUIDE.md)** - Complete API documentation
- **[Deployment Scripts](DEPLOYMENT_SCRIPTS.md)** - How to deploy the API
- **[Architecture](ARCHITECTURE.md)** - System design and database schema

## 🔧 **Customization**

### **Add More Properties**
Edit the `properties` array in `scripts/seed-mongodb.js`:

```javascript
const properties = [
  // ... existing properties ...
  {
    _id: "prop-004",
    ownerId: "owner-002",
    name: "New Property",
    // ... add all required fields
  }
];
```

### **Add More Owners**
Edit the `owners` array:

```javascript
const owners = [
  // ... existing owners ...
  {
    _id: "owner-004",
    fullName: "New Owner",
    // ... add all required fields
  }
];
```

### **Modify Data**
- **Prices**: Update the `price` field
- **Locations**: Update `city` and `neighborhood`
- **Features**: Modify boolean fields like `hasPool`, `hasGarden`
- **Media**: Update `cover.url` and `media[]` arrays

---

**Last Updated**: January 2024  
**Script Version**: v1.0.0  
**Database**: MongoDB Atlas
