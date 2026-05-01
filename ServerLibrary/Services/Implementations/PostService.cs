using BaseLibrary.DTOs.Posts;
using BaseLibrary.Helpers;
using BaseLibrary.Entities.Posts;
using ServerLibrary.Data;
using Microsoft.EntityFrameworkCore;

namespace ServerLibrary.Services.Implementations;

public interface IPostService
{
    Task<ApiResponse<PaginatedResponse<PostSummaryDto>>> GetPostsFeedAsync(string userId, string? groupId = null, int page = 1, int pageSize = 10);
    Task<ApiResponse<PostDetailDto>> GetPostDetailAsync(Guid id);
    Task<ApiResponse<PostDetailDto>> CreatePostAsync(CreatePostDto dto, string authorId, string authorName, string authorStructure);
    Task<ApiResponse<PostDetailDto>> UpdatePostAsync(Guid id, CreatePostDto dto, string userId, string role);
    Task<ApiResponse<bool>> DeletePostAsync(Guid id, string userId, string role);
    Task<ApiResponse<bool>> TogglePinPostAsync(Guid id, string role);
    Task<ApiResponse<bool>> SubmitInteractionAsync(Guid postId, string userId, int selectedIndex);
    Task<ApiResponse<double>> RatePostAsync(Guid postId, string userId, int rating);
    // Report
    Task<ApiResponse<List<ReportReasonDto>>> GetReportReasonsAsync();
    Task<ApiResponse<ReportReasonDto>> CreateReportReasonAsync(CreateReportReasonDto dto);
    Task<ApiResponse<bool>> DeleteReportReasonAsync(Guid id);
    Task<ApiResponse<bool>> SubmitReportAsync(Guid postId, string userId, SubmitReportDto dto);
    Task<ApiResponse<PaginatedResponse<PostSummaryDto>>> SearchPostsAsync(SearchFilterDto filter, string userId);
}

public class PostService : IPostService
{
    private readonly ApplicationDbContext _db;
    public PostService(ApplicationDbContext db) { _db = db; }

    public async Task<ApiResponse<PaginatedResponse<PostSummaryDto>>> GetPostsFeedAsync(string userId, string? groupId = null, int page = 1, int pageSize = 10)
    {
        // Types ouverts à tous (définis par un rôle d'admin/contenu requis à la création)
        var globalTypeIds = await _db.PostTypes.Where(pt => pt.RequiresPrivilege).Select(pt => pt.Id).ToListAsync();
        
        var userGroupIds = await _db.GroupMembers.Where(m => m.UserId == userId).Select(m => m.GroupId).ToListAsync();

        var query = _db.Posts.Where(p => 
            p.Status == PostStatus.ACTIVE && 
            (globalTypeIds.Contains(p.PostTypeId) || userGroupIds.Contains(p.GroupId))
        ).OrderByDescending(p => p.IsPinned).ThenByDescending(p => p.CreatedAt).AsQueryable();

        if (!string.IsNullOrEmpty(groupId) && Guid.TryParse(groupId, out var gid))
            query = query.Where(p => p.GroupId == gid);

        var totalCount = await query.CountAsync();
        var posts = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var result = new List<PostSummaryDto>();

        foreach (var p in posts)
        {
            var postType = await _db.PostTypes.FindAsync(p.PostTypeId);
            var hashtags = await _db.PostHashtags.Where(h => h.PostId == p.Id).Select(h => h.Tag).ToListAsync();
            var commentsCount = await _db.Comments.CountAsync(c => c.PostId == p.Id);
            
            var postRatings = await _db.PostRatings.Where(r => r.PostId == p.Id).ToListAsync();
            var avgRating = postRatings.Any() ? postRatings.Average(r => r.Score) : 0;
            var userRating = postRatings.FirstOrDefault(r => r.UserId == userId)?.Score ?? 0;

            result.Add(new PostSummaryDto
            {
                Id = p.Id,
                GroupId = p.GroupId,
                DomainId = p.DomainId,
                TypeCode = postType?.Code ?? "UNKNOWN",
                Title = p.Title,
                AuthorId = p.IsAnonymous ? "" : p.AuthorId,
                AuthorName = p.IsAnonymous ? "Anonyme" : p.AuthorId,
                AuthorStructure = "",
                CreatedAt = p.CreatedAt,
                ViewsCount = p.ViewsCount,
                Hashtags = hashtags,
                IsAnonymous = p.IsAnonymous,
                IsPinned = p.IsPinned,
                AverageRating = avgRating,
                UserRating = userRating
            });

        }

        return new ApiResponse<PaginatedResponse<PostSummaryDto>>
        {
            Success = true,
            Data = new PaginatedResponse<PostSummaryDto> 
            { 
                Data = result, 
                Total = totalCount, 
                Page = page, 
                PageSize = pageSize 
            }
        };
    }

    public async Task<ApiResponse<PostDetailDto>> GetPostDetailAsync(Guid id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return new ApiResponse<PostDetailDto> { Success = false, Message = "Poste introuvable." };

        var postType = await _db.PostTypes.FindAsync(post.PostTypeId);
        var sections = await _db.PostSections.Where(s => s.PostId == id).OrderBy(s => s.OrderIndex).ToListAsync();
        var hashtags = await _db.PostHashtags.Where(h => h.PostId == id).Select(h => h.Tag).ToListAsync();
        var commentsCount = await _db.Comments.CountAsync(c => c.PostId == id);

        var sectionDict = new Dictionary<string, string>();
        foreach (var s in sections)
            sectionDict[s.SectionType.ToString()] = s.Content;

        // Fetch Interaction data
        var dbVotes = await _db.SurveyVotes.Where(v => v.PostId == id).ToListAsync();
        var counts = new Dictionary<int, int>();

        foreach (var vote in dbVotes)
        {
            var strId = vote.OptionId.ToString();
            if (int.TryParse(strId.Substring(24), out int idx))
            {
                if (!counts.ContainsKey(idx)) counts[idx] = 0;
                counts[idx]++;
            }
        }

        var postRatings = await _db.PostRatings.Where(r => r.PostId == id).ToListAsync();
        var avgRating = postRatings.Any() ? postRatings.Average(r => r.Score) : 0;

        return new ApiResponse<PostDetailDto>
        {
            Success = true,
            Data = new PostDetailDto
            {
                Id = post.Id,
                GroupId = post.GroupId,
                DomainId = post.DomainId,
                TypeCode = postType?.Code ?? "UNKNOWN",
                Title = post.Title,
                AuthorId = post.IsAnonymous ? "" : post.AuthorId,
                AuthorName = post.IsAnonymous ? "Anonyme" : post.AuthorId,
                AuthorStructure = "",
                CreatedAt = post.CreatedAt,
                ViewsCount = post.ViewsCount,
                CommentsCount = commentsCount,
                Hashtags = hashtags,
                Sections = sectionDict,
                InteractionCounts = counts,
                IsAnonymous = post.IsAnonymous,
                IsPinned = post.IsPinned,
                AverageRating = avgRating
            }
        };
    }

    public async Task<ApiResponse<PostDetailDto>> GetPostDetailForUserAsync(Guid id, string userId)
    {
        // Auto-increment view count
        var postForView = await _db.Posts.FindAsync(id);
        if (postForView != null) { postForView.ViewsCount++; await _db.SaveChangesAsync(); }

        var res = await GetPostDetailAsync(id);
        if (res.Data != null)
        {
            var userVote = await _db.SurveyVotes.FirstOrDefaultAsync(v => v.PostId == id && v.UserId == userId);
            if (userVote != null)
            {
                var strId = userVote.OptionId.ToString();
                if (int.TryParse(strId.Substring(24), out int idx))
                    res.Data.UserVotedIndex = idx;
            }
            var userRating = await _db.PostRatings.FirstOrDefaultAsync(r => r.PostId == id && r.UserId == userId);
            if (userRating != null) res.Data.UserRating = userRating.Score;
        }
        return res;
    }

    public async Task<ApiResponse<PostDetailDto>> CreatePostAsync(CreatePostDto dto, string authorId, string authorName, string authorStructure)
    {
        // Find the PostType by code
        var postType = await _db.PostTypes.FirstOrDefaultAsync(pt => pt.Code == dto.TypeCode);
        if (postType == null)
            return new ApiResponse<PostDetailDto> { Success = false, Message = "Type de post invalide." };

        // Verify group exists and user is a member (if not submitting as admin to a global group)
        var group = await _db.Groups.FindAsync(dto.GroupId);
        if (group == null)
            return new ApiResponse<PostDetailDto> { Success = false, Message = "Groupe introuvable." };

        var isMember = await _db.GroupMembers.AnyAsync(m => m.GroupId == dto.GroupId && m.UserId == authorId);
        if (!isMember)
            return new ApiResponse<PostDetailDto> { Success = false, Message = "Vous devez rejoindre ce groupe pour y publier." };

        var post = new Post
        {
            Id = Guid.NewGuid(),
            GroupId = dto.GroupId,
            DomainId = dto.DomainId,
            PostTypeId = postType.Id,
            AuthorId = authorId,
            Title = dto.Title,
            Status = PostStatus.ACTIVE,
            IsAnonymous = dto.IsAnonymous,
            ViewsCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Posts.Add(post);

        // Add sections
        int order = 0;
        foreach (var section in dto.Sections)
        {
            if (Enum.TryParse<SectionType>(section.Key, out var sectionType))
            {
                _db.PostSections.Add(new PostSection
                {
                    Id = Guid.NewGuid(),
                    PostId = post.Id,
                    SectionType = sectionType,
                    Content = section.Value,
                    OrderIndex = order++
                });
            }
        }

        // Add hashtags
        foreach (var tag in dto.Hashtags)
        {
            _db.PostHashtags.Add(new PostHashtag
            {
                Id = Guid.NewGuid(),
                PostId = post.Id,
                Tag = tag.StartsWith("#") ? tag : $"#{tag}"
            });
        }

        await _db.SaveChangesAsync();

        // Return the created post detail
        return await GetPostDetailAsync(post.Id);
    }

    public async Task<ApiResponse<PostDetailDto>> UpdatePostAsync(Guid id, CreatePostDto dto, string userId, string role)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null || post.Status == PostStatus.DELETED) 
            return new ApiResponse<PostDetailDto> { Success = false, Message = "Poste introuvable." };

        if (role != "SuperAdmin" && post.AuthorId != userId)
            return new ApiResponse<PostDetailDto> { Success = false, Message = "Non autorisé à modifier ce poste." };

        var postType = await _db.PostTypes.FirstOrDefaultAsync(pt => pt.Code == dto.TypeCode);
        if (postType == null) return new ApiResponse<PostDetailDto> { Success = false, Message = "Type invalide." };
        
        post.Title = dto.Title;
        if (dto.GroupId != Guid.Empty) post.GroupId = dto.GroupId;
        if (dto.DomainId.HasValue) post.DomainId = dto.DomainId;
        post.PostTypeId = postType.Id;
        post.UpdatedAt = DateTime.UtcNow;
        _db.Posts.Update(post);

        // Delete old sections and hashtags
        var oldSecs = await _db.PostSections.Where(s => s.PostId == id).ToListAsync();
        _db.PostSections.RemoveRange(oldSecs);
        
        var oldTags = await _db.PostHashtags.Where(h => h.PostId == id).ToListAsync();
        _db.PostHashtags.RemoveRange(oldTags);

        // Recreate sections
        int order = 0;
        foreach (var section in dto.Sections)
            if (Enum.TryParse<SectionType>(section.Key, out var sectionType))
                _db.PostSections.Add(new PostSection { Id = Guid.NewGuid(), PostId = post.Id, SectionType = sectionType, Content = section.Value, OrderIndex = order++ });

        foreach (var tag in dto.Hashtags)
            _db.PostHashtags.Add(new PostHashtag { Id = Guid.NewGuid(), PostId = post.Id, Tag = tag.StartsWith("#") ? tag : $"#{tag}" });

        await _db.SaveChangesAsync();

        return await GetPostDetailAsync(post.Id);
    }

    public async Task<ApiResponse<bool>> DeletePostAsync(Guid id, string userId, string role)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return new ApiResponse<bool> { Success = false, Message = "Poste introuvable." };

        // Admin can delete any, user can only delete their own
        if (role != "SuperAdmin" && post.AuthorId != userId)
            return new ApiResponse<bool> { Success = false, Message = "Non autorisé." };

        post.Status = PostStatus.DELETED;
        _db.Posts.Update(post);
        return new ApiResponse<bool> { Success = true, Message = "Poste supprimé.", Data = true };
    }

    public async Task<ApiResponse<bool>> TogglePinPostAsync(Guid id, string role)
    {
        if (role != "SuperAdmin")
            return new ApiResponse<bool> { Success = false, Message = "Seul un SuperAdmin peut épingler un poste." };

        var post = await _db.Posts.FindAsync(id);
        if (post == null) return new ApiResponse<bool> { Success = false, Message = "Poste introuvable." };

        post.IsPinned = !post.IsPinned;
        _db.Posts.Update(post);
        await _db.SaveChangesAsync();

        return new ApiResponse<bool> { Success = true, Message = post.IsPinned ? "Poste épinglé." : "Poste désépinglé.", Data = post.IsPinned };
    }

    public async Task<ApiResponse<bool>> SubmitInteractionAsync(Guid postId, string userId, int selectedIndex)

    {
        var existing = await _db.SurveyVotes.FirstOrDefaultAsync(v => v.PostId == postId && v.UserId == userId);
        if (existing != null)
        {
            _db.SurveyVotes.Remove(existing);
            await _db.SaveChangesAsync();
        }

        var optGuid = new Guid($"00000000-0000-0000-0000-{selectedIndex:D12}");
        _db.SurveyVotes.Add(new BaseLibrary.Entities.Survey.SurveyVote
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            UserId = userId,
            OptionId = optGuid,
            VotedAt = DateTime.UtcNow
        });
        
        await _db.SaveChangesAsync();
        return new ApiResponse<bool> { Success = true, Data = true, Message = "A voté." };
    }

    // ===== COMMENT OPERATIONS =====
    public async Task<List<CommentDto>> GetCommentsAsync(Guid postId, string userId)
    {
        var comments = await _db.Comments
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        var result = new List<CommentDto>();
        foreach (var c in comments)
        {
            var upvotes = await _db.CommentVotes.CountAsync(v => v.CommentId == c.Id && v.VoteType == 1);
            var downvotes = await _db.CommentVotes.CountAsync(v => v.CommentId == c.Id && v.VoteType == -1);
            var myVote = await _db.CommentVotes.FirstOrDefaultAsync(v => v.CommentId == c.Id && v.UserId == userId);

            result.Add(new CommentDto
            {
                Id = c.Id,
                PostId = c.PostId,
                ParentId = c.ParentId,
                AuthorId = c.AuthorId,
                AuthorName = c.AuthorId,
                Content = c.Content,
                Score = upvotes - downvotes,
                UserVote = myVote?.VoteType ?? 0,
                CreatedAt = c.CreatedAt
            });
        }
        return result;
    }

    public async Task<ApiResponse<CommentDto>> AddCommentAsync(CreateCommentDto dto, string userId)
    {
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PostId = dto.PostId,
            ParentId = dto.ParentId,
            AuthorId = userId,
            Content = dto.Content,
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow
        };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        return new ApiResponse<CommentDto>
        {
            Success = true,
            Data = new CommentDto
            {
                Id = comment.Id,
                PostId = comment.PostId,
                ParentId = comment.ParentId,
                AuthorId = comment.AuthorId,
                AuthorName = comment.AuthorId,
                Content = comment.Content,
                Score = 0,
                UserVote = 0,
                CreatedAt = comment.CreatedAt
            }
        };
    }

    public async Task<ApiResponse<bool>> VoteCommentAsync(Guid commentId, string userId, short voteType)
    {
        var existing = await _db.CommentVotes.FirstOrDefaultAsync(v => v.CommentId == commentId && v.UserId == userId);
        if (existing != null)
        {
            if (existing.VoteType == voteType)
            {
                _db.CommentVotes.Remove(existing);
                await _db.SaveChangesAsync();
                return new ApiResponse<bool> { Success = true, Data = true, Message = "Vote retiré." };
            }
            existing.VoteType = voteType;
            existing.VotedAt = DateTime.UtcNow;
        }
        else
        {
            _db.CommentVotes.Add(new CommentVote
            {
                Id = Guid.NewGuid(),
                CommentId = commentId,
                UserId = userId,
                VoteType = voteType,
                VotedAt = DateTime.UtcNow
            });
        }
        await _db.SaveChangesAsync();
        return new ApiResponse<bool> { Success = true, Data = true, Message = "A voté." };
    }

    public async Task<ApiResponse<double>> RatePostAsync(Guid postId, string userId, int rating)
    {
        if (rating < 1 || rating > 5) return new ApiResponse<double> { Success = false, Message = "Note invalide." };

        var post = await _db.Posts.FindAsync(postId);
        if (post == null) return new ApiResponse<double> { Success = false, Message = "Poste introuvable." };

        var isMember = await _db.GroupMembers.AnyAsync(m => m.GroupId == post.GroupId && m.UserId == userId);
        if (!isMember) return new ApiResponse<double> { Success = false, Message = "Vous devez être membre du groupe pour noter." };

        var existing = await _db.PostRatings.FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);
        if (existing != null)
        {
            existing.Score = rating;
            existing.CreatedAt = DateTime.UtcNow;
            _db.PostRatings.Update(existing);
        }
        else
        {
            _db.PostRatings.Add(new PostRating { Id = Guid.NewGuid(), PostId = postId, UserId = userId, Score = rating });
        }
        await _db.SaveChangesAsync();
        
        var allRatings = await _db.PostRatings.Where(r => r.PostId == postId).ToListAsync();
        double newAverage = allRatings.Any() ? allRatings.Average(r => r.Score) : 0;
        
        return new ApiResponse<double> { Success = true, Message = "Avis enregistré.", Data = newAverage };
    }

    // ===== REPORT REASONS (Admin) =====
    public async Task<ApiResponse<List<ReportReasonDto>>> GetReportReasonsAsync()
    {
        var reasons = await _db.ReportReasons.Where(r => r.IsActive).OrderBy(r => r.CreatedAt).ToListAsync();
        var list = reasons.Select(r => new ReportReasonDto
        {
            Id = r.Id, ReasonTextAr = r.ReasonTextAr, ReasonTextFr = r.ReasonTextFr, IsActive = r.IsActive
        }).ToList();
        return new ApiResponse<List<ReportReasonDto>> { Success = true, Data = list };
    }

    public async Task<ApiResponse<ReportReasonDto>> CreateReportReasonAsync(CreateReportReasonDto dto)
    {
        var reason = new ReportReason
        {
            Id = Guid.NewGuid(),
            ReasonTextAr = dto.ReasonTextAr,
            ReasonTextFr = dto.ReasonTextFr,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.ReportReasons.Add(reason);
        await _db.SaveChangesAsync();
        return new ApiResponse<ReportReasonDto>
        {
            Success = true,
            Data = new ReportReasonDto { Id = reason.Id, ReasonTextAr = reason.ReasonTextAr, ReasonTextFr = reason.ReasonTextFr, IsActive = true },
            Message = "Motif créé."
        };
    }

    public async Task<ApiResponse<bool>> DeleteReportReasonAsync(Guid id)
    {
        var reason = await _db.ReportReasons.FindAsync(id);
        if (reason == null) return new ApiResponse<bool> { Success = false, Message = "Motif introuvable." };
        reason.IsActive = false;
        await _db.SaveChangesAsync();
        return new ApiResponse<bool> { Success = true, Data = true, Message = "Motif désactivé." };
    }

    public async Task<ApiResponse<bool>> SubmitReportAsync(Guid postId, string userId, SubmitReportDto dto)
    {
        var post = await _db.Posts.FindAsync(postId);
        if (post == null) return new ApiResponse<bool> { Success = false, Message = "Poste introuvable." };

        var reason = await _db.ReportReasons.FindAsync(dto.ReasonId);
        if (reason == null) return new ApiResponse<bool> { Success = false, Message = "Motif invalide." };

        // Check if already reported by this user
        var alreadyReported = await _db.PostReports.AnyAsync(r => r.PostId == postId && r.UserId == userId);
        if (alreadyReported) return new ApiResponse<bool> { Success = false, Message = "Vous avez déjà signalé ce poste." };

        _db.PostReports.Add(new PostReport
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            UserId = userId,
            ReasonId = dto.ReasonId,
            AdditionalComments = dto.AdditionalComments,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return new ApiResponse<bool> { Success = true, Data = true, Message = "Signalement enregistré. Merci." };
    }

    // ===== SEARCH =====
    public async Task<ApiResponse<PaginatedResponse<PostSummaryDto>>> SearchPostsAsync(SearchFilterDto filter, string userId)
    {
        var globalTypeIds = await _db.PostTypes.Where(pt => pt.RequiresPrivilege).Select(pt => pt.Id).ToListAsync();
        var userGroupIds = await _db.GroupMembers.Where(m => m.UserId == userId).Select(m => m.GroupId).ToListAsync();

        var query = _db.Posts.Where(p => 
            p.Status == PostStatus.ACTIVE &&
            (globalTypeIds.Contains(p.PostTypeId) || userGroupIds.Contains(p.GroupId))
        ).AsQueryable();

        // 1. Keyword (title)
        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var kw = filter.Keyword.Trim().ToLower();
            query = query.Where(p => p.Title.ToLower().Contains(kw));
        }

        // 2. Author
        if (!string.IsNullOrWhiteSpace(filter.AuthorId))
            query = query.Where(p => p.AuthorId == filter.AuthorId);

        // 3. Group
        if (filter.GroupId.HasValue && filter.GroupId.Value != Guid.Empty)
            query = query.Where(p => p.GroupId == filter.GroupId.Value);

        // 4. Domain
        if (filter.DomainId.HasValue && filter.DomainId.Value != Guid.Empty)
            query = query.Where(p => p.DomainId == filter.DomainId.Value);

        // 5. Post type
        if (!string.IsNullOrWhiteSpace(filter.TypeCode))
        {
            var typeId = await _db.PostTypes.Where(pt => pt.Code == filter.TypeCode).Select(pt => pt.Id).FirstOrDefaultAsync();
            if (typeId != Guid.Empty)
                query = query.Where(p => p.PostTypeId == typeId);
        }

        // 6. Date range
        if (filter.DateFrom.HasValue)
            query = query.Where(p => p.CreatedAt >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue)
            query = query.Where(p => p.CreatedAt <= filter.DateTo.Value);

        // 7. Views
        if (filter.ViewsMin.HasValue)
            query = query.Where(p => p.ViewsCount >= filter.ViewsMin.Value);
        if (filter.ViewsMax.HasValue)
            query = query.Where(p => p.ViewsCount <= filter.ViewsMax.Value);

        var posts = await query.OrderByDescending(p => p.IsPinned).ThenByDescending(p => p.CreatedAt).ToListAsync();

        // Post-filter: rating + hashtag (require joins)
        var result = new List<PostSummaryDto>();
        foreach (var p in posts)
        {
            var postType = await _db.PostTypes.FindAsync(p.PostTypeId);
            var hashtags = await _db.PostHashtags.Where(h => h.PostId == p.Id).Select(h => h.Tag).ToListAsync();
            var commentsCount = await _db.Comments.CountAsync(c => c.PostId == p.Id);
            var postRatings = await _db.PostRatings.Where(r => r.PostId == p.Id).ToListAsync();
            var avgRating = postRatings.Any() ? postRatings.Average(r => r.Score) : 0;
            var userRating = postRatings.FirstOrDefault(r => r.UserId == userId)?.Score ?? 0;

            // 8. Hashtag filter
            if (!string.IsNullOrWhiteSpace(filter.Hashtag))
            {
                var ht = filter.Hashtag.Trim().ToLower();
                if (!hashtags.Any(h => h.ToLower().Contains(ht)))
                    continue;
            }

            // Rating range filter
            if (filter.RatingMin.HasValue && avgRating < filter.RatingMin.Value) continue;
            if (filter.RatingMax.HasValue && avgRating > filter.RatingMax.Value) continue;

            result.Add(new PostSummaryDto
            {
                Id = p.Id,
                GroupId = p.GroupId,
                DomainId = p.DomainId,
                TypeCode = postType?.Code ?? "UNKNOWN",
                Title = p.Title,
                AuthorId = p.IsAnonymous ? "" : p.AuthorId,
                AuthorName = p.IsAnonymous ? "Anonyme" : p.AuthorId,
                AuthorStructure = "",
                CreatedAt = p.CreatedAt,
                ViewsCount = p.ViewsCount,
                CommentsCount = commentsCount,
                Hashtags = hashtags,
                IsAnonymous = p.IsAnonymous,
                IsPinned = p.IsPinned,
                AverageRating = avgRating,
                UserRating = userRating
            });
        }
        var totalCount = result.Count;
        var pagedResult = result.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();

        return new ApiResponse<PaginatedResponse<PostSummaryDto>>
        {
            Success = true,
            Data = new PaginatedResponse<PostSummaryDto>
            {
                Data = pagedResult,
                Total = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            }
        };
    }
}
