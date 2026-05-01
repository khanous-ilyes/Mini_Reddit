using System;

namespace BaseLibrary.Entities.Survey;

public class SurveyVote : Base.BaseEntity
{
    public Guid PostId {get;set;}
    public Guid OptionId {get;set;}
    public string UserId {get;set;} = string.Empty;
    public DateTime VotedAt {get;set;}
}
