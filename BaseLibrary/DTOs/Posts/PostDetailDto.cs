using System.Collections.Generic;

namespace BaseLibrary.DTOs.Posts;

public class PostDetailDto : PostSummaryDto
{
    // Key: SectionType (PROBLEM_DESC, SOLUTION_DESC, CONTEXT, RESULT, ect), Value: Content
    public Dictionary<string, string> Sections { get; set; } = new();

    // Interaction data
    public Dictionary<int, int>? InteractionCounts { get; set; }
    public int UserVotedIndex { get; set; } = -1;
}
