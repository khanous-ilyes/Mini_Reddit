using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BaseLibrary.DTOs.Media;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MediaController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public MediaController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<UploadResponseDto>> Upload(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new UploadResponseDto { Success = false, Message = "No file uploaded" });

                var uploadsPath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return the relative URL (assuming the server is at the root)
                var url = $"/uploads/{fileName}";
                return Ok(new UploadResponseDto { Success = true, Url = url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new UploadResponseDto { Success = false, Message = ex.Message });
            }
        }
    }
}
