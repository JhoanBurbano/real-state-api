const { MongoClient } = require('mongodb');
const bcrypt = require('bcrypt');

const uri = "mongodb+srv://jsburbano:EmpanadasConAji123@pruebastecnicas.sm4lf1d.mongodb.net/?retryWrites=true&w=majority&appName=pruebastecnicas";
const client = new MongoClient(uri);

async function createSimpleUser() {
    try {
        await client.connect();
        console.log("Connected to MongoDB");

        const database = client.db("million");
        const ownersCollection = database.collection("owners");

        const password = "12345678";
        const hash = await bcrypt.hash(password, 11);

        // Create a simple test user
        const simpleUser = {
            _id: "simple-user-123",
            fullName: "Simple User",
            email: "simple@test.com",
            role: 0, // Owner role
            phoneE164: "+1234567890",
            photoUrl: "https://example.com/photos/simple.jpg",
            passwordHash: hash,
            isActive: true,
            createdAt: new Date(),
            updatedAt: new Date(),
            lastActive: new Date()
        };

        // Check if user already exists
        const existingUser = await ownersCollection.findOne({ email: "simple@test.com" });
        if (existingUser) {
            console.log("Simple user already exists, updating password");
            await ownersCollection.updateOne(
                { email: "simple@test.com" },
                { $set: { passwordHash: hash } }
            );
        } else {
            const result = await ownersCollection.insertOne(simpleUser);
            console.log("Simple user created successfully:", result.insertedId);
        }

        console.log("Password:", password);
        console.log("Hash:", hash);

    } catch (error) {
        console.error("Error creating simple user:", error);
    } finally {
        await client.close();
    }
}

createSimpleUser();
