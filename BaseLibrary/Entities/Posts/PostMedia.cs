using System;

namespace BaseLibrary.Entities.Posts;

public class PostMedia : Base.BaseEntity
{
    public Guid PostId {get;set;}
    public string FileUrl {get;set;} = string.Empty;
    public string? FileType {get;set;}
    public string? FileName {get;set;}
    public int OrderIndex {get;set;}
}
