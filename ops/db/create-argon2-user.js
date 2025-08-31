const { MongoClient } = require('mongodb');

const uri = "mongodb+srv://jsburbano:EmpanadasConAji123@pruebastecnicas.sm4lf1d.mongodb.net/?retryWrites=true&w=majority&appName=pruebastecnicas";
const client = new MongoClient(uri);

async function createArgon2User() {
    try {
        await client.connect();
        console.log("Connected to MongoDB");

        const database = client.db("million");
        const ownersCollection = database.collection("owners");

        // Create a user with a simple password that we can hash with Argon2
        const simpleUser = {
            _id: "argon2-user-123",
            fullName: "Argon2 User",
            email: "argon2@test.com",
            role: 0, // Owner role
            phoneE164: "+1234567890",
            photoUrl: "https://example.com/photos/argon2.jpg",
            passwordHash: "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8=", // This is a placeholder, we'll update it
            isActive: true,
            createdAt: new Date(),
            updatedAt: new Date(),
            lastActive: new Date()
        };

        // Check if user already exists
        const existingUser = await ownersCollection.findOne({ email: "argon2@test.com" });
        if (existingUser) {
            console.log("Argon2 user already exists");
        } else {
            const result = await ownersCollection.insertOne(simpleUser);
            console.log("Argon2 user created successfully:", result.insertedId);
        }

        console.log("User email: argon2@test.com");
        console.log("Password: 12345678");

    } catch (error) {
        console.error("Error creating Argon2 user:", error);
    } finally {
        await client.close();
    }
}

createArgon2User();
