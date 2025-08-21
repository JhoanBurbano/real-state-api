// Run with: mongosh "mongodb://localhost:27017/million" ops/db/create-indexes.js
const dbName = db.getName();
print(`Using DB: ${dbName}`);
const coll = db.getCollection('properties');
coll.createIndex({ Name: "text", AddressProperty: "text" }, { name: "idx_text_name_address" });
coll.createIndex({ PriceProperty: 1 }, { name: "idx_price" });
print("Indexes created");

