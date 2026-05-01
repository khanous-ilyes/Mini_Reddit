using System;

namespace BaseLibrary.Entities.Posts;

public class Post : Base.BaseEntity
{
    public Guid GroupId {get;set;}
    public Guid? DomainId {get;set;}
    public Guid PostTypeId {get;set;}
    public string AuthorId {get;set;} = string.Empty;
    public string Title {get;set;} = string.Empty;
    public Helpers.PostStatus Status {get;set;} = Helpers.PostStatus.ACTIVE;
    public bool IsAnonymous {get;set;}
    public bool IsPinned {get;set;} = false;
    public int ViewsCount {get;set;}
    public DateTime CreatedAt {get;set;}
    public DateTime UpdatedAt {get;set;}
}
