// Run with: docker exec -i million-mongodb-dev mongosh "mongodb://localhost:27017/million" < ops/db/seed-database-50-properties.js
const dbName = db.getName();
print(`🌱 Seeding database: ${dbName} with 50 properties`);

// Helper functions - Define these first
function generateMedia(propertyIndex) {
  const mediaCount = 3 + (propertyIndex % 8); // 3-10 media items
  const media = [];
  
  for (let j = 1; j <= mediaCount; j++) {
    media.push({
      id: `media-${propertyIndex}-${j}`,
      type: "Image",
      url: `https://blob.vercel-storage.com/properties/prop-${String(propertyIndex).padStart(3, '0')}/${j}.jpg`,
      index: j,
      enabled: true,
      featured: j <= 3 // First 3 are featured
    });
  }
  
  return media;
}

function generateLegacyImages(propertyIndex) {
  const imageCount = 3 + (propertyIndex % 8);
  const images = [];
  
  for (let j = 1; j <= imageCount; j++) {
    images.push({
      url: `https://blob.vercel-storage.com/properties/prop-${String(propertyIndex).padStart(3, '0')}/${j}.jpg`,
      index: j,
      enabled: true
    });
  }
  
  return images;
}

function generateDescription(propertyType, city, neighborhood, bedrooms, bathrooms) {
  const descriptions = [
    `Stunning ${propertyType.toLowerCase()} in the prestigious ${neighborhood} area of ${city}`,
    `Luxurious ${propertyType.toLowerCase()} with ${bedrooms} bedrooms and ${bathrooms} bathrooms in ${city}`,
    `Beautiful ${propertyType.toLowerCase()} located in the heart of ${neighborhood}, ${city}`,
    `Modern ${propertyType.toLowerCase()} offering the perfect blend of luxury and comfort in ${city}`,
    `Elegant ${propertyType.toLowerCase()} in the exclusive ${neighborhood} neighborhood of ${city}`,
    `Sophisticated ${propertyType.toLowerCase()} with premium finishes in ${city}`,
    `Contemporary ${propertyType.toLowerCase()} featuring ${bedrooms} spacious bedrooms in ${city}`,
    `Exquisite ${propertyType.toLowerCase()} in the desirable ${neighborhood} area of ${city}`,
    `Premium ${propertyType.toLowerCase()} with ${bathrooms} designer bathrooms in ${city}`,
    `Charming ${propertyType.toLowerCase()} in the vibrant ${neighborhood} community of ${city}`
  ];
  
  return descriptions[propertyIndex % descriptions.length];
}

function generateTraces(propertyIndex, currentPrice) {
  if (propertyIndex % 3 === 0) { // 1/3 of properties have traces
    const previousPrice = currentPrice * (0.8 + Math.random() * 0.3); // 80-110% of current price
    const tax = previousPrice * 0.05; // 5% tax
    
    return [{
      id: `trace-${propertyIndex}`,
      dateSale: new Date(Date.now() - (Math.random() * 5 * 365 * 24 * 60 * 60 * 1000)), // 0-5 years ago
      name: "Previous Sale",
      value: Math.round(previousPrice),
      tax: Math.round(tax)
    }];
  }
  
  return [];
}

// Clear existing data
print("🧹 Clearing existing data...");
try {
  db.properties.drop();
  print("✅ Properties collection dropped");
} catch (e) {
  print("⚠️ Could not drop properties collection: " + e.message);
}

try {
  db.owners.drop();
  print("✅ Owners collection dropped");
} catch (e) {
  print("⚠️ Could not drop owners collection: " + e.message);
}

try {
  db.owner_sessions.drop();
  print("✅ Owner sessions collection dropped");
} catch (e) {
  print("⚠️ Could not drop owner_sessions collection: " + e.message);
}

// Seed Owners
print("👥 Seeding owners...");
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
    _id: "owner-003",
    fullName: "David Johnson",
    email: "david.johnson@million.com",
    phoneE164: "+13055559012",
    photoUrl: "https://blob.vercel-storage.com/owners/david-johnson.jpg",
    role: "Owner",
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z")
  },
  {
    _id: "owner-004",
    fullName: "Ana Martinez",
    email: "ana.martinez@million.com",
    phoneE164: "+13055554567",
    photoUrl: "https://blob.vercel-storage.com/owners/ana-martinez.jpg",
    role: "Owner",
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z")
  },
  {
    _id: "owner-005",
    fullName: "Roberto Silva",
    email: "roberto.silva@million.com",
    phoneE164: "+13055557890",
    photoUrl: "https://blob.vercel-storage.com/owners/roberto-silva.jpg",
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

try {
  db.owners.insertMany(owners);
  print(`✅ Inserted ${owners.length} owners`);
} catch (e) {
  print("❌ Error inserting owners: " + e.message);
}

// Property data templates
const propertyTypes = ["Villa", "Penthouse", "Estate", "Condo", "Townhouse", "Mansion", "Beach House", "Golf Villa", "Waterfront Home", "Modern House"];
const cities = ["Miami Beach", "Coral Gables", "Miami", "Key Biscayne", "Bal Harbour", "Surfside", "Bay Harbor Islands", "Aventura", "Hollywood", "Fort Lauderdale", "Boca Raton", "Palm Beach", "Delray Beach", "West Palm Beach", "Miami Shores", "North Miami", "Doral", "Edgewater", "Wynwood", "Design District"];
const neighborhoods = ["South Beach", "North Beach", "Mid-Beach", "Point Dume", "Old Cutler", "Gables Estates", "Brickell", "Downtown", "Coconut Grove", "Key Colony", "Bal Harbour", "Surfside", "BHI", "Aventura", "Hollywood Beach", "Las Olas", "Boca Raton", "Palm Beach", "Delray Beach", "Clematis Street", "Miami Shores", "North Miami", "Doral", "Edgewater", "Wynwood", "Design District"];

// Generate 50 properties
print("🏠 Seeding 50 properties...");
const properties = [];

for (let i = 1; i <= 50; i++) {
  const propertyType = propertyTypes[i % propertyTypes.length];
  const city = cities[i % cities.length];
  const neighborhood = neighborhoods[i % neighborhoods.length];
  const ownerId = `owner-${String((i % 5) + 1).padStart(3, '0')}`;
  const year = 2015 + (i % 10);
  const price = 500000 + (i * 250000) + (Math.random() * 1000000);
  const size = 1500 + (i * 100) + (Math.random() * 2000);
  const bedrooms = 2 + (i % 4);
  const bathrooms = 2 + (i % 3);
  
  const property = {
    _id: `prop-${String(i).padStart(3, '0')}`,
    ownerId: ownerId,
    name: `${propertyType} ${i} - ${city}`,
    address: `${100 + i} ${city} Dr, ${city}, FL`,
    price: Math.round(price),
    codeInternal: `${city.substring(0, 2).toUpperCase()}${String(i).padStart(3, '0')}`,
    year: year,
    status: "Active",
    cover: {
      type: "Image",
      url: `https://blob.vercel-storage.com/properties/prop-${String(i).padStart(3, '0')}/cover.jpg`,
      index: 0
    },
    media: generateMedia(i),
    description: generateDescription(propertyType, city, neighborhood, bedrooms, bathrooms),
    city: city,
    neighborhood: neighborhood,
    propertyType: propertyType,
    size: Math.round(size),
    bedrooms: bedrooms,
    bathrooms: bathrooms,
    hasPool: Math.random() > 0.3,
    hasGarden: Math.random() > 0.4,
    hasParking: Math.random() > 0.2,
    isFurnished: Math.random() > 0.6,
    availableFrom: new Date(Date.now() + (Math.random() * 365 * 24 * 60 * 60 * 1000)),
    availableTo: new Date(Date.now() + (365 + Math.random() * 365) * 24 * 60 * 60 * 1000),
    traces: generateTraces(i, price),
    coverImage: `https://blob.vercel-storage.com/properties/prop-${String(i).padStart(3, '0')}/cover.jpg`,
    images: generateLegacyImages(i),
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z"),
    isActive: true
  };
  
  properties.push(property);
}

try {
  db.properties.insertMany(properties);
  print(`✅ Inserted ${properties.length} properties`);
} catch (e) {
  print("❌ Error inserting properties: " + e.message);
}

// Seed Owner Sessions
print("🔐 Seeding owner sessions...");
const sessions = [
  {
    _id: "session-001",
    ownerId: "admin-001",
    refreshTokenHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test",
    ip: "192.168.1.100",
    userAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
    issuedAt: new Date("2024-01-01T00:00:00Z"),
    expiresAt: new Date("2024-12-31T23:59:59Z"),
    revokedAt: null,
    rotatedAt: null
  },
  {
    _id: "session-002",
    ownerId: "owner-001",
    refreshTokenHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test",
    ip: "192.168.1.101",
    userAgent: "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36",
    issuedAt: new Date("2024-01-01T00:00:00Z"),
    expiresAt: new Date("2024-12-31T23:59:59Z"),
    revokedAt: null,
    rotatedAt: null
  },
  {
    _id: "session-003",
    ownerId: "owner-002",
    refreshTokenHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test",
    ip: "192.168.1.102",
    userAgent: "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15",
    issuedAt: new Date("2024-01-01T00:00:00Z"),
    expiresAt: new Date("2024-12-31T23:59:59Z"),
    revokedAt: null,
    rotatedAt: null
  }
];

try {
  db.owner_sessions.insertMany(sessions);
  print(`✅ Inserted ${sessions.length} owner sessions`);
} catch (e) {
  print("❌ Error inserting sessions: " + e.message);
}

// Create indexes
print("🔍 Creating indexes...");
try {
  const propertiesColl = db.getCollection('properties');
  propertiesColl.createIndex({ "name": "text", "address": "text" }, { name: "idx_text_name_address" });
  propertiesColl.createIndex({ "price": 1 }, { name: "idx_price" });
  propertiesColl.createIndex({ "codeInternal": 1 }, { name: "idx_code_internal", unique: true });
  propertiesColl.createIndex({ "ownerId": 1 }, { name: "idx_owner_id" });
  propertiesColl.createIndex({ "status": 1 }, { name: "idx_status" });
  propertiesColl.createIndex({ "city": 1 }, { name: "idx_city" });
  propertiesColl.createIndex({ "neighborhood": 1 }, { name: "idx_neighborhood" });
  propertiesColl.createIndex({ "propertyType": 1 }, { name: "idx_property_type" });
  propertiesColl.createIndex({ "bedrooms": 1 }, { name: "idx_bedrooms" });
  propertiesColl.createIndex({ "bathrooms": 1 }, { name: "idx_bathrooms" });
  propertiesColl.createIndex({ "year": 1 }, { name: "idx_year" });
  propertiesColl.createIndex({ "isActive": 1 }, { name: "idx_is_active" });
  propertiesColl.createIndex({ "createdAt": -1 }, { name: "idx_created_at" });
  print("✅ Properties indexes created");
} catch (e) {
  print("⚠️ Error creating properties indexes: " + e.message);
}

try {
  const ownersColl = db.getCollection('owners');
  ownersColl.createIndex({ "email": 1 }, { name: "idx_email", unique: true });
  ownersColl.createIndex({ "role": 1 }, { name: "idx_role" });
  ownersColl.createIndex({ "isActive": 1 }, { name: "idx_owner_is_active" });
  ownersColl.createIndex({ "createdAt": -1 }, { name: "idx_owner_created_at" });
  print("✅ Owners indexes created");
} catch (e) {
  print("⚠️ Error creating owners indexes: " + e.message);
}

try {
  const sessionsColl = db.getCollection('owner_sessions');
  sessionsColl.createIndex({ "ownerId": 1 }, { name: "idx_session_owner_id" });
  sessionsColl.createIndex({ "refreshTokenHash": 1 }, { name: "idx_refresh_token_hash" });
  sessionsColl.createIndex({ "expiresAt": 1 }, { name: "idx_expires_at", expireAfterSeconds: 0 });
  sessionsColl.createIndex({ "revokedAt": 1 }, { name: "idx_revoked_at" });
  print("✅ Sessions indexes created");
} catch (e) {
  print("⚠️ Error creating sessions indexes: " + e.message);
}

print("🎉 Database seeding completed successfully!");
print(`📊 Summary:`);
print(`   - Owners: ${db.owners.countDocuments()}`);
print(`   - Properties: ${db.properties.countDocuments()}`);
print(`   - Sessions: ${db.owner_sessions.countDocuments()}`);

// Count indexes
try {
  const propertiesIndexes = db.properties.getIndexes().length;
  const ownersIndexes = db.owners.getIndexes().length;
  const sessionsIndexes = db.owner_sessions.getIndexes().length;
  print(`   - Total Indexes: ${propertiesIndexes + ownersIndexes + sessionsIndexes}`);
} catch (e) {
  print("   - Indexes: Could not count (authentication required)");
}

// Show sample of properties
print("\n🏠 Sample Properties Created:");
const sampleProperties = db.properties.find().limit(5).toArray();
sampleProperties.forEach((prop, index) => {
  print(`   ${index + 1}. ${prop.name} - $${prop.price.toLocaleString()} - ${prop.city}`);
});
