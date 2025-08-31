const { MongoClient } = require('mongodb');

const uri = "mongodb+srv://jsburbano:EmpanadasConAji123@pruebastecnicas.sm4lf1d.mongodb.net/?retryWrites=true&w=majority&appName=pruebastecnicas";
const client = new MongoClient(uri);

async function updateTestUser() {
    try {
        await client.connect();
        console.log("Connected to MongoDB");

        const database = client.db("million");
        const ownersCollection = database.collection("owners");

        // Update test user with correct password hash
        const result = await ownersCollection.updateOne(
            { email: "test@example.com" },
            { 
                $set: { 
                    passwordHash: "$2b$11$y7OK.bVcYM0MoUpYq7Q9Zehl2xxBnZdGeyMJN4ldCn4nzvXXYYpmC"
                } 
            }
        );

        if (result.modifiedCount > 0) {
            console.log("Test user password updated successfully");
        } else {
            console.log("Test user not found or no changes made");
        }

    } catch (error) {
        console.error("Error updating test user:", error);
    } finally {
        await client.close();
    }
}

updateTestUser();
