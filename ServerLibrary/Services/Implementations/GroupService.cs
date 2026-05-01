using BaseLibrary.DTOs.Groups;
using BaseLibrary.Helpers;
using BaseLibrary.Entities.Groups;
using ServerLibrary.Data;
using Microsoft.EntityFrameworkCore;

namespace ServerLibrary.Services.Implementations;

public interface IGroupService
{
    Task<ApiResponse<IEnumerable<GroupSummaryDto>>> GetAllGroupsAsync(string userId);
    Task<ApiResponse<GroupSummaryDto>> GetGroupByIdAsync(Guid id);
    Task<ApiResponse<GroupSummaryDto>> CreateGroupAsync(CreateGroupDto dto, string createdBy);
    Task<ApiResponse<GroupSummaryDto>> UpdateGroupAsync(UpdateGroupDto dto);
    Task<ApiResponse<bool>> DeleteGroupAsync(Guid id);
    Task<ApiResponse<bool>> JoinGroupAsync(Guid id, string userId);
    Task<ApiResponse<bool>> LeaveGroupAsync(Guid id, string userId);
    Task<ApiResponse<IEnumerable<DomainDto>>> GetDomainsAsync(Guid groupId);
}

public class GroupService : IGroupService
{
    private readonly ApplicationDbContext _db;
    public GroupService(ApplicationDbContext db) { _db = db; }

    public async Task<ApiResponse<IEnumerable<GroupSummaryDto>>> GetAllGroupsAsync(string userId)
    {
        var groups = await _db.Groups.Where(g => g.IsActive).ToListAsync();
        var result = new List<GroupSummaryDto>();

        foreach (var g in groups)
        {
            var memberCount = await _db.GroupMembers.CountAsync(m => m.GroupId == g.Id);
            var isJoined = await _db.GroupMembers.AnyAsync(m => m.GroupId == g.Id && m.UserId == userId);
            var domains = await _db.Domains.Where(d => d.GroupId == g.Id && d.IsActive)
                .Select(d => new DomainDto { Id = d.Id, Name = d.Name, Description = d.Description ?? "" }).ToListAsync();

            result.Add(new GroupSummaryDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description ?? "",
                IconUrl = g.IconUrl ?? "📂",
                MemberCount = memberCount,
                IsJoined = isJoined,
                Domains = domains
            });
        }

        return new ApiResponse<IEnumerable<GroupSummaryDto>> { Success = true, Data = result };
    }

    public async Task<ApiResponse<GroupSummaryDto>> GetGroupByIdAsync(Guid id)
    {
        var g = await _db.Groups.FindAsync(id);
        if (g == null) return new ApiResponse<GroupSummaryDto> { Success = false, Message = "Groupe introuvable." };

        var memberCount = await _db.GroupMembers.CountAsync(m => m.GroupId == g.Id);
        var domains = await _db.Domains.Where(d => d.GroupId == g.Id && d.IsActive)
            .Select(d => new DomainDto { Id = d.Id, Name = d.Name, Description = d.Description ?? "" }).ToListAsync();

        return new ApiResponse<GroupSummaryDto>
        {
            Success = true,
            Data = new GroupSummaryDto { Id = g.Id, Name = g.Name, Description = g.Description ?? "", IconUrl = g.IconUrl ?? "📂", MemberCount = memberCount, Domains = domains }
        };
    }

    public async Task<ApiResponse<GroupSummaryDto>> CreateGroupAsync(CreateGroupDto dto, string createdBy)
    {
        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            IconUrl = dto.IconUrl,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _db.Groups.Add(group);

        var domains = new List<DomainDto>();
        if (dto.Domains != null && dto.Domains.Any())
        {
            foreach (var d in dto.Domains)
            {
                var newDomain = new Domain
                {
                    Id = Guid.NewGuid(),
                    GroupId = group.Id,
                    Name = d.Name,
                    Description = d.Description,
                    IsActive = true
                };
                _db.Domains.Add(newDomain);
                domains.Add(new DomainDto { Id = newDomain.Id, Name = newDomain.Name, Description = newDomain.Description ?? "" });
            }
        }

        await _db.SaveChangesAsync();

        return new ApiResponse<GroupSummaryDto>
        {
            Success = true,
            Message = "Groupe créé avec succès.",
            Data = new GroupSummaryDto { Id = group.Id, Name = group.Name, Description = group.Description ?? "", IconUrl = group.IconUrl ?? "📂", MemberCount = 0, Domains = domains }
        };
    }

    public async Task<ApiResponse<GroupSummaryDto>> UpdateGroupAsync(UpdateGroupDto dto)
    {
        var group = await _db.Groups.FindAsync(dto.Id);
        if (group == null) return new ApiResponse<GroupSummaryDto> { Success = false, Message = "Groupe introuvable." };

        group.Name = dto.Name;
        group.Description = dto.Description;
        group.IconUrl = dto.IconUrl;
        group.IsActive = dto.IsActive;

        var existingDomains = await _db.Domains.Where(d => d.GroupId == group.Id).ToListAsync();
        
        // Deactivate domains missing from dto
        if (dto.Domains != null)
        {
            var updatedDomainIds = dto.Domains.Select(d => d.Id).ToList();
            foreach (var existing in existingDomains)
            {
                if (!updatedDomainIds.Contains(existing.Id))
                    existing.IsActive = false;
            }

            // Update existing or add new
            foreach (var d in dto.Domains)
            {
                var existing = existingDomains.FirstOrDefault(ed => ed.Id == d.Id);
                if (existing != null)
                {
                    existing.Name = d.Name;
                    existing.Description = d.Description;
                    existing.IsActive = true;
                }
                else
                {
                    _db.Domains.Add(new Domain
                    {
                        Id = Guid.NewGuid(),
                        GroupId = group.Id,
                        Name = d.Name,
                        Description = d.Description,
                        IsActive = true
                    });
                }
            }
        }

        _db.Groups.Update(group);
        await _db.SaveChangesAsync();

        var outputDomains = await _db.Domains.Where(d => d.GroupId == group.Id && d.IsActive)
            .Select(d => new DomainDto { Id = d.Id, Name = d.Name, Description = d.Description ?? "" }).ToListAsync();

        return new ApiResponse<GroupSummaryDto>
        {
            Success = true,
            Message = "Groupe modifié.",
            Data = new GroupSummaryDto { Id = group.Id, Name = group.Name, Description = group.Description ?? "", IconUrl = group.IconUrl ?? "📂", Domains = outputDomains }
        };
    }

    public async Task<ApiResponse<bool>> DeleteGroupAsync(Guid id)
    {
        var group = await _db.Groups.FindAsync(id);
        if (group == null) return new ApiResponse<bool> { Success = false, Message = "Groupe introuvable." };

        group.IsActive = false; // Soft delete
        _db.Groups.Update(group);
        await _db.SaveChangesAsync();

        return new ApiResponse<bool> { Success = true, Message = "Groupe supprimé.", Data = true };
    }

    public async Task<ApiResponse<bool>> JoinGroupAsync(Guid id, string userId)
    {
        var group = await _db.Groups.FindAsync(id);
        if (group == null || !group.IsActive) return new ApiResponse<bool> { Success = false, Message = "Groupe introuvable." };

        var existing = await _db.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == id && m.UserId == userId);
        if (existing == null)
        {
            _db.GroupMembers.Add(new GroupMember { Id = Guid.NewGuid(), GroupId = id, UserId = userId });
            await _db.SaveChangesAsync();
        }
        return new ApiResponse<bool> { Success = true, Message = "Groupe rejoint avec succès.", Data = true };
    }

    public async Task<ApiResponse<bool>> LeaveGroupAsync(Guid id, string userId)
    {
        var membership = await _db.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == id && m.UserId == userId);
        if (membership != null)
        {
            _db.GroupMembers.Remove(membership);
            await _db.SaveChangesAsync();
        }
        return new ApiResponse<bool> { Success = true, Message = "Groupe quitté.", Data = true };
    }

    public async Task<ApiResponse<IEnumerable<DomainDto>>> GetDomainsAsync(Guid groupId)
    {
        var domains = await _db.Domains
            .Where(d => d.GroupId == groupId && d.IsActive)
            .Select(d => new DomainDto { Id = d.Id, Name = d.Name, Description = d.Description ?? "" })
            .ToListAsync();
        return new ApiResponse<IEnumerable<DomainDto>> { Success = true, Data = domains };
    }
}
