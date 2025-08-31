const { MongoClient } = require('mongodb');

const uri = "mongodb+srv://jsburbano:EmpanadasConAji123@pruebastecnicas.sm4lf1d.mongodb.net/?retryWrites=true&w=majority&appName=pruebastecnicas";
const client = new MongoClient(uri);

async function updateArgon2User() {
    try {
        await client.connect();
        console.log("Connected to MongoDB");

        const database = client.db("million");
        const ownersCollection = database.collection("owners");

        // Update Argon2 user with correct password hash
        const result = await ownersCollection.updateOne(
            { email: "argon2@test.com" },
            { 
                $set: { 
                    passwordHash: "CJrgyBQ4guIzW12+dtubmwRcywJvK92RZAICGF8LcU+TVyU8WYBWuOQ1QgRYK8p2"
                } 
            }
        );

        if (result.modifiedCount > 0) {
            console.log("Argon2 user password updated successfully");
        } else {
            console.log("Argon2 user not found or no changes made");
        }

    } catch (error) {
        console.error("Error updating Argon2 user:", error);
    } finally {
        await client.close();
    }
}

updateArgon2User();
