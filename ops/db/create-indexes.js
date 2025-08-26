// Run with: mongosh "mongodb://localhost:27017/million" ops/db/create-indexes.js
const dbName = db.getName();
print(`Using DB: ${dbName}`);

// Properties collection indexes
print("Creating properties indexes...");
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

// Owners collection indexes
print("Creating owners indexes...");
const ownersColl = db.getCollection('owners');
ownersColl.createIndex({ "email": 1 }, { name: "idx_email", unique: true });
ownersColl.createIndex({ "role": 1 }, { name: "idx_role" });
ownersColl.createIndex({ "isActive": 1 }, { name: "idx_owner_is_active" });
ownersColl.createIndex({ "createdAt": -1 }, { name: "idx_owner_created_at" });

// Owner sessions collection indexes
print("Creating owner_sessions indexes...");
const sessionsColl = db.getCollection('owner_sessions');
sessionsColl.createIndex({ "ownerId": 1 }, { name: "idx_session_owner_id" });
sessionsColl.createIndex({ "refreshTokenHash": 1 }, { name: "idx_refresh_token_hash" });
sessionsColl.createIndex({ "expiresAt": 1 }, { name: "idx_expires_at", expireAfterSeconds: 0 });
sessionsColl.createIndex({ "revokedAt": 1 }, { name: "idx_revoked_at" });

print("All indexes created successfully!");

