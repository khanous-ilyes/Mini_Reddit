using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ServerLibrary.Services.Implementations;
using BaseLibrary.DTOs.Posts;
using System.Security.Claims;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    public PostsController(IPostService postService) { _postService = postService; }

    public async Task<IActionResult> GetFeed([FromQuery] string? groupId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return Ok(await _postService.GetPostsFeedAsync(userId, groupId, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        if (_postService is PostService concreteService)
            return Ok(await concreteService.GetPostDetailForUserAsync(id, userId));
        return Ok(await _postService.GetPostDetailAsync(id));
    }

    [HttpPost("{id}/interact")]
    public async Task<IActionResult> Interact(Guid id, [FromBody] int index)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return Ok(await _postService.SubmitInteractionAsync(id, userId, index));
    }

    [HttpPost("{id}/rate")]
    public async Task<IActionResult> Rate(Guid id, [FromQuery] int rating)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return Ok(await _postService.RatePostAsync(id, userId, rating));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePostDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? userId;
        var userStructure = User.FindFirst("structure_name")?.Value ?? "";
        return Ok(await _postService.CreatePostAsync(dto, userId, userName, userStructure));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreatePostDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        return Ok(await _postService.UpdatePostAsync(id, dto, userId, role));
    }

    [HttpPut("{id}/pin")]
    public async Task<IActionResult> TogglePin(Guid id)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        return Ok(await _postService.TogglePinPostAsync(id, role));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        return Ok(await _postService.DeletePostAsync(id, userId, role));
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchFilterDto filter)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return Ok(await _postService.SearchPostsAsync(filter, userId));
    }

    // ===== Report Reasons (Admin) =====
    [HttpGet("report-reasons")]
    public async Task<IActionResult> GetReportReasons()
    {
        return Ok(await _postService.GetReportReasonsAsync());
    }

    [HttpPost("report-reasons")]
    public async Task<IActionResult> CreateReportReason([FromBody] CreateReportReasonDto dto)
    {
        return Ok(await _postService.CreateReportReasonAsync(dto));
    }

    [HttpDelete("report-reasons/{id}")]
    public async Task<IActionResult> DeleteReportReason(Guid id)
    {
        return Ok(await _postService.DeleteReportReasonAsync(id));
    }

    // ===== Post Reports (User) =====
    [HttpPost("{id}/report")]
    public async Task<IActionResult> ReportPost(Guid id, [FromBody] SubmitReportDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return Ok(await _postService.SubmitReportAsync(id, userId, dto));
    }
}
