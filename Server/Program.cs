using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ServerLibrary.Data;
using ServerLibrary.Data.Seeders;
using ServerLibrary.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add DbContexts for SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AppDb")));

builder.Services.AddDbContext<ExternalDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ExternalDb")));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "KnowledgeHub",
            ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "KnowledgeHub",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? "1234567890_VerySecretKeyForKnowledgeHub_2025"))
        };
    });

// CORS for Blazor WASM
builder.Services.AddCors(opt => opt.AddPolicy("BlazorClient",
    p => p.WithOrigins("http://localhost:5168", "https://localhost:7168", "http://localhost:5043").AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ========== AUTO-CREATE & SEED DATABASES ==========
using (var scope = app.Services.CreateScope())
{
    var appDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var extDb = scope.ServiceProvider.GetRequiredService<ExternalDbContext>();

    // Create all tables automatically
    appDb.Database.EnsureCreated();
    extDb.Database.EnsureCreated();

    // Seed External DB — Simulating DBA synonym tables (Structures + Employees + Roles)
    if (!extDb.Structures.Any())
    {
        extDb.Structures.AddRange(
            new Structure { IdStructure = "STR001", StructureName = "DSI - Direction des Systèmes d'Information" },
            new Structure { IdStructure = "STR002", StructureName = "DRH - Direction des Ressources Humaines" },
            new Structure { IdStructure = "STR003", StructureName = "DSEC - Direction de la Sécurité" },
            new Structure { IdStructure = "STR004", StructureName = "DG - Direction Générale" },
            new Structure { IdStructure = "STR005", StructureName = "DAF - Direction Administrative et Financière" }
        );
        extDb.SaveChanges();
    }

    if (!extDb.Employees.Any())
    {
        extDb.Employees.AddRange(
            // SuperAdmins (username contains "admin")
            new Employee { IdUser = "admin",    Nom = "BENALI",   Prenom = "Mohamed",  Grade = "Directeur SI",           IdStructure = "STR001" },
            new Employee { IdUser = "admin2",   Nom = "HADJ",     Prenom = "Amina",    Grade = "Chef de Département",    IdStructure = "STR001" },
            // ContentManagers (username starts with "cm_")
            new Employee { IdUser = "cm_rh",    Nom = "SAIDI",    Prenom = "Fatima",   Grade = "Responsable Formation",  IdStructure = "STR002" },
            new Employee { IdUser = "cm_sec",   Nom = "KHELIFI",  Prenom = "Youcef",   Grade = "Analyste Sécurité",      IdStructure = "STR003" },
            // Regular Users
            new Employee { IdUser = "EMP001",   Nom = "BOUDIAF",  Prenom = "Karim",    Grade = "Ingénieur Développeur",  IdStructure = "STR001" },
            new Employee { IdUser = "EMP002",   Nom = "MEZIANE",  Prenom = "Sarah",    Grade = "Architecte Logiciel",    IdStructure = "STR001" },
            new Employee { IdUser = "EMP003",   Nom = "TABET",    Prenom = "Riad",     Grade = "Technicien Réseau",      IdStructure = "STR003" },
            new Employee { IdUser = "EMP004",   Nom = "FERHAT",   Prenom = "Lina",     Grade = "Chargée RH",             IdStructure = "STR002" },
            new Employee { IdUser = "EMP005",   Nom = "DJEBBAR",  Prenom = "Omar",     Grade = "Comptable",              IdStructure = "STR005" }
        );
        extDb.SaveChanges();
    }

    // Seed App DB (PostTypes)
    await PostTypeSeeder.SeedAsync(appDb);

    // Ensure new tables exist (EnsureCreated won't add them to existing DB)
    await appDb.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS ReportReasons (
            Id TEXT PRIMARY KEY,
            ReasonTextAr TEXT NOT NULL DEFAULT '',
            ReasonTextFr TEXT NOT NULL DEFAULT '',
            IsActive INTEGER NOT NULL DEFAULT 1,
            CreatedAt TEXT NOT NULL DEFAULT '2026-01-01T00:00:00'
        );
        CREATE TABLE IF NOT EXISTS PostReports (
            Id TEXT PRIMARY KEY,
            PostId TEXT NOT NULL,
            UserId TEXT NOT NULL DEFAULT '',
            ReasonId TEXT NOT NULL,
            AdditionalComments TEXT,
            CreatedAt TEXT NOT NULL DEFAULT '2026-01-01T00:00:00',
            FOREIGN KEY (PostId) REFERENCES Posts(Id),
            FOREIGN KEY (ReasonId) REFERENCES ReportReasons(Id)
        );
    ");

    try
    {
        await appDb.Database.ExecuteSqlRawAsync("ALTER TABLE Posts ADD COLUMN IsPinned INTEGER NOT NULL DEFAULT 0;");
    }
    catch { /* Ignore if it already exists */ }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("BlazorClient");
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
