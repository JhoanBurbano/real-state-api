const { MongoClient } = require('mongodb');

const MONGO_URI = 'mongodb://localhost:27017';
const DATABASE = 'million';

async function cleanDatabase() {
    const client = new MongoClient(MONGO_URI);
    
    try {
        await client.connect();
        console.log('✅ Connected to MongoDB');
        
        const db = client.db(DATABASE);
        const propertiesCollection = db.collection('properties');
        
        // Get all properties
        const properties = await propertiesCollection.find({}).toArray();
        console.log(`📊 Found ${properties.length} properties to clean`);
        
        let updatedCount = 0;
        
        for (const property of properties) {
            const cleanedProperty = {
                _id: property._id,
                ownerId: property.ownerId,
                name: property.name,
                address: property.address,
                price: property.price,
                codeInternal: property.codeInternal,
                year: property.year,
                status: property.status,
                description: property.description,
                city: property.city,
                neighborhood: property.neighborhood,
                propertyType: property.propertyType,
                size: property.size,
                bedrooms: property.bedrooms,
                bathrooms: property.bathrooms,
                hasPool: property.hasPool,
                hasGarden: property.hasGarden,
                hasParking: property.hasParking,
                isFurnished: property.isFurnished,
                availableFrom: property.availableFrom,
                availableTo: property.availableTo,
                traces: property.traces || [],
                createdAt: property.createdAt,
                updatedAt: property.updatedAt,
                isActive: property.isActive
            };
            
            // Clean cover - keep only PascalCase fields
            if (property.cover) {
                cleanedProperty.cover = {
                    Type: property.cover.Type || 0, // MediaType.Image = 0
                    Url: property.cover.Url || property.cover.url || '',
                    Index: property.cover.Index || property.cover.index || 0,
                    Poster: property.cover.Poster || property.cover.poster || null
                };
            }
            
            // Clean media - keep only PascalCase fields
            if (property.media && Array.isArray(property.media)) {
                cleanedProperty.media = property.media.map(media => ({
                    Id: media.Id || media.id || '',
                    Type: media.Type || 0, // MediaType.Image = 0
                    Url: media.Url || media.url || '',
                    Index: media.Index || media.index || 0,
                    Enabled: media.Enabled !== undefined ? media.Enabled : (media.enabled !== undefined ? media.enabled : true),
                    Featured: media.Featured !== undefined ? media.Featured : (media.featured !== undefined ? media.featured : false),
                    Poster: media.Poster || media.poster || null,
                    Duration: media.Duration || media.duration || null,
                    Variants: media.Variants || null
                }));
            }
            
            // Remove legacy fields
            delete cleanedProperty.coverImage;
            delete cleanedProperty.images;
            
            // Update the document
            await propertiesCollection.replaceOne({ _id: property._id }, cleanedProperty);
            updatedCount++;
            
            if (updatedCount % 10 === 0) {
                console.log(`🔄 Cleaned ${updatedCount}/${properties.length} properties`);
            }
        }
        
        console.log(`✅ Successfully cleaned ${updatedCount} properties`);
        
        // Verify the cleaning
        const sampleProperty = await propertiesCollection.findOne({});
        console.log('\n📋 Sample cleaned property structure:');
        console.log(JSON.stringify(sampleProperty, null, 2));
        
    } catch (error) {
        console.error('❌ Error cleaning database:', error);
    } finally {
        await client.close();
        console.log('🔌 Disconnected from MongoDB');
    }
}

cleanDatabase().catch(console.error);
