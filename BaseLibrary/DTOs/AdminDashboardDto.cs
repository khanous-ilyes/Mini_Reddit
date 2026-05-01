namespace BaseLibrary.DTOs;

public class UserRankDto
{
    public string UserId { get; set; } = string.Empty;
    public double Score { get; set; }
    public int PostCount { get; set; }
    public int CommentCount { get; set; }
    public int VoteBalance { get; set; }
    public int ReportCount { get; set; }
}

public class DomainTrendDto
{
    public Guid DomainId { get; set; }
    public string DomainName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public int PostCount { get; set; }
}

public class PostTrendDto
{
    public Guid PostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string TypeCode { get; set; } = string.Empty;
    public int ViewsCount { get; set; }
    public double AverageRating { get; set; }
}

public class AdminDashboardDto
{
    public List<UserRankDto> TopRatingUsers { get; set; } = new();
    public List<UserRankDto> TopVoteUsers { get; set; } = new();
    public List<UserRankDto> BadRatingUsers { get; set; } = new();
    public List<UserRankDto> NegativeVoteUsers { get; set; } = new();
    public List<UserRankDto> TopSolutionAuthors { get; set; } = new();
    public List<UserRankDto> TopProblemAuthors { get; set; } = new();
    public List<DomainTrendDto> TrendingDomains { get; set; } = new();
    public List<PostTrendDto> TrendingProblems { get; set; } = new();
    public List<UserRankDto> TopReporters { get; set; } = new();
    
    // Global stats
    public int TotalPosts { get; set; }
    public int TotalUsers { get; set; }
    public int TotalReports { get; set; }
    public int TotalComments { get; set; }
}
