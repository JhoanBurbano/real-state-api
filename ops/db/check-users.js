const { MongoClient } = require('mongodb');

const uri = "mongodb+srv://jsburbano:EmpanadasConAji123@pruebastecnicas.sm4lf1d.mongodb.net/?retryWrites=true&w=majority&appName=pruebastecnicas";
const client = new MongoClient(uri);

async function checkUsers() {
    try {
        await client.connect();
        console.log("Connected to MongoDB");

        const database = client.db("million");
        const ownersCollection = database.collection("owners");

        // Get all users
        const users = await ownersCollection.find({}).toArray();
        
        console.log("Total users found:", users.length);
        users.forEach(user => {
            console.log("User:", {
                id: user._id,
                email: user.email,
                fullName: user.fullName,
                role: user.role,
                isActive: user.isActive,
                passwordHash: user.passwordHash ? user.passwordHash.substring(0, 20) + "..." : "null"
            });
        });

    } catch (error) {
        console.error("Error checking users:", error);
    } finally {
        await client.close();
    }
}

checkUsers();
