using System;

namespace BaseLibrary.Entities.Auth;

public class UserProfile
{
    [System.ComponentModel.DataAnnotations.Key]
    public string IdUser { get; set; }
    public string? Telephone { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
