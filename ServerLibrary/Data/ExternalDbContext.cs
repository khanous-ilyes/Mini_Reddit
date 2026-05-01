using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ServerLibrary.Data;

// Simulation de la DB externe
public class Structure
{
    [Key]
    public string IdStructure { get; set; } = string.Empty;
    public string StructureName { get; set; } = string.Empty;
}

public class Employee
{
    [Key]
    public string IdUser { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string IdStructure { get; set; } = string.Empty;
}

public class ExternalDbContext : DbContext
{
    public ExternalDbContext(DbContextOptions<ExternalDbContext> options) : base(options)
    {
    }

    public DbSet<Structure> Structures { get; set; }
    public DbSet<Employee> Employees { get; set; }
}
