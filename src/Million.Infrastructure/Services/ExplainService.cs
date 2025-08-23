using MongoDB.Bson;
using MongoDB.Driver;
using Million.Application.DTOs;
using Million.Application.Services;
using Million.Infrastructure.Persistence;

namespace Million.Infrastructure.Services;

public class ExplainService : IExplainService
{
    private readonly IMongoCollection<PropertyDocument> _collection;

    public ExplainService(MongoContext context)
    {
        _collection = context.GetCollection<PropertyDocument>("properties");
    }

    public async Task<string> ExplainPropertyListingPipelineAsync(PropertyListQuery query, CancellationToken cancellationToken = default)
    {
        var filters = new List<FilterDefinition<PropertyDocument>>();
        var builder = Builders<PropertyDocument>.Filter;

        // Build the same filters as the repository
        if (query.MinPrice.HasValue)
            filters.Add(builder.Gte(x => x.Price, query.MinPrice.Value));

        if (query.MaxPrice.HasValue)
            filters.Add(builder.Lte(x => x.Price, query.MaxPrice.Value));

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchFilter = builder.Or(
                builder.Regex(x => x.Name, new BsonRegularExpression(query.Search, "i")),
                builder.Regex(x => x.Description, new BsonRegularExpression(query.Search, "i")),
                builder.Regex(x => x.Address, new BsonRegularExpression(query.Search, "i")),
                builder.Regex(x => x.City, new BsonRegularExpression(query.Search, "i")),
                builder.Regex(x => x.Neighborhood, new BsonRegularExpression(query.Search, "i"))
            );
            filters.Add(searchFilter);
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var locationFilter = builder.Or(
                builder.Regex(x => x.City, new BsonRegularExpression(query.Location, "i")),
                builder.Regex(x => x.Neighborhood, new BsonRegularExpression(query.Location, "i"))
            );
            filters.Add(locationFilter);
        }

        if (!string.IsNullOrWhiteSpace(query.PropertyType))
            filters.Add(builder.Regex(x => x.PropertyType, new BsonRegularExpression(query.PropertyType, "i")));

        if (query.Bedrooms.HasValue)
            filters.Add(builder.Eq(x => x.Bedrooms, query.Bedrooms.Value));

        if (query.Bathrooms.HasValue)
            filters.Add(builder.Eq(x => x.Bathrooms, query.Bedrooms.Value));

        if (query.MinSize.HasValue)
            filters.Add(builder.Gte(x => x.Size, query.MinSize.Value));

        if (query.MaxSize.HasValue)
            filters.Add(builder.Lte(x => x.Size, query.MaxSize.Value));

        if (query.HasPool.HasValue)
            filters.Add(builder.Eq(x => x.HasPool, query.HasPool.Value));

        if (query.HasGarden.HasValue)
            filters.Add(builder.Eq(x => x.HasGarden, query.HasGarden.Value));

        if (query.HasParking.HasValue)
            filters.Add(builder.Eq(x => x.HasParking, query.HasParking.Value));

        if (query.IsFurnished.HasValue)
            filters.Add(builder.Eq(x => x.IsFurnished, query.IsFurnished.Value));

        if (!string.IsNullOrWhiteSpace(query.IdOwner))
            filters.Add(builder.Eq(x => x.OwnerId, query.IdOwner));

        if (query.AvailableFrom.HasValue)
            filters.Add(builder.Lte(x => x.AvailableFrom, query.AvailableFrom.Value));

        if (query.AvailableTo.HasValue)
            filters.Add(builder.Gte(x => x.AvailableTo, query.AvailableTo.Value));

        filters.Add(builder.Eq(x => x.IsActive, true));

        var filter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

        // Build the aggregation pipeline
        var pipeline = new List<BsonDocument>
        {
            new BsonDocument("$match", filter.ToBsonDocument()),
            new BsonDocument("$project", new BsonDocument
            {
                { "name", 1 },
                { "address", 1 },
                { "price", 1 },
                { "year", 1 },
                { "codeInternal", 1 },
                { "ownerId", 1 },
                { "status", 1 },
                { "coverUrl", new BsonDocument("$ifNull", new BsonArray { "$cover.url", "$cover.poster" }) },
                { "totalImages", new BsonDocument("$size", new BsonDocument("$filter", new BsonDocument
                {
                    { "input", "$media" },
                    { "as", "m" },
                    { "cond", new BsonDocument("$eq", new BsonArray { "$$m.type", 0 }) }
                })) },
                { "totalVideos", new BsonDocument("$size", new BsonDocument("$filter", new BsonDocument
                {
                    { "input", "$media" },
                    { "as", "m" },
                    { "cond", new BsonDocument("$eq", new BsonArray { "$$m.type", 1 }) }
                })) }
            }),
            new BsonDocument("$addFields", new BsonDocument
            {
                { "hasMoreMedia", new BsonDocument("$gt", new BsonArray { "$totalImages", 12 }) }
            }),
            new BsonDocument("$sort", new BsonDocument("price", -1)),
            new BsonDocument("$facet", new BsonDocument
            {
                { "items", new BsonArray
                {
                    new BsonDocument("$skip", (query.Page - 1) * query.PageSize),
                    new BsonDocument("$limit", query.PageSize)
                }},
                { "total", new BsonArray { new BsonDocument("$count", "count") } }
            })
        };

        // Execute explain on the pipeline
        var explainPipeline = new List<BsonDocument>
        {
            new BsonDocument("$explain", new BsonDocument("verbose", true))
        };
        explainPipeline.AddRange(pipeline);

        var explainResult = await _collection.Aggregate<BsonDocument>(explainPipeline).ToListAsync(cancellationToken);

        // Format the explain output
        var explain = explainResult.FirstOrDefault();
        if (explain == null) return "No explain data available";

        var formattedExplain = FormatExplainOutput(explain, query);
        return formattedExplain;
    }

    private static string FormatExplainOutput(BsonDocument explain, PropertyListQuery query)
    {
        var output = new System.Text.StringBuilder();
        output.AppendLine("=== MongoDB Query Execution Plan ===");
        output.AppendLine($"Query: {query.Search ?? "No search"} | Location: {query.Location ?? "Any"} | Price: {query.MinPrice?.ToString() ?? "0"} - {query.MaxPrice?.ToString() ?? "∞"}");
        output.AppendLine($"Page: {query.Page}, PageSize: {query.PageSize}, Sort: {query.Sort ?? "-price"}");
        output.AppendLine();

        if (explain.Contains("queryPlanner"))
        {
            var queryPlanner = explain["queryPlanner"].AsBsonDocument;
            output.AppendLine("=== Query Planner ===");

            if (queryPlanner.Contains("winningPlan"))
            {
                var winningPlan = queryPlanner["winningPlan"].AsBsonDocument;
                output.AppendLine($"Strategy: {winningPlan.GetValue("stage", "unknown")}");

                if (winningPlan.Contains("inputStage"))
                {
                    var inputStage = winningPlan["inputStage"].AsBsonDocument;
                    output.AppendLine($"Input Stage: {inputStage.GetValue("stage", "unknown")}");

                    if (inputStage.Contains("indexName"))
                    {
                        output.AppendLine($"Index Used: {inputStage["indexName"]}");
                    }
                }
            }

            if (queryPlanner.Contains("rejectedPlans"))
            {
                output.AppendLine($"Rejected Plans: {queryPlanner["rejectedPlans"].AsBsonArray.Count}");
            }
        }

        if (explain.Contains("executionStats"))
        {
            var executionStats = explain["executionStats"].AsBsonDocument;
            output.AppendLine();
            output.AppendLine("=== Execution Stats ===");
            output.AppendLine($"Total Documents: {executionStats.GetValue("totalDocsExamined", 0)}");
            output.AppendLine($"Execution Time: {executionStats.GetValue("executionTimeMillis", 0)}ms");

            if (executionStats.Contains("totalKeysExamined"))
            {
                output.AppendLine($"Keys Examined: {executionStats["totalKeysExamined"]}");
            }
        }

        return output.ToString();
    }
}
