// Run with: mongosh "mongodb://localhost:27017/million" ops/db/seed-database.js
const dbName = db.getName();
print(`🌱 Seeding database: ${dbName}`);

// Clear existing data
print("🧹 Clearing existing data...");
db.properties.drop();
db.owners.drop();
db.owner_sessions.drop();

// Seed Owners
print("👥 Seeding owners...");
const owners = [
  {
    _id: "owner-001",
    fullName: "Carlos Rodriguez",
    email: "carlos.rodriguez@million.com",
    phoneE164: "+13055551234",
    photoUrl: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/owners/carlos-rodriguez.jpg",
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
    photoUrl: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/owners/maria-gonzalez.jpg",
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
    photoUrl: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/owners/david-johnson.jpg",
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
    photoUrl: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/owners/admin.jpg",
    role: "Admin",
    isActive: true,
    passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z")
  }
];

db.owners.insertMany(owners);
print(`✅ Inserted ${owners.length} owners`);

// Seed Properties
print("🏠 Seeding properties...");
const properties = [
  {
    _id: "prop-001",
    ownerId: "owner-001",
    name: "Oceanfront Villa",
    address: "100 Ocean Dr, Miami Beach, FL",
    price: 12500000,
    codeInternal: "MB001",
    year: 2020,
    status: "Active",
    cover: {
      type: "Image",
      url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mb001/prop-mb001_photo-01.jpg",
      index: 0
    },
    media: [
      {
        id: "media-001",
        type: "Image",
        url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mb001/1.jpg",
        index: 1,
        enabled: true,
        featured: true
      },
      {
        id: "media-002",
        type: "Image",
        url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mb001/2.jpg",
        index: 2,
        enabled: true,
        featured: true
      },
      {
        id: "media-003",
        type: "Image",
        url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mb001/3.jpg",
        index: 3,
        enabled: true,
        featured: true
      }
    ],
    description: "Luxurious oceanfront villa with panoramic views of the Atlantic Ocean",
    city: "Miami Beach",
    neighborhood: "South Beach",
    propertyType: "Villa",
    size: 8500,
    bedrooms: 5,
    bathrooms: 6,
    hasPool: true,
    hasGarden: true,
    hasParking: true,
    isFurnished: true,
    availableFrom: new Date("2024-01-15T00:00:00Z"),
    availableTo: new Date("2024-12-31T00:00:00Z"),
    traces: [
      {
        id: "trace-001",
        dateSale: new Date("2020-06-15T00:00:00Z"),
        name: "Previous Sale",
        value: 11000000,
        tax: 750000
      }
    ],
    coverImage: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mb001/prop-mb001_photo-01.jpg",
    images: [
      {
        url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mb001/1.jpg",
        index: 1,
        enabled: true
      },
      {
        url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mb001/2.jpg",
        index: 2,
        enabled: true
      },
      {
        url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mb001/3.jpg",
        index: 3,
        enabled: true
      }
    ],
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z"),
    isActive: true
  },
  {
    _id: "prop-002",
    ownerId: "owner-002",
    name: "Coral Gables Estate",
    address: "200 Coral Way, Coral Gables, FL",
    price: 6800000,
    codeInternal: "CG001",
    year: 2018,
    status: "Active",
    cover: {
      type: "Image",
      url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/cg001/prop-cg001_photo-01.jpg",
      index: 0
    },
    media: [
      {
        id: "media-004",
        type: "Image",
        url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/cg001/1.jpg",
        index: 1,
        enabled: true,
        featured: true
      },
      {
        id: "media-005",
        type: "Image",
        url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/cg001/2.jpg",
        index: 2,
        enabled: true,
        featured: true
      }
    ],
    description: "Elegant estate in the prestigious Coral Gables area",
    city: "Coral Gables",
    neighborhood: "Old Cutler",
    propertyType: "Estate",
    size: 6200,
    bedrooms: 4,
    bathrooms: 5,
    hasPool: true,
    hasGarden: true,
    hasParking: true,
    isFurnished: false,
    availableFrom: new Date("2024-02-01T00:00:00Z"),
    availableTo: new Date("2024-11-30T00:00:00Z"),
    traces: [],
    coverImage: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/cg001/prop-cg001_photo-01.jpg",
    images: [
      {
        url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/cg001/1.jpg",
        index: 1,
        enabled: true
      },
      {
        url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/cg001/2.jpg",
        index: 2,
        enabled: true
      }
    ],
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z"),
    isActive: true
  },
  {
    _id: "prop-003",
    ownerId: "owner-003",
    name: "Brickell Sky Penthouse",
    address: "300 Brickell Ave, Miami, FL",
    price: 9800000,
    codeInternal: "BK001",
    year: 2022,
    status: "Active",
    cover: {
      type: "Image",
      url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/bk001/prop-bk001_photo-01.jpg",
      index: 0
    },
    media: [
      {
        id: "media-006",
        type: "Image",
        url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/bk001/1.jpg",
        index: 1,
        enabled: true,
        featured: true
      },
      {
        id: "media-007",
        type: "Image",
        url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/bk001/2.jpg",
        index: 2,
        enabled: true,
        featured: true
      },
      {
        id: "media-008",
        type: "Image",
        url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/bk001/3.jpg",
        index: 3,
        enabled: true,
        featured: false
      }
    ],
    description: "Ultra-modern penthouse with stunning city and bay views",
    city: "Miami",
    neighborhood: "Brickell",
    propertyType: "Penthouse",
    size: 3800,
    bedrooms: 3,
    bathrooms: 4,
    hasPool: true,
    hasGarden: false,
    hasParking: true,
    isFurnished: true,
    availableFrom: new Date("2024-01-20T00:00:00Z"),
    availableTo: new Date("2024-10-31T00:00:00Z"),
    traces: [],
    coverImage: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/bk001/prop-bk001_photo-01.jpg",
    images: [
      {
        url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/bk001/1.jpg",
        index: 1,
        enabled: true
      },
      {
        url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/bk001/2.jpg",
        index: 2,
        enabled: true
      },
      {
        url: "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/bk001/3.jpg",
        index: 3,
        enabled: true
      }
    ],
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z"),
    isActive: true
  }
];

db.properties.insertMany(properties);
print(`✅ Inserted ${properties.length} properties`);

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

db.owner_sessions.insertMany(sessions);
print(`✅ Inserted ${sessions.length} owner sessions`);

// Create indexes
print("🔍 Creating indexes...");
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

const ownersColl = db.getCollection('owners');
ownersColl.createIndex({ "email": 1 }, { name: "idx_email", unique: true });
ownersColl.createIndex({ "role": 1 }, { name: "idx_role" });
ownersColl.createIndex({ "isActive": 1 }, { name: "idx_owner_is_active" });
ownersColl.createIndex({ "createdAt": -1 }, { name: "idx_owner_created_at" });

const sessionsColl = db.getCollection('owner_sessions');
sessionsColl.createIndex({ "ownerId": 1 }, { name: "idx_session_owner_id" });
sessionsColl.createIndex({ "refreshTokenHash": 1 }, { name: "idx_refresh_token_hash" });
sessionsColl.createIndex({ "expiresAt": 1 }, { name: "idx_expires_at", expireAfterSeconds: 0 });
sessionsColl.createIndex({ "revokedAt": 1 }, { name: "idx_revoked_at" });

print("🎉 Database seeding completed successfully!");
print(`📊 Summary:`);
print(`   - Owners: ${db.owners.countDocuments()}`);
print(`   - Properties: ${db.properties.countDocuments()}`);
print(`   - Sessions: ${db.owner_sessions.countDocuments()}`);
print(`   - Indexes: ${db.properties.getIndexes().length + db.owners.getIndexes().length + db.owner_sessions.getIndexes().length}`);

