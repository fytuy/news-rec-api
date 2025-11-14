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

app.UseHttpsRedirection();
app.UseCors("all");
app.UseAuthorization();
app.MapControllers();

app.Run();