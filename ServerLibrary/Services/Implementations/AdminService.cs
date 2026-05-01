using BaseLibrary.DTOs;
using BaseLibrary.Helpers;
using ServerLibrary.Data;
using Microsoft.EntityFrameworkCore;

namespace ServerLibrary.Services.Implementations;

public interface IAdminService
{
    Task<ApiResponse<AdminDashboardDto>> GetDashboardAsync();
    Task<ApiResponse<PaginatedResponse<UserRankDto>>> GetReportersAsync(int page = 1, int pageSize = 10);
}

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _db;
    public AdminService(ApplicationDbContext db) { _db = db; }

    public async Task<ApiResponse<AdminDashboardDto>> GetDashboardAsync()
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var result = new AdminDashboardDto();

        // ── Global Stats ─────────────────────────────────────────────
        result.TotalPosts    = await _db.Posts.CountAsync();
        result.TotalUsers    = await _db.UserProfiles.CountAsync();
        result.TotalReports  = await _db.PostReports.CountAsync();
        result.TotalComments = await _db.Comments.CountAsync();

        // ── Top 5 Rating (avg score par auteur) ──────────────────────
        var postsByAuthor = await _db.Posts
            .Where(p => !p.IsAnonymous)
            .Select(p => new { p.Id, p.AuthorId })
            .ToListAsync();

        var ratings = await _db.PostRatings.ToListAsync();

        var ratingByAuthor = postsByAuthor
            .GroupJoin(ratings, p => p.Id, r => r.PostId, (p, rs) => new { p.AuthorId, Scores = rs.Select(r => r.Score) })
            .GroupBy(x => x.AuthorId)
            .Select(g => new UserRankDto
            {
                UserId = g.Key,
                Score = g.SelectMany(x => x.Scores).Any() ? Math.Round(g.SelectMany(x => x.Scores).Average(), 2) : 0,
                PostCount = g.Count()
            })
            .Where(u => u.Score > 0)
            .OrderByDescending(u => u.Score)
            .Take(5)
            .ToList();

        result.TopRatingUsers = ratingByAuthor;

        // ── Bad Ratings (avg < 2) ─────────────────────────────────────
        result.BadRatingUsers = postsByAuthor
            .GroupJoin(ratings, p => p.Id, r => r.PostId, (p, rs) => new { p.AuthorId, Scores = rs.Select(r => r.Score) })
            .GroupBy(x => x.AuthorId)
            .Select(g => new UserRankDto
            {
                UserId = g.Key,
                Score = g.SelectMany(x => x.Scores).Any() ? Math.Round(g.SelectMany(x => x.Scores).Average(), 2) : 0,
                PostCount = g.Count()
            })
            .Where(u => u.Score > 0 && u.Score < 2)
            .OrderBy(u => u.Score)
            .Take(5)
            .ToList();

        // ── Top 5 Flèches (vote net positif sur commentaires) ─────────
        var votes = await _db.CommentVotes.ToListAsync();
        var comments = await _db.Comments.Select(c => new { c.Id, c.AuthorId }).ToListAsync();

        var voteByAuthor = comments
            .GroupJoin(votes, c => c.Id, v => v.CommentId, (c, vs) => new { c.AuthorId, Balance = vs.Sum(v => (int)v.VoteType) })
            .GroupBy(x => x.AuthorId)
            .Select(g => new UserRankDto
            {
                UserId = g.Key,
                VoteBalance = g.Sum(x => x.Balance),
                CommentCount = g.Count()
            })
            .OrderByDescending(u => u.VoteBalance)
            .ToList();

        result.TopVoteUsers = voteByAuthor.Where(u => u.VoteBalance > 0).Take(5).ToList();
        result.NegativeVoteUsers = voteByAuthor.Where(u => u.VoteBalance < 0).OrderBy(u => u.VoteBalance).Take(5).ToList();

        // ── Top Solution Authors ───────────────────────────────────────
        var postTypes = await _db.PostTypes.ToDictionaryAsync(t => t.Id, t => t.Code);

        var allPosts = await _db.Posts
            .Where(p => !p.IsAnonymous && p.Status == PostStatus.ACTIVE)
            .ToListAsync();

        result.TopSolutionAuthors = allPosts
            .Where(p => postTypes.TryGetValue(p.PostTypeId, out var code) && code == "SOLUTION")
            .GroupBy(p => p.AuthorId)
            .Select(g => new UserRankDto { UserId = g.Key, PostCount = g.Count() })
            .OrderByDescending(u => u.PostCount)
            .Take(5)
            .ToList();

        // ── Top Problem Authors ───────────────────────────────────────
        result.TopProblemAuthors = allPosts
            .Where(p => postTypes.TryGetValue(p.PostTypeId, out var code) && code == "PROBLEM")
            .GroupBy(p => p.AuthorId)
            .Select(g => new UserRankDto { UserId = g.Key, PostCount = g.Count() })
            .OrderByDescending(u => u.PostCount)
            .Take(5)
            .ToList();

        // ── Trending Domains (30 derniers jours) ─────────────────────
        var recentPosts = allPosts.Where(p => p.CreatedAt >= thirtyDaysAgo && p.DomainId.HasValue).ToList();
        var domains = await _db.Domains.ToListAsync();
        var groups  = await _db.Groups.ToDictionaryAsync(g => g.Id, g => g.Name);

        result.TrendingDomains = recentPosts
            .GroupBy(p => p.DomainId!.Value)
            .Select(g =>
            {
                var domain = domains.FirstOrDefault(d => d.Id == g.Key);
                return new DomainTrendDto
                {
                    DomainId   = g.Key,
                    DomainName = domain?.Name ?? "Inconnu",
                    GroupName  = domain != null && groups.TryGetValue(domain.GroupId, out var gn) ? gn : "",
                    PostCount  = g.Count()
                };
            })
            .OrderByDescending(d => d.PostCount)
            .Take(8)
            .ToList();

        // ── Trending Problems (par vues) ─────────────────────────────
        result.TrendingProblems = allPosts
            .Where(p => postTypes.TryGetValue(p.PostTypeId, out var code) && code == "PROBLEM")
            .OrderByDescending(p => p.ViewsCount)
            .Take(8)
            .Select(p => new PostTrendDto
            {
                PostId   = p.Id,
                Title    = p.Title,
                AuthorId = p.IsAnonymous ? "Anonyme" : p.AuthorId,
                TypeCode = postTypes.TryGetValue(p.PostTypeId, out var c) ? c : "UNKNOWN",
                ViewsCount = p.ViewsCount,
                AverageRating = ratings.Where(r => r.PostId == p.Id).Select(r => r.Score).DefaultIfEmpty(0).Average()
            })
            .ToList();

        // ── Top 10 Reporters ─────────────────────────────────────────
        var reports = await _db.PostReports.ToListAsync();
        result.TopReporters = reports
            .GroupBy(r => r.UserId)
            .Select(g => new UserRankDto { UserId = g.Key, ReportCount = g.Count() })
            .OrderByDescending(u => u.ReportCount)
            .Take(10)
            .ToList();

        return new ApiResponse<AdminDashboardDto> { Success = true, Data = result };
    }

    public async Task<ApiResponse<PaginatedResponse<UserRankDto>>> GetReportersAsync(int page = 1, int pageSize = 10)
    {
        var reports = await _db.PostReports.ToListAsync();

        var all = reports
            .GroupBy(r => r.UserId)
            .Select(g => new UserRankDto { UserId = g.Key, ReportCount = g.Count() })
            .OrderByDescending(u => u.ReportCount)
            .ToList();

        var total = all.Count;
        var paged = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new ApiResponse<PaginatedResponse<UserRankDto>>
        {
            Success = true,
            Data = new PaginatedResponse<UserRankDto>
            {
                Data = paged,
                Total = total,
                Page = page,
                PageSize = pageSize
            }
        };
    }
}
