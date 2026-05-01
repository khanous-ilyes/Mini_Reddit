using System;
using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities.Base;

public class BaseEntity
{
    [Key]
    public Guid Id { get; set; }
}
