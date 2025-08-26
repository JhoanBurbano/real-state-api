const { MongoClient } = require('mongodb');

// MongoDB connection string
const uri = process.env.MONGO_URI || 'mongodb://localhost:27017';
const dbName = process.env.MONGO_DB || 'million';

// Sample descriptions for different property types
const propertyDescriptions = {
    'Villa': [
        'Luxurious villa with stunning ocean views and modern amenities. Features spacious rooms, high-end finishes, and a private pool.',
        'Elegant villa offering the perfect blend of luxury and comfort. Includes premium appliances and beautiful landscaping.',
        'Sophisticated villa with contemporary design and premium materials. Perfect for families seeking elegance and space.',
        'Exclusive villa with panoramic views and exceptional craftsmanship. Features custom details and luxury finishes.',
        'Premium villa with resort-style amenities and breathtaking views. Includes smart home technology and designer touches.'
    ],
    'Apartment': [
        'Modern apartment with sleek design and city views. Features open floor plan and premium finishes.',
        'Contemporary apartment with luxury amenities and convenient location. Perfect for urban professionals.',
        'Stylish apartment with high-end appliances and beautiful views. Includes fitness center and concierge service.',
        'Elegant apartment with modern architecture and premium materials. Features smart home technology.',
        'Luxury apartment with panoramic city views and exceptional amenities. Perfect for sophisticated living.'
    ],
    'House': [
        'Beautiful family home with spacious rooms and modern updates. Features large backyard and garage.',
        'Charming house with character and modern conveniences. Perfect for growing families.',
        'Stunning house with premium finishes and excellent location. Includes updated kitchen and bathrooms.',
        'Elegant house with classic architecture and contemporary amenities. Features beautiful landscaping.',
        'Premium house with luxury details and exceptional craftsmanship. Perfect for discerning buyers.'
    ],
    'Penthouse': [
        'Exclusive penthouse with breathtaking city views and luxury finishes. Features private terrace and premium amenities.',
        'Sophisticated penthouse with contemporary design and exceptional views. Includes smart home technology.',
        'Luxury penthouse with panoramic vistas and premium materials. Features custom details and designer touches.',
        'Premium penthouse with stunning architecture and city skyline views. Perfect for luxury living.',
        'Exclusive penthouse with world-class amenities and exceptional views. Features private elevator and concierge.'
    ],
    'Condo': [
        'Modern condo with sleek design and city convenience. Features fitness center and community amenities.',
        'Contemporary condo with luxury finishes and excellent location. Perfect for urban professionals.',
        'Stylish condo with high-end appliances and beautiful views. Includes parking and storage.',
        'Elegant condo with modern architecture and premium materials. Features community pool and gym.',
        'Luxury condo with panoramic views and exceptional amenities. Perfect for sophisticated living.'
    ]
};

// Default descriptions for unknown property types
const defaultDescriptions = [
    'Beautiful property with modern amenities and excellent location. Features premium finishes and quality construction.',
    'Stunning property offering luxury living with contemporary design. Includes high-end appliances and beautiful views.',
    'Elegant property with sophisticated design and premium materials. Perfect for discerning buyers.',
    'Premium property with exceptional craftsmanship and luxury details. Features smart home technology.',
    'Exclusive property with world-class amenities and stunning design. Perfect for luxury living.'
];

async function addDescriptionsToProperties() {
    const client = new MongoClient(uri);
    
    try {
        await client.connect();
        console.log('✅ Connected to MongoDB');
        
        const db = client.db(dbName);
        const propertiesCollection = db.collection('properties');
        
        // Get all properties without descriptions
        const properties = await propertiesCollection.find({
            $or: [
                { description: { $exists: false } },
                { description: "" },
                { description: null }
            ]
        }).toArray();
        
        console.log(`📊 Found ${properties.length} properties without descriptions`);
        
        if (properties.length === 0) {
            console.log('✅ All properties already have descriptions');
            return;
        }
        
        let updatedCount = 0;
        
        for (const property of properties) {
            const propertyType = property.propertyType || property.PropertyType || 'House';
            const descriptions = propertyDescriptions[propertyType] || defaultDescriptions;
            
            // Select a random description
            const randomDescription = descriptions[Math.floor(Math.random() * descriptions.length)];
            
            // Update the property with description
            const result = await propertiesCollection.updateOne(
                { _id: property._id },
                { 
                    $set: { 
                        description: randomDescription,
                        Description: randomDescription // Also update PascalCase version
                    } 
                }
            );
            
            if (result.modifiedCount > 0) {
                updatedCount++;
                console.log(`✅ Updated property ${property._id || property.Id}: ${property.name || property.Name}`);
            }
        }
        
        console.log(`🎉 Successfully updated ${updatedCount} properties with descriptions`);
        
        // Verify the updates
        const propertiesWithDescriptions = await propertiesCollection.find({
            description: { $exists: true, $ne: "" }
        }).count();
        
        const totalProperties = await propertiesCollection.countDocuments();
        console.log(`📊 Total properties: ${totalProperties}`);
        console.log(`📊 Properties with descriptions: ${propertiesWithDescriptions}`);
        
    } catch (error) {
        console.error('❌ Error:', error);
    } finally {
        await client.close();
        console.log('🔌 Disconnected from MongoDB');
    }
}

// Run the script
addDescriptionsToProperties().catch(console.error);
