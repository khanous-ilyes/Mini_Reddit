using System;

namespace BaseLibrary.Entities.Posts;

public class PostSection : Base.BaseEntity
{
    public Guid PostId {get;set;}
    public Helpers.SectionType SectionType {get;set;}
    public string Content {get;set;} = string.Empty;
    public int OrderIndex {get;set;}
}
