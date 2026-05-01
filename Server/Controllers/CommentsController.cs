using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ServerLibrary.Services.Implementations;
using BaseLibrary.DTOs.Posts;
using System.Security.Claims;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly PostService _postService;
    public CommentsController(IPostService postService) { _postService = (PostService)postService; }

    [HttpGet("{postId}")]
    public async Task<IActionResult> GetComments(Guid postId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return Ok(await _postService.GetCommentsAsync(postId, userId));
    }

    [HttpPost]
    public async Task<IActionResult> AddComment([FromBody] CreateCommentDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return Ok(await _postService.AddCommentAsync(dto, userId));
    }

    [HttpPost("{commentId}/vote")]
    public async Task<IActionResult> Vote(Guid commentId, [FromBody] short voteType)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return Ok(await _postService.VoteCommentAsync(commentId, userId, voteType));
    }
}
