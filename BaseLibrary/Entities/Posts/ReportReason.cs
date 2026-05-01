using System;
using BaseLibrary.Entities.Base;

namespace BaseLibrary.Entities.Posts;

public class ReportReason
{
    public Guid Id { get; set; }
    public string ReasonTextAr { get; set; } = string.Empty;
    public string ReasonTextFr { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
