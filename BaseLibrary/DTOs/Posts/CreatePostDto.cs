using System;
using System.Collections.Generic;

namespace BaseLibrary.DTOs.Posts;

public class CreatePostDto
{
    public string TypeCode { get; set; } = string.Empty; // SOLUTION or PROBLEM
    public Guid GroupId { get; set; }
    public Guid? DomainId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Dictionary<string, string> Sections { get; set; } = new();
    public List<string> Hashtags { get; set; } = new();
    public bool IsAnonymous { get; set; }
}
