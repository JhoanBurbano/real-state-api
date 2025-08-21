using Million.Application.DTOs;
using Million.Domain.Entities;

namespace Million.Application.Interfaces;

public interface IPropertyRepository
{
    Task<(IReadOnlyList<Property> Items, long Total)> FindAsync(PropertyListQuery query, CancellationToken cancellationToken);
    Task<Property?> GetByIdAsync(string id, CancellationToken cancellationToken);
}

