using System;

namespace BaseLibrary.Entities.Quiz;

public class QuizResponse : Base.BaseEntity
{
    public Guid PostId {get;set;}
    public string UserId {get;set;} = string.Empty;
    public Guid QuestionId {get;set;}
    public Guid SelectedOption {get;set;}
    public DateTime AnsweredAt {get;set;}
}
