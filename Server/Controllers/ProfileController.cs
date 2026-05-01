using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using BaseLibrary.DTOs;
using BaseLibrary.DTOs.Posts;
using BaseLibrary.Helpers;
using System.Security.Claims;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ExternalDbContext _extDb;

    public ProfileController(ApplicationDbContext db, ExternalDbContext extDb)
    {
        _db = db;
        _extDb = extDb;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetProfile(string userId)
    {
        var employee = await _extDb.Employees.FirstOrDefaultAsync(e => e.IdUser == userId);
        if (employee == null)
            return Ok(new ApiResponse<UserProfileDto> { Success = false, Message = "Utilisateur introuvable" });

        var structure = await _extDb.Structures.FirstOrDefaultAsync(s => s.IdStructure == employee.IdStructure);
        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.IdUser == userId);

        var allPosts = await _db.Posts.Where(p => p.AuthorId == userId && p.Status == BaseLibrary.Helpers.PostStatus.ACTIVE)
            .OrderByDescending(p => p.CreatedAt).ToListAsync();

        var postTypes = await _db.PostTypes.ToListAsync();
        var postTypeLookup = postTypes.ToDictionary(pt => pt.Id, pt => pt.Code);

        var recentPosts = new List<PostSummaryDto>();
        foreach (var p in allPosts.Take(20))
        {
            var typeCode = postTypeLookup.GetValueOrDefault(p.PostTypeId, "UNKNOWN");
            var hashtags = await _db.PostHashtags.Where(h => h.PostId == p.Id).Select(h => h.Tag).ToListAsync();
            var commentsCount = await _db.Comments.CountAsync(c => c.PostId == p.Id);

            recentPosts.Add(new PostSummaryDto
            {
                Id = p.Id,
                TypeCode = typeCode,
                Title = p.Title,
                AuthorId = p.AuthorId,
                AuthorName = $"{employee.Prenom} {employee.Nom}",
                AuthorStructure = structure?.StructureName ?? "",
                CreatedAt = p.CreatedAt,
                ViewsCount = p.ViewsCount,
                CommentsCount = commentsCount,
                Hashtags = hashtags
            });
        }

        var solutionTypeId = postTypes.FirstOrDefault(pt => pt.Code == "SOLUTION")?.Id ?? Guid.Empty;
        var problemTypeId = postTypes.FirstOrDefault(pt => pt.Code == "PROBLEM")?.Id ?? Guid.Empty;

        var dto = new UserProfileDto
        {
            UserId = userId,
            FullName = $"{employee.Prenom} {employee.Nom}",
            Grade = employee.Grade,
            Structure = structure?.StructureName ?? "",
            Bio = profile?.Bio,
            Telephone = profile?.Telephone,
            AvatarUrl = profile?.AvatarUrl,
            TotalPosts = allPosts.Count,
            TotalSolutions = allPosts.Count(p => p.PostTypeId == solutionTypeId),
            TotalProblems = allPosts.Count(p => p.PostTypeId == problemTypeId),
            TotalViews = allPosts.Sum(p => p.ViewsCount),
            MemberSince = allPosts.Any() ? allPosts.Min(p => p.CreatedAt) : DateTime.UtcNow,
            RecentPosts = recentPosts
        };

        return Ok(new ApiResponse<UserProfileDto> { Success = true, Data = dto });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new ApiResponse<bool> { Success = false, Message = "Non autorisé" });

        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.IdUser == userId);
        if (profile == null)
        {
            profile = new BaseLibrary.Entities.Auth.UserProfile { IdUser = userId, IsActive = true };
            _db.UserProfiles.Add(profile);
        }

        profile.Telephone = dto.Telephone;
        profile.Bio = dto.Bio;

        await _db.SaveChangesAsync();

        return Ok(new ApiResponse<bool> { Success = true, Message = "Profil mis à jour", Data = true });
    }
}
