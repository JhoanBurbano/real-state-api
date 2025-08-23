// Script de inicialización para MongoDB
// Se ejecuta automáticamente cuando se crea el contenedor

print('🚀 Inicializando base de datos Million Real Estate...');

// Crear base de datos
db = db.getSiblingDB('million');

// Crear usuario para la aplicación
db.createUser({
  user: 'million_app',
  pwd: 'million_app_password',
  roles: [
    { role: 'readWrite', db: 'million' }
  ]
});

print('✅ Usuario de aplicación creado');

// Crear colecciones
db.createCollection('properties');
db.createCollection('owners');
db.createCollection('owner_sessions');

print('✅ Colecciones creadas');

// Crear índices básicos
db.properties.createIndex({ "name": "text", "address": "text" });
db.properties.createIndex({ "price": 1 });
db.properties.createIndex({ "ownerId": 1 });
db.properties.createIndex({ "codeInternal": 1 }, { unique: true });
db.properties.createIndex({ "isActive": 1 });

db.owners.createIndex({ "email": 1 }, { unique: true });
db.owners.createIndex({ "role": 1 });
db.owners.createIndex({ "isActive": 1 });

db.owner_sessions.createIndex({ "ownerId": 1 });
db.owner_sessions.createIndex({ "refreshTokenHash": 1 });
db.owner_sessions.createIndex({ "expiresAt": 1 }, { expireAfterSeconds: 0 });

print('✅ Índices creados');

// Insertar datos de ejemplo (opcional)
db.owners.insertOne({
  _id: "admin-001",
  fullName: "Admin User",
  email: "admin@million.com",
  role: "admin",
  isActive: true,
  passwordHash: "$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test", // Cambiar en producción
  createdAt: new Date(),
  updatedAt: new Date()
});

print('✅ Usuario admin creado');

print('🎉 Base de datos Million Real Estate inicializada correctamente!');

