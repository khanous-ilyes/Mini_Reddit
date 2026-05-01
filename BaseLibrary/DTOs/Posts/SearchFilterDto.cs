using System;
using System.Collections.Generic;

namespace BaseLibrary.DTOs.Posts;

public class SearchFilterDto
{
    public string? Keyword { get; set; }
    public string? AuthorId { get; set; }
    public Guid? GroupId { get; set; }
    public Guid? DomainId { get; set; }
    public string? TypeCode { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public double? RatingMin { get; set; }
    public double? RatingMax { get; set; }
    public int? ViewsMin { get; set; }
    public int? ViewsMax { get; set; }
    public string? Hashtag { get; set; }
    
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
