using System;
using System.Collections.Generic;

namespace BaseLibrary.DTOs.Posts;

public class PostSummaryDto
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid? DomainId { get; set; }
    public string TypeCode { get; set; } = string.Empty; 
    public string Title { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorStructure { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ViewsCount { get; set; }
    public int CommentsCount { get; set; }
    public List<string> Hashtags { get; set; } = new();
    public bool IsAnonymous { get; set; }
    public bool IsPinned { get; set; }
    public double AverageRating { get; set; }
    public int UserRating { get; set; }
}
