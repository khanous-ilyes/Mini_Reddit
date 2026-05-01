using System;
using System.Collections.Generic;

namespace BaseLibrary.DTOs.Groups;

public class CreateGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public List<DomainDto> Domains { get; set; } = new();
}

public class UpdateGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public List<DomainDto> Domains { get; set; } = new();
}
