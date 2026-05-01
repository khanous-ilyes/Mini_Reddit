using System;

namespace BaseLibrary.Entities.Quiz;

public class QuizOption : Base.BaseEntity
{
    public Guid QuestionId {get;set;}
    public string OptionText {get;set;} = string.Empty;
    public bool IsCorrect {get;set;}
    public int OrderIndex {get;set;}
}
