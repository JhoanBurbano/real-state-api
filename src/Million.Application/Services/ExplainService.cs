using Million.Application.DTOs;

namespace Million.Application.Services;

public interface IExplainService
{
    Task<string> ExplainPropertyListingPipelineAsync(PropertyListQuery query, CancellationToken cancellationToken = default);
}
