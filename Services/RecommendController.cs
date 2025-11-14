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
            return BadRequest(new { message = "文本不能为空" });

        try
        {
            var embedding = _service.GetEmbedding(input.Text);
            if (embedding == null)
            {
                return Ok(new
                {
                    message = "模型尚未加载完成，请稍后再试",
                    preview = Array.Empty<float>(),
                    dimensions = 0
                });
            }

            return Ok(new
            {
                message = "推荐成功",
                preview = embedding.Take(5).ToArray(),
                dimensions = embedding.Length
            });
        }
        catch (InvalidOperationException ex)
        {
            // 模型未加载或其他可控异常
            return Ok(new
            {
                message = ex.Message,
                preview = Array.Empty<float>(),
                dimensions = 0
            });
        }
        catch (Exception ex)
        {
            // 其它异常返回 500
            return StatusCode(500, new
            {
                message = "内部服务器错误",
                detail = ex.Message
            });
        }
    }
}

public class Input
{
    public string Text { get; set; } = "";
}
