using System;

namespace BaseLibrary.DTOs.Groups;

public class GroupSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public bool IsJoined { get; set; }
    public List<DomainDto> Domains { get; set; } = new();
}
