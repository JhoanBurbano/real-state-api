# 🌱 Database Seeding Guide

This guide explains how to seed the Million Real Estate API database with sample data.

## 📋 Prerequisites

- MongoDB instance running (local or Atlas)
- Node.js installed
- Required npm packages installed

## 🔐 Environment Setup

**IMPORTANT: Never hardcode database credentials in your code!**

Create a `.env` file in your project root:

```bash
# .env
MONGODB_URI=mongodb+srv://username:password@cluster.mongodb.net/?retryWrites=true&w=majority
MONG_DATABASE=million
```

## 🚀 Running the Seeding Scripts

### 1. Basic Seeding

```bash
# Install dependencies
npm install

# Run basic seeding
npm run seed:basic
```

### 2. Advanced Seeding with 50 Properties

```bash
# Run advanced seeding
npm run seed:advanced
```

### 3. Custom Seeding

```bash
# Run custom seeding script
node scripts/seed-mongodb.js
```

## 📊 Sample Data Structure

### Owners
- Basic owner information
- Contact details
- Role assignments

### Properties
- Property details
- Media attachments
- Pricing information
- Status tracking

### Property Traces
- Change history
- Timeline tracking
- User activity logs

## 🔧 Scripts Available

### `scripts/seed-mongodb.js`
Basic seeding script that creates:
- 2 sample owners
- 1 sample property
- Basic property traces

### `scripts/seed-mongodb-fixed.js`
Enhanced seeding script with:
- Better error handling
- Environment variable support
- Comprehensive data validation

### `ops/db/seed-database.js`
Database seeding script for Docker environments

## 🐳 Docker Seeding

```bash
# Using Docker Compose
docker-compose exec mongodb mongosh "mongodb://localhost:27017/million" < ops/db/seed-database.js

# Direct Docker execution
docker exec -i million-mongodb mongosh "mongodb://localhost:27017/million" < ops/db/seed-database.js
```

## 🔒 Security Best Practices

1. **Never hardcode credentials** in source code
2. **Use environment variables** for sensitive data
3. **Rotate passwords regularly** in production
4. **Use least privilege access** for database users
5. **Monitor access logs** for suspicious activity

## 🚨 Troubleshooting

### Connection Issues
- Verify MongoDB URI format
- Check network connectivity
- Ensure proper authentication

### Permission Issues
- Verify user roles and permissions
- Check database access rights
- Ensure proper authentication tokens

### Data Issues
- Verify data format compliance
- Check required field validation
- Ensure proper data types

## 📝 Customization

To customize the seeding data:

1. Modify the data arrays in the scripts
2. Add new entity types as needed
3. Update validation rules
4. Test with small datasets first

## 🔄 Resetting Data

To reset the database:

```bash
# Clear all collections
node scripts/clean-mongodb.js

# Or manually in MongoDB
use million
db.dropDatabase()
```

## 📈 Performance Tips

- Use bulk operations for large datasets
- Index frequently queried fields
- Monitor memory usage during seeding
- Use appropriate batch sizes

## 🎯 Next Steps

After seeding:
1. Verify data integrity
2. Test API endpoints
3. Validate business logic
4. Monitor performance metrics
