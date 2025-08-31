const { MongoClient } = require('mongodb');

const uri = "mongodb+srv://jsburbano:EmpanadasConAji123@pruebastecnicas.sm4lf1d.mongodb.net/?retryWrites=true&w=majority&appName=pruebastecnicas";
const client = new MongoClient(uri);

async function createTestUser() {
    try {
        await client.connect();
        console.log("Connected to MongoDB");

        const database = client.db("million");
        const ownersCollection = database.collection("owners");

        // Check if test user already exists
        const existingUser = await ownersCollection.findOne({ email: "test@example.com" });
        if (existingUser) {
            console.log("Test user already exists:", existingUser.email);
            return;
        }

        // Create test user
        const testUser = {
            _id: "test-user-123",
            fullName: "Test User",
            email: "test@example.com",
            role: "Owner",
            phoneE164: "+1234567890",
            photoUrl: "https://example.com/photos/test.jpg",
            passwordHash: "$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi", // password: test1234
            isActive: true,
            createdAt: new Date(),
            updatedAt: new Date(),
            lastActive: new Date()
        };

        const result = await ownersCollection.insertOne(testUser);
        console.log("Test user created successfully:", result.insertedId);

    } catch (error) {
        console.error("Error creating test user:", error);
    } finally {
        await client.close();
    }
}

createTestUser();
