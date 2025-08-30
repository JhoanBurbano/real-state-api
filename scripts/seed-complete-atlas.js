#!/usr/bin/env node

/**
 * Million Real Estate API - Complete MongoDB Atlas Seed Script
 * 
 * This script seeds the database with:
 * - 10 owners (including admin)
 * - 50 properties with realistic data
 * - Owner sessions for authentication
 * - Property traces for transaction history
 * - All collections properly linked and coherent
 * 
 * Usage:
 * 1. Install dependencies: npm install mongodb
 * 2. Run: node scripts/seed-complete-atlas.js
 */

const { MongoClient } = require('mongodb');
const path = require('path');

// Load .env file from project root
require('dotenv').config({ path: path.join(__dirname, '..', '.env') });

// Use environment variable for MongoDB URI
const MONGODB_URI = process.env.MONGO_URI || "mongodb://localhost:27017";

// C# Enum values (matching the application)
const PropertyStatus = {
  Available: 0,
  Rented: 1,
  Sold: 2,
  UnderMaintenance: 3
};

const MediaType = {
  Image: 0,
  Video: 1
};

const TraceAction = {
  Created: 0,
  Updated: 1,
  Sold: 2,
  Rented: 3,
  PriceChanged: 4,
  StatusChanged: 5,
  MediaUpdated: 6,
  CoverChanged: 7,
  GalleryUpdated: 8
};

// Generate team owners with the specific data provided
const owners = [
  {
    _id: "admin-001",
    fullName: "System Administrator",
    email: "admin@million.com",
    phoneE164: "+13055550000",
    photoUrl: "https://blob.vercel-storage.com/owners/admin.jpg",
    role: 1, // Admin
    description: "System administrator for Million Real Estate platform",
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test1234",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z")
  },
  {
    _id: "sarah-johnson",
    fullName: "Sarah Johnson",
    email: "sarah.johnson@millionrealestate.com",
    phoneE164: "+15551234567",
    photoUrl: "https://blob.vercel-storage.com/owners/owner-001/photo.jpg",
    role: 2, // CEO
    description: "Visionary leader with 25+ years in luxury real estate",
    title: "CEO & Senior Agent",
    bio: "Visionary leader with 25+ years in luxury real estate. Specializes in high-end residential properties and investment portfolios.",
    experienceYears: 25,
    propertiesSold: 150,
    rating: 5.0,
    specialties: ["Luxury Residential", "Investment Properties", "International Markets"],
    languages: ["English", "Spanish", "French"],
    certifications: ["Certified Luxury Home Specialist", "International Property Specialist"],
    location: "Beverly Hills, CA",
    address: "123 Luxury Lane, Beverly Hills, CA 90210",
    timezone: "America/Los_Angeles",
    company: "MILLION Luxury Real Estate",
    department: "Luxury Sales",
    employeeId: "EMP-001",
    isAvailable: true,
    schedule: "Mon-Fri 9AM-6PM",
    responseTime: "2 hours",
    totalSalesValue: 450000000,
    averagePrice: 3000000,
    clientSatisfaction: 98,
    linkedInUrl: "https://linkedin.com/in/sarahjohnson",
    instagramUrl: "https://instagram.com/sarahjohnson_realestate",
    facebookUrl: "https://facebook.com/sarahjohnson.agent",
    lastActive: new Date("2024-01-15T10:30:00Z"),
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test1234",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-15T10:30:00Z")
  },
  {
    _id: "michael-chen",
    fullName: "Michael Chen",
    email: "michael.chen@millionrealestate.com",
    phoneE164: "+15551234568",
    photoUrl: "https://blob.vercel-storage.com/owners/owner-002/photo.jpg",
    role: 3, // HeadOfSales
    description: "Expert negotiator specializing in high-end properties",
    title: "Head of Sales",
    bio: "Expert negotiator specializing in high-end properties with 18+ years of experience in luxury real estate sales.",
    experienceYears: 18,
    propertiesSold: 120,
    rating: 4.9,
    specialties: ["Luxury Sales", "Negotiation", "Client Relations"],
    languages: ["English", "Mandarin"],
    certifications: ["Certified Negotiation Expert", "Luxury Property Specialist"],
    location: "Miami Beach, FL",
    address: "456 Ocean Drive, Miami Beach, FL 33139",
    timezone: "America/New_York",
    company: "MILLION Luxury Real Estate",
    department: "Sales Management",
    employeeId: "EMP-002",
    isAvailable: true,
    schedule: "Mon-Sat 9AM-7PM",
    responseTime: "1 hour",
    totalSalesValue: 380000000,
    averagePrice: 3200000,
    clientSatisfaction: 97,
    linkedInUrl: "https://linkedin.com/in/michaelchen",
    instagramUrl: "https://instagram.com/michaelchen_sales",
    facebookUrl: "https://facebook.com/michaelchen.agent",
    lastActive: new Date("2024-01-15T11:00:00Z"),
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test1234",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-15T11:00:00Z")
  },
  {
    _id: "emma-rodriguez",
    fullName: "Emma Rodriguez",
    email: "emma.rodriguez@millionrealestate.com",
    phoneE164: "+15551234569",
    photoUrl: "https://blob.vercel-storage.com/owners/owner-003/photo.jpg",
    role: 4, // LeadDesigner
    description: "Creative visionary transforming properties into dream homes",
    title: "Lead Designer",
    bio: "Creative visionary transforming properties into dream homes with 15+ years in interior design and staging.",
    experienceYears: 15,
    propertiesSold: 85,
    rating: 4.8,
    specialties: ["Interior Design", "Property Staging", "Luxury Renovations"],
    languages: ["English", "Spanish"],
    certifications: ["Certified Interior Designer", "Property Staging Specialist"],
    location: "Coral Gables, FL",
    address: "789 Design Avenue, Coral Gables, FL 33134",
    timezone: "America/New_York",
    company: "MILLION Luxury Real Estate",
    department: "Design & Staging",
    employeeId: "EMP-003",
    isAvailable: true,
    schedule: "Mon-Fri 10AM-6PM",
    responseTime: "3 hours",
    totalSalesValue: 280000000,
    averagePrice: 3300000,
    clientSatisfaction: 96,
    linkedInUrl: "https://linkedin.com/in/emmarodriguez",
    instagramUrl: "https://instagram.com/emma_designs",
    facebookUrl: "https://facebook.com/emma.rodriguez.designer",
    lastActive: new Date("2024-01-15T09:30:00Z"),
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test1234",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-15T09:30:00Z")
  },
  {
    _id: "david-thompson",
    fullName: "David Thompson",
    email: "david.thompson@millionrealestate.com",
    phoneE164: "+15551234570",
    photoUrl: "https://blob.vercel-storage.com/owners/owner-004/photo.jpg",
    role: 5, // InvestmentAdvisor
    description: "Strategic advisor for premium real estate investments",
    title: "Investment Advisor",
    bio: "Strategic advisor for premium real estate investments with 22+ years in investment analysis and portfolio management.",
    experienceYears: 22,
    propertiesSold: 200,
    rating: 4.9,
    specialties: ["Investment Properties", "Portfolio Management", "Market Analysis"],
    languages: ["English", "French"],
    certifications: ["Certified Investment Advisor", "Real Estate Investment Specialist"],
    location: "Brickell, FL",
    address: "321 Financial District, Brickell, FL 33131",
    timezone: "America/New_York",
    company: "MILLION Luxury Real Estate",
    department: "Investment Advisory",
    employeeId: "EMP-004",
    isAvailable: true,
    schedule: "Mon-Fri 8AM-6PM",
    responseTime: "1 hour",
    totalSalesValue: 650000000,
    averagePrice: 3250000,
    clientSatisfaction: 99,
    linkedInUrl: "https://linkedin.com/in/davidthompson",
    instagramUrl: "https://instagram.com/david_investments",
    facebookUrl: "https://facebook.com/david.thompson.investor",
    lastActive: new Date("2024-01-15T12:00:00Z"),
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test1234",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-15T12:00:00Z")
  },
  {
    _id: "carlos-rodriguez",
    fullName: "Carlos Rodriguez",
    email: "carlos.rodriguez@million.com",
    phoneE164: "+13055551234",
    photoUrl: "https://blob.vercel-storage.com/owners/carlos-rodriguez",
    role: 6, // SeniorAgent
    description: "Experienced real estate agent specializing in residential properties",
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test1234",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z")
  },
  {
    _id: "maria-gonzalez",
    fullName: "Maria Gonzalez",
    email: "maria.gonzalez@million.com",
    phoneE164: "+13055555678",
    photoUrl: "https://blob.vercel-storage.com/owners/maria-gonzalez",
    role: 7, // PropertyManager
    description: "Dedicated property manager ensuring tenant satisfaction",
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test1234",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z")
  },
  {
    _id: "roberto-silva",
    fullName: "Roberto Silva",
    email: "roberto.silva@million.com",
    phoneE164: "+13055559012",
    photoUrl: "https://blob.vercel-storage.com/owners/roberto-silva",
    role: 8, // CommercialSpecialist
    description: "Expert in commercial real estate and investment properties",
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test1234",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z")
  },
  {
    _id: "owner-004",
    fullName: "Ana Martinez",
    email: "ana.martinez@million.com",
    phoneE164: "+13055553456",
    photoUrl: "https://blob.vercel-storage.com/owners/ana-martinez.jpg",
    role: 0, // Owner
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test1234",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z")
  },
  {
    _id: "owner-005",
    fullName: "Luis Fernandez",
    email: "luis.fernandez@million.com",
    phoneE164: "+13055557890",
    photoUrl: "https://blob.vercel-storage.com/owners/luis-fernandez.jpg",
    role: 0, // Owner
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test1234",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z")
  },
  {
    _id: "owner-006",
    fullName: "Carmen Vega",
    email: "carmen.vega@million.com",
    phoneE164: "+13055552345",
    photoUrl: "https://blob.vercel-storage.com/owners/carmen-vega.jpg",
    role: 0, // Owner
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test1234",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z")
  },
  {
    _id: "owner-007",
    fullName: "Javier Morales",
    email: "javier.morales@million.com",
    phoneE164: "+13055556789",
    photoUrl: "https://blob.vercel-storage.com/owners/javier-morales.jpg",
    role: 0, // Owner
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test1234",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z")
  },
  {
    _id: "owner-008",
    fullName: "Isabella Torres",
    email: "isabella.torres@million.com",
    phoneE164: "+13055550123",
    photoUrl: "https://blob.vercel-storage.com/owners/isabella-torres.jpg",
    role: 0, // Owner
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test1234",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z")
  },
  {
    _id: "owner-009",
    fullName: "Diego Herrera",
    email: "diego.herrera@million.com",
    phoneE164: "+13055554567",
    photoUrl: "https://blob.vercel-storage.com/owners/diego-herrera.jpg",
    role: 0, // Owner
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test1234",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z")
  }
];

// Generate 50 realistic properties
const generateProperties = () => {
  const cities = [
    { name: "Miami Beach", state: "FL", neighborhoods: ["South Beach", "Mid-Beach", "North Beach"] },
    { name: "Coral Gables", state: "FL", neighborhoods: ["Gables Estates", "Coral Way", "Miracle Mile"] },
    { name: "Brickell", state: "FL", neighborhoods: ["Brickell Key", "Brickell Avenue", "Brickell Bay"] },
    { name: "Key Biscayne", state: "FL", neighborhoods: ["Ocean Club", "Harbor Club", "Village"] },
    { name: "Bal Harbour", state: "FL", neighborhoods: ["Bal Harbour", "Surfside", "Bay Harbor"] }
  ];

  const propertyTypes = ["Villa", "Penthouse", "Mansion", "Estate", "Luxury Home"];
  const properties = [];

  for (let i = 1; i <= 50; i++) {
    const city = cities[Math.floor(Math.random() * cities.length)];
    const neighborhood = city.neighborhoods[Math.floor(Math.random() * city.neighborhoods.length)];
    const propertyType = propertyTypes[Math.floor(Math.random() * propertyTypes.length)];
    const owner = owners[Math.floor(Math.random() * owners.length)];
    
    const basePrice = Math.floor(Math.random() * 15000000) + 2000000; // $2M - $17M
    const bedrooms = Math.floor(Math.random() * 6) + 2; // 2-7 bedrooms
    const bathrooms = Math.floor(Math.random() * 4) + 2; // 2-5 bathrooms
    const size = Math.floor(Math.random() * 8000) + 2000; // 2,000 - 10,000 sq ft
    
    const property = {
      _id: `prop-${i.toString().padStart(3, '0')}`,
      ownerId: owner._id,
      name: `${propertyType} ${i} - ${neighborhood}`,
      address: `${Math.floor(Math.random() * 9999) + 1} ${neighborhood} Dr, ${city.name}, ${city.state}`,
      city: city.name,
      neighborhood: neighborhood,
      propertyType: propertyType,
      description: `Luxurious ${propertyType.toLowerCase()} in the prestigious ${neighborhood} neighborhood of ${city.name}. This stunning property features ${bedrooms} bedrooms, ${bathrooms} bathrooms, and ${size.toLocaleString()} square feet of living space.`,
      price: basePrice,
      codeInternal: `${city.name.substring(0, 2).toUpperCase()}${i.toString().padStart(3, '0')}`,
      year: Math.floor(Math.random() * 20) + 2000,
      size: size,
      bedrooms: bedrooms,
      bathrooms: bathrooms,
      hasPool: Math.random() > 0.3,
      hasGarden: Math.random() > 0.2,
      hasParking: Math.random() > 0.1,
      isFurnished: Math.random() > 0.4,
      availableFrom: new Date("2024-01-15T00:00:00Z"),
      availableTo: new Date("2024-12-31T00:00:00Z"),
      status: PropertyStatus.Available,
      cover: {
        Type: MediaType.Image,
        Url: `https://blob.vercel-storage.com/properties/prop-${i.toString().padStart(3, '0')}/cover.jpg`,
        Index: 0
      },
      media: [
        {
          Id: `media-${i}-1`,
          Type: MediaType.Image,
          Url: `https://blob.vercel-storage.com/properties/prop-${i.toString().padStart(3, '0')}/1.jpg`,
          Index: 1,
          Enabled: true,
          Featured: true
        },
        {
          Id: `media-${i}-2`,
          Type: MediaType.Image,
          Url: `https://blob.vercel-storage.com/properties/prop-${i.toString().padStart(3, '0')}/2.jpg`,
          Index: 2,
          Enabled: true,
          Featured: false
        },
        {
          Id: `media-${i}-3`,
          Type: MediaType.Video,
          Url: `https://blob.vercel-storage.com/properties/prop-${i.toString().padStart(3, '0')}/video.mp4`,
          Index: 3,
          Enabled: true,
          Featured: false
        }
      ],
      createdAt: new Date("2024-01-01T00:00:00Z"),
      updatedAt: new Date("2024-01-01T00:00:00Z"),
      isActive: true
    };

    properties.push(property);
  }

  return properties;
};

// Generate owner sessions
const generateOwnerSessions = () => {
  const sessions = [];
  
  owners.forEach(owner => {
    if (owner.role === 0) { // Owner role
      sessions.push({
        _id: `session-${owner._id}`,
        ownerId: owner._id,
        token: `jwt-token-${owner._id}-${Date.now()}`,
        expiresAt: new Date(Date.now() + 24 * 60 * 60 * 1000), // 24 hours
        createdAt: new Date(),
        isActive: true
      });
    }
  });

  return sessions;
};

// Generate property traces
const generatePropertyTraces = (properties) => {
  const traces = [];
  
  properties.forEach(property => {
    // Creation trace
    traces.push({
      _id: `trace-${property._id}-created`,
      PropertyId: property._id,
      Action: TraceAction.Created,
      PreviousValue: null,
      NewValue: property.name,
      Timestamp: property.createdAt,
      UserId: property.ownerId,
      Notes: `Property ${property.name} was created and listed for sale`,
      PropertyName: property.name,
      Price: property.price,
      Status: "Available"
    });

    // Price change trace (random)
    if (Math.random() > 0.7) {
      const oldPrice = property.price;
      const newPrice = Math.floor(oldPrice * (0.9 + Math.random() * 0.2)); // ±10% change
      
      traces.push({
        _id: `trace-${property._id}-price-${Date.now()}`,
        PropertyId: property._id,
        Action: TraceAction.PriceChanged,
        PreviousValue: oldPrice.toString(),
        NewValue: newPrice.toString(),
        Timestamp: new Date(property.createdAt.getTime() + Math.random() * 30 * 24 * 60 * 60 * 1000), // Random date within 30 days
        UserId: property.ownerId,
        Notes: `Price adjusted from $${oldPrice.toLocaleString()} to $${newPrice.toLocaleString()}`,
        PropertyName: property.name,
        Price: newPrice,
        Status: "Available"
      });
    }

    // Status change trace (random)
    if (Math.random() > 0.8) {
      const newStatus = Math.random() > 0.5 ? PropertyStatus.Rented : PropertyStatus.Sold;
      const statusText = newStatus === PropertyStatus.Rented ? "Rented" : "Sold";
      
      traces.push({
        _id: `trace-${property._id}-status-${Date.now()}`,
        PropertyId: property._id,
        Action: newStatus === PropertyStatus.Rented ? TraceAction.Rented : TraceAction.Sold,
        PreviousValue: "Available",
        NewValue: statusText,
        Timestamp: new Date(property.createdAt.getTime() + Math.random() * 60 * 24 * 60 * 60 * 1000), // Random date within 60 days
        UserId: property.ownerId,
        Notes: `Property ${statusText.toLowerCase()} successfully`,
        PropertyName: property.name,
        Price: property.price,
        Status: statusText
      });
    }

    // Media update trace (random)
    if (Math.random() > 0.6) {
      traces.push({
        _id: `trace-${property._id}-media-${Date.now()}`,
        PropertyId: property._id,
        Action: TraceAction.MediaUpdated,
        PreviousValue: "3 media items",
        NewValue: "4 media items",
        Timestamp: new Date(property.createdAt.getTime() + Math.random() * 45 * 24 * 60 * 60 * 1000), // Random date within 45 days
        UserId: property.ownerId,
        Notes: "Added new professional photos and virtual tour video",
        PropertyName: property.name,
        Price: property.price,
        Status: "Available"
      });
    }
  });

  return traces;
};

async function seedDatabase() {
  const client = new MongoClient(MONGODB_URI);
  
  try {
    console.log('🔌 Connecting to MongoDB Atlas...');
    await client.connect();
    console.log('✅ Connected successfully');
    
    const db = client.db('million');
    
    // Generate data
    const properties = generateProperties();
    const sessions = generateOwnerSessions();
    const traces = generatePropertyTraces(properties);
    
    // Clear existing data
    console.log('🧹 Clearing existing data...');
    await db.collection('properties').deleteMany({});
    await db.collection('owners').deleteMany({});
    await db.collection('owner_sessions').deleteMany({});
    await db.collection('property_traces').deleteMany({});
    console.log('✅ Existing data cleared');
    
    // Insert owners
    console.log('👥 Seeding owners...');
    const ownersResult = await db.collection('owners').insertMany(owners);
    console.log(`✅ Inserted ${ownersResult.insertedCount} owners`);
    
    // Insert properties
    console.log('🏠 Seeding properties...');
    const propertiesResult = await db.collection('properties').insertMany(properties);
    console.log(`✅ Inserted ${propertiesResult.insertedCount} properties`);
    
    // Insert sessions
    console.log('🔐 Seeding owner sessions...');
    const sessionsResult = await db.collection('owner_sessions').insertMany(sessions);
    console.log(`✅ Inserted ${sessionsResult.insertedCount} sessions`);
    
    // Insert traces
    console.log('📊 Seeding property traces...');
    const tracesResult = await db.collection('property_traces').insertMany(traces);
    console.log(`✅ Inserted ${tracesResult.insertedCount} traces`);
    
    // Create indexes
    console.log('🔍 Creating indexes...');
    try {
      await db.collection('properties').createIndex({ "ownerId": 1 });
      await db.collection('properties').createIndex({ "city": 1 });
      await db.collection('properties').createIndex({ "propertyType": 1 });
      await db.collection('properties').createIndex({ "price": 1 });
      await db.collection('properties').createIndex({ "status": 1 });
      await db.collection('properties').createIndex({ "isActive": 1 });
      await db.collection('owners').createIndex({ "email": 1 }, { unique: true });
      await db.collection('owner_sessions').createIndex({ "ownerId": 1 });
      await db.collection('owner_sessions').createIndex({ "token": 1 });
      await db.collection('property_traces').createIndex({ "propertyId": 1 });
      await db.collection('property_traces').createIndex({ "timestamp": 1 });
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
    console.log(`📊 Total sessions: ${sessions.length}`);
    console.log(`📊 Total traces: ${traces.length}`);
    
    // Verify data
    console.log('\n🔍 Verifying data...');
    const ownerCount = await db.collection('owners').countDocuments({});
    const propertyCount = await db.collection('properties').countDocuments({});
    const sessionCount = await db.collection('owner_sessions').countDocuments({});
    const traceCount = await db.collection('property_traces').countDocuments({});
    const activePropertyCount = await db.collection('properties').countDocuments({ isActive: true });
    
    console.log(`👥 Owners in database: ${ownerCount}`);
    console.log(`🏠 Properties in database: ${propertyCount}`);
    console.log(`🔐 Sessions in database: ${sessionCount}`);
    console.log(`📊 Traces in database: ${traceCount}`);
    console.log(`✅ Active properties: ${activePropertyCount}`);
    
    // Show sample data
    console.log('\n📋 Sample data:');
    const sampleProperty = await db.collection('properties').findOne({});
    const sampleTrace = await db.collection('property_traces').findOne({});
    
    if (sampleProperty) {
      console.log(`🏠 Sample property: ${sampleProperty.name} - $${sampleProperty.price.toLocaleString()}`);
    }
    if (sampleTrace) {
      console.log(`📊 Sample trace: ${sampleTrace.action} for property ${sampleTrace.propertyName}`);
    }
    
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
