// Program.cs
using NewsRecommendation.Api.Services; // 必须这样写！

var builder = WebApplication.CreateBuilder(args);

// 添加服务
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<ModelDownloadService>(); // 现在能找到！
builder.Services.AddSingleton<EmbeddingService>();

// 开启 CORS（前端调用）
builder.Services.AddCors(options =>
{
    options.AddPolicy("all", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ------------- IMPORTANT: bind to PORT for container platforms -------------
// Read PORT env var (Render/Heroku/other container platforms set this)
// and bind to 0.0.0.0 so external traffic can reach the container.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");
// ---------------------------------------------------------------------------

// Optional: if HTTPS redirection causes problems in container, comment it out.
// app.UseHttpsRedirection();

app.UseCors("all");
app.UseAuthorization();
app.MapControllers();

// minimal health endpoint for quick check
app.MapGet("/", () => Results.Text("OK - news-rec-api is running"));

// start app
app.Run();
