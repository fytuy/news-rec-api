// Controllers/RecommendController.cs
using Microsoft.AspNetCore.Mvc;
using NewsRecommendation.Api.Services;

namespace NewsRecommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendController : ControllerBase
{
    private readonly EmbeddingService _service;

    public RecommendController(EmbeddingService service) => _service = service;

    [HttpPost]
    public IActionResult Post([FromBody] Input input)
    {
        if (string.IsNullOrWhiteSpace(input.Text))
            return BadRequest("文本不能为空");

        var embedding = _service.GetEmbedding(input.Text);
        return Ok(new
        {
            message = "推荐成功",
            preview = embedding.Take(5).ToArray(),
            dimensions = embedding.Length
        });
    }
}

public class Input { public string Text { get; set; } = ""; }