using BaseLibrary.DTOs.Auth;
using BaseLibrary.Helpers;
using ServerLibrary.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ServerLibrary.Services.Implementations;

public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
}

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _appDb;
    private readonly ExternalDbContext _extDb;
    private readonly IConfiguration _config;

    public AuthService(ApplicationDbContext appDb, ExternalDbContext extDb, IConfiguration config)
    {
        _appDb = appDb;
        _extDb = extDb;
        _config = config;
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {
        // 1. Rechercher l'employé dans la DB externe (synonyme DBA)
        var employee = await _extDb.Employees.FirstOrDefaultAsync(e => e.IdUser == request.Username);
        
        if (employee == null)
        {
            return new ApiResponse<LoginResponseDto>
            {
                Success = false,
                Message = "Identifiant inconnu. Contactez votre administrateur DBA."
            };
        }

        // 2. Récupérer la structure
        var structure = await _extDb.Structures.FirstOrDefaultAsync(s => s.IdStructure == employee.IdStructure);

        // 3. Synchroniser le UserProfile dans l'app DB
        var profile = await _appDb.UserProfiles.FirstOrDefaultAsync(p => p.IdUser == request.Username);
        if (profile == null)
        {
            profile = new BaseLibrary.Entities.Auth.UserProfile { IdUser = request.Username, IsActive = true };
            _appDb.UserProfiles.Add(profile);
            await _appDb.SaveChangesAsync();
        }

        // 4. Déterminer le rôle (simulé — en prod viendrait du DBA)
        string role = "User";
        if (request.Username.Contains("admin", StringComparison.OrdinalIgnoreCase))
            role = "SuperAdmin";
        else if (request.Username.StartsWith("cm_", StringComparison.OrdinalIgnoreCase))
            role = "ContentManager";

        // 5. Générer le JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"] ?? "1234567890_VerySecretKeyForKnowledgeHub_2025");
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, employee.IdUser),
            new Claim(ClaimTypes.Name, $"{employee.Prenom} {employee.Nom}"),
            new Claim("id_structure", employee.IdStructure),
            new Claim("structure_name", structure?.StructureName ?? ""),
            new Claim("grade", employee.Grade),
            new Claim(ClaimTypes.Role, role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = "KnowledgeHub",
            Audience = "KnowledgeHub"
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        return new ApiResponse<LoginResponseDto>
        {
            Success = true,
            Message = $"Bienvenue {employee.Prenom} {employee.Nom} !",
            Data = new LoginResponseDto { Token = tokenHandler.WriteToken(token) }
        };
    }
}
