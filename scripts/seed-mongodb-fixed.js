#!/usr/bin/env node

/**
 * Million Real Estate API - MongoDB Atlas Seed Script (Fixed)
 * 
 * This script connects to MongoDB Atlas and seeds the database with test data.
 * Fixed to use proper C# entity field names.
 * 
 * Usage:
 * 1. Install dependencies: npm install mongodb
 * 2. Run: node scripts/seed-mongodb-fixed.js
 */

const { MongoClient } = require('mongodb');
require('dotenv').config();

// Use environment variable for MongoDB URI - NEVER hardcode credentials
const MONGODB_URI = process.env.MONGODB_URI || "mongodb://localhost:27017";

// PropertyStatus enum values (matching C# enum)
const PropertyStatus = {
  Active: 0,
  Sold: 1,
  OffMarket: 2
};

// MediaType enum values (matching C# enum)
const MediaType = {
  Image: 0,
  Video: 1
};

// Test data
const owners = [
  {
    _id: "owner-001",
    fullName: "Carlos Rodriguez",
    email: "carlos.rodriguez@million.com",
    phoneE164: "+13055551234",
    photoUrl: "https://blob.vercel-storage.com/owners/carlos-rodriguez.jpg",
    role: "Owner",
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z")
  },
  {
    _id: "owner-002",
    fullName: "Maria Gonzalez",
    email: "maria.gonzalez@million.com",
    phoneE164: "+13055555678",
    photoUrl: "https://blob.vercel-storage.com/owners/maria-gonzalez.jpg",
    role: "Owner",
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z")
  },
  {
    _id: "admin-001",
    fullName: "System Administrator",
    email: "admin@million.com",
    phoneE164: "+13055550000",
    photoUrl: "https://blob.vercel-storage.com/owners/admin.jpg",
    role: "Admin",
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z")
  }
];

const properties = [
  {
    _id: "prop-001",
    ownerId: "owner-001",
    name: "Oceanfront Villa",
    address: "100 Ocean Dr, Miami Beach, FL",
    city: "Miami Beach",
    neighborhood: "South Beach",
    propertyType: "Villa",
    description: "Luxurious oceanfront villa with stunning views of the Atlantic Ocean. This 5-bedroom, 6-bathroom estate features premium finishes, a private pool, and direct beach access.",
    price: 12500000,
    codeInternal: "MB001",
    year: 2020,
    size: 8500,
    bedrooms: 5,
    bathrooms: 6,
    hasPool: true,
    hasGarden: true,
    hasParking: true,
    isFurnished: true,
    availableFrom: new Date("2024-01-15T00:00:00Z"),
    availableTo: new Date("2024-12-31T00:00:00Z"),
    status: PropertyStatus.Active, // Use enum value (0) instead of string
    cover: {
      Type: MediaType.Image, // Use proper C# field name and enum value
      Url: "https://blob.vercel-storage.com/properties/mb001/cover.jpg",
      Index: 0
    },
    media: [
      {
        Id: "media-001", // Use proper C# field name
        Type: MediaType.Image, // Use proper C# field name and enum value
        Url: "https://blob.vercel-storage.com/properties/mb001/1.jpg",
        Index: 1,
        Enabled: true, // Use proper C# field name
        Featured: true // Use proper C# field name
      },
      {
        Id: "media-002",
        Type: MediaType.Image,
        Url: "https://blob.vercel-storage.com/properties/mb001/2.jpg",
        Index: 2,
        Enabled: true,
        Featured: true
      }
    ],
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z"),
    isActive: true
  },
  {
    _id: "prop-002",
    ownerId: "owner-002",
    name: "Downtown Penthouse",
    address: "500 Brickell Ave, Miami, FL",
    city: "Miami",
    neighborhood: "Brickell",
    propertyType: "Penthouse",
    description: "Modern luxury penthouse in the heart of Miami's financial district. Features floor-to-ceiling windows, designer kitchen, and panoramic city views.",
    price: 8500000,
    codeInternal: "MI002",
    year: 2022,
    size: 4200,
    bedrooms: 3,
    bathrooms: 4,
    hasPool: false,
    hasGarden: false,
    hasParking: true,
    isFurnished: false,
    availableFrom: new Date("2024-02-01T00:00:00Z"),
    availableTo: new Date("2024-12-31T00:00:00Z"),
    status: PropertyStatus.Active,
    cover: {
      Type: MediaType.Image,
      Url: "https://blob.vercel-storage.com/properties/mi002/cover.jpg",
      Index: 0
    },
    media: [
      {
        Id: "media-003",
        Type: MediaType.Image,
        Url: "https://blob.vercel-storage.com/properties/mi002/1.jpg",
        Index: 1,
        Enabled: true,
        Featured: true
      }
    ],
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z"),
    isActive: true
  },
  {
    _id: "prop-003",
    ownerId: "owner-001",
    name: "Garden Estate",
    address: "1500 Sunset Blvd, Coral Gables, FL",
    city: "Coral Gables",
    neighborhood: "Sunset",
    propertyType: "Estate",
    description: "Spacious family estate with lush gardens, tennis court, and guest house. Perfect for large families seeking privacy and luxury.",
    price: 9800000,
    codeInternal: "CG003",
    year: 2018,
    size: 7200,
    bedrooms: 6,
    bathrooms: 7,
    hasPool: true,
    hasGarden: true,
    hasParking: true,
    isFurnished: true,
    availableFrom: new Date("2024-03-01T00:00:00Z"),
    availableTo: new Date("2024-12-31T00:00:00Z"),
    status: PropertyStatus.Active,
    cover: {
      Type: MediaType.Image,
      Url: "https://blob.vercel-storage.com/properties/cg003/cover.jpg",
      Index: 0
    },
    media: [
      {
        Id: "media-004",
        Type: MediaType.Image,
        Url: "https://blob.vercel-storage.com/properties/cg003/1.jpg",
        Index: 1,
        Enabled: true,
        Featured: true
      },
      {
        Id: "media-005",
        Type: MediaType.Image,
        Url: "https://blob.vercel-storage.com/properties/cg003/2.jpg",
        Index: 2,
        Enabled: true,
        Featured: true
      }
    ],
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z"),
    isActive: true
  }
];

async function seedDatabase() {
  const client = new MongoClient(MONGODB_URI);
  
  try {
    console.log('🔌 Connecting to MongoDB Atlas...');
    await client.connect();
    console.log('✅ Connected successfully');
    
    const db = client.db('million');
    
    // Clear existing data
    console.log('🧹 Clearing existing data...');
    await db.collection('properties').deleteMany({});
    await db.collection('owners').deleteMany({});
    await db.collection('owner_sessions').deleteMany({});
    console.log('✅ Existing data cleared');
    
    // Insert owners
    console.log('👥 Seeding owners...');
    const ownersResult = await db.collection('owners').insertMany(owners);
    console.log(`✅ Inserted ${ownersResult.insertedCount} owners`);
    
    // Insert properties
    console.log('🏠 Seeding properties...');
    const propertiesResult = await db.collection('properties').insertMany(properties);
    console.log(`✅ Inserted ${propertiesResult.insertedCount} properties`);
    
    // Create indexes (skip if they already exist)
    console.log('🔍 Creating indexes...');
    try {
      await db.collection('properties').createIndex({ "ownerId": 1 });
      await db.collection('properties').createIndex({ "city": 1 });
      await db.collection('properties').createIndex({ "propertyType": 1 });
      await db.collection('properties').createIndex({ "price": 1 });
      await db.collection('properties').createIndex({ "status": 1 });
      await db.collection('owners').createIndex({ "email": 1 }, { unique: true });
      console.log('✅ Indexes created');
    } catch (indexError) {
      if (indexError.code === 85) { // IndexOptionsConflict
        console.log('⚠️  Some indexes already exist, skipping...');
      } else {
        throw indexError;
      }
    }
    
    console.log('🎉 Database seeded successfully!');
    console.log(`📊 Total owners: ${owners.length}`);
    console.log(`📊 Total properties: ${properties.length}`);
    console.log(`🔧 PropertyStatus values: Active=${PropertyStatus.Active}, Sold=${PropertyStatus.Sold}, OffMarket=${PropertyStatus.OffMarket}`);
    console.log(`🔧 MediaType values: Image=${MediaType.Image}, Video=${MediaType.Video}`);
    
    // Verify data was inserted correctly
    console.log('\n🔍 Verifying data...');
    const ownerCount = await db.collection('owners').countDocuments({});
    const propertyCount = await db.collection('properties').countDocuments({});
    const activePropertyCount = await db.collection('properties').countDocuments({ isActive: true });
    
    console.log(`👥 Owners in database: ${ownerCount}`);
    console.log(`🏠 Properties in database: ${propertyCount}`);
    console.log(`✅ Active properties: ${activePropertyCount}`);
    
  } catch (error) {
    console.error('❌ Error seeding database:', error);
    process.exit(1);
  } finally {
    await client.close();
    console.log('🔌 Connection closed');
  }
}

// Run the seed function
seedDatabase().catch(console.error);
