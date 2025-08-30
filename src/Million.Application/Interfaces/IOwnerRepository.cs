using Million.Domain.Entities;

namespace Million.Application.Interfaces;

public interface IOwnerRepository
{
    Task<Owner?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Owner?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Owner?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<List<Owner>> GetAllAsync(CancellationToken ct = default);
    Task<List<Owner>> FindAsync(string? query = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<Owner> CreateAsync(Owner owner, CancellationToken ct = default);
    Task<Owner> UpdateAsync(Owner owner, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
}

