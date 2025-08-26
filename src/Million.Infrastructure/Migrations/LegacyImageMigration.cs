using MongoDB.Driver;
using Million.Domain.Entities;
using Million.Infrastructure.Persistence;

namespace Million.Infrastructure.Migrations;

public static class LegacyImageMigration
{
    public static async Task MigrateLegacyImagesAsync(MongoContext context, CancellationToken cancellationToken = default)
    {
        var collection = context.Database.GetCollection<PropertyDocument>("properties");

        // Find properties with legacy image fields
        var filter = Builders<PropertyDocument>.Filter.Or(
            Builders<PropertyDocument>.Filter.Ne(x => x.CoverImage, ""),
            Builders<PropertyDocument>.Filter.SizeGt(x => x.Images, 0)
        );

        var properties = await collection.Find(filter).ToListAsync(cancellationToken);

        foreach (var property in properties)
        {
            var updates = new List<UpdateDefinition<PropertyDocument>>();

            // Migrate cover image
            if (!string.IsNullOrEmpty(property.CoverImage))
            {
                var cover = Cover.CreateImage(property.CoverImage);
                updates.Add(Builders<PropertyDocument>.Update.Set(x => x.Cover, CoverDocument.FromEntity(cover)));

                // Clear legacy field
                updates.Add(Builders<PropertyDocument>.Update.Set(x => x.CoverImage, ""));
            }

            // Migrate gallery images
            if (property.Images.Any())
            {
                var media = new List<Media>();
                var index = 1;

                foreach (var legacyImage in property.Images.Take(12)) // Limit to 12 featured
                {
                    if (!string.IsNullOrEmpty(legacyImage.Url))
                    {
                        media.Add(new Media
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = MediaType.Image,
                            Url = legacyImage.Url,
                            Index = index++,
                            Enabled = legacyImage.Enabled,
                            Featured = index <= 12 // First 12 are featured
                        });
                    }
                }

                if (media.Any())
                {
                    var mediaDocuments = media.Select(m => MediaDocument.FromEntity(m)).ToList();
                    updates.Add(Builders<PropertyDocument>.Update.Set(x => x.Media, mediaDocuments));
                }

                // Clear legacy field
                updates.Add(Builders<PropertyDocument>.Update.Set(x => x.Images, new List<LegacyImage>()));
            }

            if (updates.Any())
            {
                var combinedUpdate = Builders<PropertyDocument>.Update.Combine(updates);
                await collection.UpdateOneAsync(
                    x => x.Id == property.Id,
                    combinedUpdate,
                    cancellationToken: cancellationToken
                );
            }
        }
    }
}
