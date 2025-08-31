const { MongoClient } = require('mongodb');

const uri = "mongodb+srv://jsburbano:EmpanadasConAji123@pruebastecnicas.sm4lf1d.mongodb.net/?retryWrites=true&w=majority&appName=pruebastecnicas";
const client = new MongoClient(uri);

async function assignPropertiesToUser() {
    try {
        await client.connect();
        console.log("Connected to MongoDB");

        const database = client.db("million");
        const propertiesCollection = database.collection("properties");

        // Get some existing properties and assign them to our test user
        const properties = await propertiesCollection.find({}).limit(3).toArray();
        
        if (properties.length > 0) {
            // Update the first 3 properties to belong to our test user
            const updatePromises = properties.map((property, index) => {
                return propertiesCollection.updateOne(
                    { _id: property._id },
                    { 
                        $set: { 
                            ownerId: "argon2-user-123",
                            name: `Test Property ${index + 1}`,
                            address: `Test Address ${index + 1}`,
                            price: 100000 + (index * 50000)
                        } 
                    }
                );
            });

            await Promise.all(updatePromises);
            console.log(`Updated ${properties.length} properties for test user`);
        } else {
            console.log("No properties found to assign");
        }

    } catch (error) {
        console.error("Error assigning properties:", error);
    } finally {
        await client.close();
    }
}

assignPropertiesToUser();
