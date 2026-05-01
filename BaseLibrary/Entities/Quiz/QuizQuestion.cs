using System;

namespace BaseLibrary.Entities.Quiz;

public class QuizQuestion : Base.BaseEntity
{
    public Guid PostId {get;set;}
    public string QuestionText {get;set;} = string.Empty;
    public int OrderIndex {get;set;}
}
