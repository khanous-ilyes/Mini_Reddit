using System;

namespace BaseLibrary.Entities.Survey;

public class SurveyOption : Base.BaseEntity
{
    public Guid PostId {get;set;}
    public string OptionText {get;set;} = string.Empty;
    public int OrderIndex {get;set;}
}
