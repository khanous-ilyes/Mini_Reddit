using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ServerLibrary.Services.Implementations;
using System.Security.Claims;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    public AdminController(IAdminService adminService) { _adminService = adminService; }

    private string Role => User.FindFirst(ClaimTypes.Role)?.Value ?? "";

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        if (Role != "SuperAdmin" && Role != "Moderator")
            return Forbid();

        return Ok(await _adminService.GetDashboardAsync());
    }

    [HttpGet("reporters")]
    public async Task<IActionResult> GetReporters([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (Role != "SuperAdmin" && Role != "Moderator")
            return Forbid();

        return Ok(await _adminService.GetReportersAsync(page, pageSize));
    }
}
