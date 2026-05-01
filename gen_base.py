import os

base_path = "c:/Users/ilyas/Desktop/OurProject/BaseLibrary"
directories = [
    f"{base_path}/Entities/Base",
    f"{base_path}/Entities/Auth",
    f"{base_path}/Entities/Groups",
    f"{base_path}/Entities/Posts",
    f"{base_path}/Entities/Quiz",
    f"{base_path}/Entities/Survey",
    f"{base_path}/DTOs",
    f"{base_path}/Helpers",
    f"{base_path}/Interfaces"
]

for d in directories:
    os.makedirs(d, exist_ok=True)

# Generate Helpers
with open(f"{base_path}/Helpers/Enums.cs", "w", encoding="utf-8") as f:
    f.write("""namespace BaseLibrary.Helpers;

public enum PostStatus { ACTIVE, ARCHIVED, DELETED }
public enum SectionType { PROBLEM_DESC, SOLUTION_DESC, CONTEXT, STEPS, RESULT, CONTENT }
""")

with open(f"{base_path}/Helpers/ApiResponse.cs", "w", encoding="utf-8") as f:
    f.write("""namespace BaseLibrary.Helpers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
""")

with open(f"{base_path}/Helpers/PaginatedResponse.cs", "w", encoding="utf-8") as f:
    f.write("""using System.Collections.Generic;

namespace BaseLibrary.Helpers;

public class PaginatedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
""")

# BaseEntity
with open(f"{base_path}/Entities/Base/BaseEntity.cs", "w", encoding="utf-8") as f:
    f.write("""using System;
using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities.Base;

public class BaseEntity
{
    [Key]
    public Guid Id { get; set; }
}
""")

# Entities (Simplified versions to be expanded safely later or just sufficient for compilation)
entities = {
    "Auth/UserProfile": "public string IdUser { get; set; } public string? Telephone { get; set; } public string? Bio { get; set; } public string? AvatarUrl { get; set; } public bool IsActive { get; set; } = true;",
    "Groups/Group": "public string Name {get;set;} = string.Empty; public string? Description {get;set;} public string? IconUrl {get;set;} public bool IsActive {get;set;} = true; public string? CreatedBy {get;set;} public DateTime CreatedAt {get;set;}",
    "Groups/Domain": "public Guid GroupId {get;set;} public string Name {get;set;} = string.Empty; public string? Description {get;set;} public bool IsActive {get;set;} = true;",
    "Groups/GroupMember": "public Guid GroupId {get;set;} public string UserId {get;set;} = string.Empty; public DateTime JoinedAt {get;set;}",
    "Posts/PostType": "public string Code {get;set;} = string.Empty; public string Label {get;set;} = string.Empty; public bool AllowComments {get;set;} = true; public bool RequiresPrivilege {get;set;} = false; public bool IsActive {get;set;} = true;",
    "Posts/Post": "public Guid GroupId {get;set;} public Guid? DomainId {get;set;} public Guid PostTypeId {get;set;} public string AuthorId {get;set;} = string.Empty; public string Title {get;set;} = string.Empty; public Helpers.PostStatus Status {get;set;} = Helpers.PostStatus.ACTIVE; public int ViewsCount {get;set;} public DateTime CreatedAt {get;set;} public DateTime UpdatedAt {get;set;}",
    "Posts/PostSection": "public Guid PostId {get;set;} public Helpers.SectionType SectionType {get;set;} public string Content {get;set;} = string.Empty; public int OrderIndex {get;set;}",
    "Posts/PostHashtag": "public Guid PostId {get;set;} public string Tag {get;set;} = string.Empty;",
    "Posts/PostMedia": "public Guid PostId {get;set;} public string FileUrl {get;set;} = string.Empty; public string? FileType {get;set;} public string? FileName {get;set;} public int OrderIndex {get;set;}",
    "Posts/Comment": "public Guid PostId {get;set;} public Guid? ParentId {get;set;} public string AuthorId {get;set;} = string.Empty; public string Content {get;set;} = string.Empty; public bool IsBestAnswer {get;set;} public string Status {get;set;} = string.Empty; public DateTime CreatedAt {get;set;}",
    "Posts/CommentVote": "public Guid CommentId {get;set;} public string UserId {get;set;} = string.Empty; public short VoteType {get;set;} public DateTime VotedAt {get;set;}",
    "Quiz/QuizQuestion": "public Guid PostId {get;set;} public string QuestionText {get;set;} = string.Empty; public int OrderIndex {get;set;}",
    "Quiz/QuizOption": "public Guid QuestionId {get;set;} public string OptionText {get;set;} = string.Empty; public bool IsCorrect {get;set;} public int OrderIndex {get;set;}",
    "Quiz/QuizResponse": "public Guid PostId {get;set;} public string UserId {get;set;} = string.Empty; public Guid QuestionId {get;set;} public Guid SelectedOption {get;set;} public DateTime AnsweredAt {get;set;}",
    "Survey/SurveyOption": "public Guid PostId {get;set;} public string OptionText {get;set;} = string.Empty; public int OrderIndex {get;set;}",
    "Survey/SurveyVote": "public Guid PostId {get;set;} public Guid OptionId {get;set;} public string UserId {get;set;} = string.Empty; public DateTime VotedAt {get;set;}"
}

for path, props in entities.items():
    file_path = f"{base_path}/Entities/{path}.cs"
    class_name = path.split('/')[-1]
    namespace = "BaseLibrary.Entities." + path.split('/')[0]
    
    # if it's user profile it's not a BaseEntity if it uses IdUser as PK, but we can just use BaseEntity and override or we just make it independent
    base_class = " : Base.BaseEntity" if class_name != "UserProfile" else ""
    
    with open(file_path, "w", encoding="utf-8") as f:
        f.write(f'''using System;

namespace {namespace};

public class {class_name}{base_class}
{{
''')
        if class_name == "UserProfile":
            f.write('    [System.ComponentModel.DataAnnotations.Key]\n')

        for prop in props.split('public '):
            if prop:
                f.write(f'    public {prop.strip()}\n')
                
        f.write('}\n')

print("Generated BaseLibrary boilerplate")
