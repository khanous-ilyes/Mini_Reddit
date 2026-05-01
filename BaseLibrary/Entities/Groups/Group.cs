using System;

namespace BaseLibrary.Entities.Groups;

public class Group : Base.BaseEntity
{
    public string Name {get;set;} = string.Empty;
    public string? Description {get;set;}
    public string? IconUrl {get;set;}
    public bool IsActive {get;set;} = true;
    public string? CreatedBy {get;set;}
    public DateTime CreatedAt {get;set;}
}
