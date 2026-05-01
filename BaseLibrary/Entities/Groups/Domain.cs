using System;

namespace BaseLibrary.Entities.Groups;

public class Domain : Base.BaseEntity
{
    public Guid GroupId {get;set;}
    public string Name {get;set;} = string.Empty;
    public string? Description {get;set;}
    public bool IsActive {get;set;} = true;
}
