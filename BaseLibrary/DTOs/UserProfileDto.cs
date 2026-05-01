using System;
using System.Collections.Generic;
using BaseLibrary.DTOs.Posts;

namespace BaseLibrary.DTOs;

public class UserProfileDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string Structure { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Telephone { get; set; }
    public string? AvatarUrl { get; set; }
    public int TotalPosts { get; set; }
    public int TotalSolutions { get; set; }
    public int TotalProblems { get; set; }
    public int TotalViews { get; set; }
    public DateTime MemberSince { get; set; }
    public List<PostSummaryDto> RecentPosts { get; set; } = new();
}
