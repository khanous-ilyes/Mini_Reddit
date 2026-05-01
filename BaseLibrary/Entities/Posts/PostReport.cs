using System;
using BaseLibrary.Entities.Base;

namespace BaseLibrary.Entities.Posts;

public class PostReport
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid ReasonId { get; set; }
    public string? AdditionalComments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Post? Post { get; set; }
    public ReportReason? Reason { get; set; }
}
