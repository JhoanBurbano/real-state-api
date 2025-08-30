#!/usr/bin/env node

/**
 * Debug script to check property traces in MongoDB Atlas
 */

const { MongoClient } = require('mongodb');
const path = require('path');

// Load .env file from project root
require('dotenv').config({ path: path.join(__dirname, '..', '.env') });

const MONGODB_URI = process.env.MONGO_URI || "mongodb://localhost:27017";

async function debugTraces() {
  const client = new MongoClient(MONGODB_URI);
  
  try {
    console.log('🔌 Connecting to MongoDB Atlas...');
    await client.connect();
    console.log('✅ Connected successfully');
    
    const db = client.db('million');
    
    // Check if property exists
    console.log('\n🔍 Checking property prop-003...');
    const property = await db.collection('properties').findOne({ _id: "prop-003" });
    if (property) {
      console.log(`✅ Property found: ${property.name}`);
      console.log(`   Owner: ${property.ownerId}`);
      console.log(`   Status: ${property.status}`);
    } else {
      console.log('❌ Property prop-003 not found');
    }
    
    // Check traces for this property
    console.log('\n🔍 Checking traces for prop-003...');
    const traces = await db.collection('property_traces').find({ propertyId: "prop-003" }).toArray();
    console.log(`📊 Found ${traces.length} traces for prop-003`);
    
    if (traces.length > 0) {
      console.log('\n📋 Sample traces:');
      traces.slice(0, 3).forEach((trace, index) => {
        console.log(`  ${index + 1}. Action: ${trace.action}, Timestamp: ${trace.timestamp}, Notes: ${trace.notes}`);
      });
    }
    
    // Check all traces collection
    console.log('\n🔍 Checking all traces collection...');
    const allTraces = await db.collection('property_traces').find({}).toArray();
    console.log(`📊 Total traces in collection: ${allTraces.length}`);
    
    if (allTraces.length > 0) {
      console.log('\n📋 Sample traces from collection:');
      allTraces.slice(0, 3).forEach((trace, index) => {
        console.log(`  ${index + 1}. PropertyId: ${trace.propertyId}, Action: ${trace.action}, Timestamp: ${trace.timestamp}`);
      });
    }
    
    // Check collection names
    console.log('\n🔍 Available collections:');
    const collections = await db.listCollections().toArray();
    collections.forEach(col => {
      console.log(`  - ${col.name}`);
    });
    
  } catch (error) {
    console.error('❌ Error:', error);
  } finally {
    await client.close();
    console.log('\n🔌 Connection closed');
  }
}

debugTraces().catch(console.error);
