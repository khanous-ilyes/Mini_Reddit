using System;
using BaseLibrary.Entities.Base;

namespace BaseLibrary.Entities.Posts;

public class PostRating
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Score { get; set; } // 1 to 5 stars
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
