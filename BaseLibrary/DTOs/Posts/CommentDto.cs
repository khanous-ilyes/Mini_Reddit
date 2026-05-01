using System;

namespace BaseLibrary.DTOs.Posts;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid? ParentId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Score { get; set; }
    public short UserVote { get; set; } // 0=none, 1=up, -1=down
    public DateTime CreatedAt { get; set; }
}

public class CreateCommentDto
{
    public Guid PostId { get; set; }
    public Guid? ParentId { get; set; }
    public string Content { get; set; } = string.Empty;
}
