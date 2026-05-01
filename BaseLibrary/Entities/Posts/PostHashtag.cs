using System;

namespace BaseLibrary.Entities.Posts;

public class PostHashtag : Base.BaseEntity
{
    public Guid PostId {get;set;}
    public string Tag {get;set;} = string.Empty;
}
