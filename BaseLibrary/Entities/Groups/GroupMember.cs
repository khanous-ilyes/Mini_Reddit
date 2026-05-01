using System;

namespace BaseLibrary.Entities.Groups;

public class GroupMember : Base.BaseEntity
{
    public Guid GroupId {get;set;}
    public string UserId {get;set;} = string.Empty;
    public DateTime JoinedAt {get;set;}
}
