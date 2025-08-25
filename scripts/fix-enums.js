const { MongoClient } = require('mongodb');

const MONGO_URI = 'mongodb://localhost:27017';
const DATABASE = 'million';

// C# enum values
const MediaType = {
    Image: 0,
    Video: 1
};

const PropertyStatus = {
    Available: 0,
    Rented: 1,
    Sold: 2,
    UnderMaintenance: 3
};

async function fixEnums() {
    const client = new MongoClient(MONGO_URI);
    
    try {
        await client.connect();
        console.log('✅ Connected to MongoDB');
        
        const db = client.db(DATABASE);
        const propertiesCollection = db.collection('properties');
        
        // Get all properties
        const properties = await propertiesCollection.find({}).toArray();
        console.log(`📊 Found ${properties.length} properties to fix`);
        
        let updatedCount = 0;
        
        for (const property of properties) {
            const updatedProperty = { ...property };
            
            // Fix PropertyStatus enum
            if (typeof property.status === 'string') {
                updatedProperty.status = PropertyStatus[property.status] || 0;
            }
            
            // Fix Cover MediaType enum
            if (property.cover && typeof property.cover.Type === 'string') {
                updatedProperty.cover.Type = MediaType[property.cover.Type] || 0;
            }
            
            // Fix Media MediaType enums
            if (property.media && Array.isArray(property.media)) {
                updatedProperty.media = property.media.map(media => ({
                    ...media,
                    Type: typeof media.Type === 'string' ? MediaType[media.Type] || 0 : media.Type
                }));
            }
            
            // Update the document
            await propertiesCollection.replaceOne({ _id: property._id }, updatedProperty);
            updatedCount++;
            
            if (updatedCount % 10 === 0) {
                console.log(`🔄 Fixed ${updatedCount}/${properties.length} properties`);
            }
        }
        
        console.log(`✅ Successfully fixed ${updatedCount} properties`);
        
        // Verify the fixes
        const sampleProperty = await propertiesCollection.findOne({});
        console.log('\n📋 Sample fixed property structure:');
        console.log('Status:', sampleProperty.status, typeof sampleProperty.status);
        console.log('Cover Type:', sampleProperty.cover.Type, typeof sampleProperty.cover.Type);
        console.log('Media Types:', sampleProperty.media.map(m => ({ id: m.Id, type: m.Type, typeType: typeof m.Type })));
        
    } catch (error) {
        console.error('❌ Error fixing enums:', error);
    } finally {
        await client.close();
        console.log('🔌 Disconnected from MongoDB');
    }
}

fixEnums().catch(console.error);
