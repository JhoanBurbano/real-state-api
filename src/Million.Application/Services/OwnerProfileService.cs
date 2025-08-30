using Million.Application.DTOs;
using Million.Domain.Entities;
using Million.Application.Interfaces;
using Million.Application.Extensions;

namespace Million.Application.Services;

public interface IOwnerProfileService
{
    Task<OwnerProfileDto?> GetProfileBySlugAsync(string slug, CancellationToken ct = default);
    Task<OwnerProfileDto?> GetProfileByIdAsync(string id, CancellationToken ct = default);
    Task<List<OwnerProfileDto>> GetAllProfilesAsync(CancellationToken ct = default);
}

public class OwnerProfileService : IOwnerProfileService
{
    private readonly IOwnerRepository _ownerRepository;

    public OwnerProfileService(IOwnerRepository ownerRepository)
    {
        _ownerRepository = ownerRepository;
    }

    public async Task<OwnerProfileDto?> GetProfileBySlugAsync(string slug, CancellationToken ct = default)
    {
        var owner = await _ownerRepository.GetBySlugAsync(slug, ct);
        return owner?.ToProfileDto();
    }

    public async Task<OwnerProfileDto?> GetProfileByIdAsync(string id, CancellationToken ct = default)
    {
        var owner = await _ownerRepository.GetByIdAsync(id, ct);
        return owner?.ToProfileDto();
    }

    public async Task<List<OwnerProfileDto>> GetAllProfilesAsync(CancellationToken ct = default)
    {
        var owners = await _ownerRepository.GetAllAsync(ct);
        return owners.Select(o => o.ToProfileDto()).ToList();
    }
}
