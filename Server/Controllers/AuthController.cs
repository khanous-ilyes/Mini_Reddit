using BaseLibrary.DTOs.Auth;
using ServerLibrary.Services.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
        if (response.Success)
            return Ok(response);
        return BadRequest(response);
    }

    [HttpPost("register-test")]
    public async Task<IActionResult> RegisterTest([FromBody] RegisterRequestDto request)
    {
        var response = await _authService.RegisterTestAsync(request);
        if (response.Success)
            return Ok(response);
        return BadRequest(response);
    }
}
