using System;

namespace BaseLibrary.Entities.Posts;

public class Comment : Base.BaseEntity
{
    public Guid PostId {get;set;}
    public Guid? ParentId {get;set;}
    public string AuthorId {get;set;} = string.Empty;
    public string Content {get;set;} = string.Empty;
    public bool IsBestAnswer {get;set;}
    public string Status {get;set;} = string.Empty;
    public DateTime CreatedAt {get;set;}
}
