// Services/ModelDownloadService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NewsRecommendation.Api.Services;

namespace NewsRecommendation.Api.Services
{
    public class ModelDownloadService : IHostedService
    {
        private readonly ILogger<ModelDownloadService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _modelsDir = "Models";

        public ModelDownloadService(
            ILogger<ModelDownloadService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task StartAsync(CancellationToken ct)
        {
            _logger.LogInformation("开始下载模型...");

            Directory.CreateDirectory(_modelsDir);

            var tasks = new[]
            {
                DownloadAsync("https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/onnx/model.onnx", "all-MiniLM-L6-v2.onnx", ct),
                DownloadAsync("https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/vocab.txt", "vocab.txt", ct)
            };

            await Task.WhenAll(tasks);
            _logger.LogInformation("模型下载完成！");
        }

        private async Task DownloadAsync(string url, string fileName, CancellationToken ct)
        {
            var path = Path.Combine(_modelsDir, fileName);
            if (File.Exists(path))
            {
                _logger.LogInformation("文件已存在: {File}", path);
                return;
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(15);

            var response = await client.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            await using var file = File.Create(path);
            await stream.CopyToAsync(file, ct);

            _logger.LogInformation("下载完成: {File}", path);
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}