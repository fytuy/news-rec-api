// Program.cs
using NewsRecommendation.Api.Services; // 必须这样写！

var builder = WebApplication.CreateBuilder(args);

// ------------------- 添加服务 -------------------
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<ModelDownloadService>();
builder.Services.AddSingleton<EmbeddingService>();

// 开启 CORS（允许任意来源访问 API）
builder.Services.AddCors(options =>
{
    options.AddPolicy("all", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
// -------------------------------------------------

var app = builder.Build();

// ------------------- 配置请求管道 -------------------
// Render / 容器平台需要绑定端口
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

// 可选：如果 HTTPS 重定向导致容器访问失败，可以注释掉
// app.UseHttpsRedirection();

// ⚠️ Routing 必须在 UseCors 前
app.UseRouting();

// ⚠️ UseCors 必须在 MapControllers 之前
app.UseCors("all");

app.UseAuthorization();

// 映射控制器
app.MapControllers();

// 健康检查接口
app.MapGet("/", () => Results.Text("OK - news-rec-api is running"));

// 启动应用
app.Run();
