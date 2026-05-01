using System;

namespace BaseLibrary.Entities.Posts;

public class CommentVote : Base.BaseEntity
{
    public Guid CommentId {get;set;}
    public string UserId {get;set;} = string.Empty;
    public short VoteType {get;set;}
    public DateTime VotedAt {get;set;}
}
