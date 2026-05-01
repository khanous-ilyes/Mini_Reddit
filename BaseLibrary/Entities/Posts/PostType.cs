using System;

namespace BaseLibrary.Entities.Posts;

public class PostType : Base.BaseEntity
{
    public string Code {get;set;} = string.Empty;
    public string Label {get;set;} = string.Empty;
    public bool AllowComments {get;set;} = true;
    public bool RequiresPrivilege {get;set;} = false;
    public bool IsActive {get;set;} = true;
}
