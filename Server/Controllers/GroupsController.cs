using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ServerLibrary.Services.Implementations;
using BaseLibrary.DTOs.Groups;
using System.Security.Claims;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    public GroupsController(IGroupService groupService) { _groupService = groupService; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return Ok(await _groupService.GetAllGroupsAsync(userId));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await _groupService.GetGroupByIdAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateGroupDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return Ok(await _groupService.CreateGroupAsync(dto, userId));
    }

    [HttpPut]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Update([FromBody] UpdateGroupDto dto)
    {
        return Ok(await _groupService.UpdateGroupAsync(dto));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return Ok(await _groupService.DeleteGroupAsync(id));
    }

    [HttpPost("{id}/join")]
    public async Task<IActionResult> Join(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return Ok(await _groupService.JoinGroupAsync(id, userId));
    }

    [HttpPost("{id}/leave")]
    public async Task<IActionResult> Leave(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return Ok(await _groupService.LeaveGroupAsync(id, userId));
    }

    [HttpGet("{id}/domains")]
    public async Task<IActionResult> GetDomains(Guid id)
    {
        return Ok(await _groupService.GetDomainsAsync(id));
    }
}
