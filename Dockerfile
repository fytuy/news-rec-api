# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制全部源码
COPY . .

# 还原并发布指定项目
RUN set -eux; \
    proj="./NewsRecommendation.Api.csproj"; \
    if [ ! -f "$proj" ]; then \
      proj=$(find . -name 'NewsRecommendation.Api.csproj' -print -quit); \
    fi; \
    echo "Using project: $proj"; \
    dotnet restore "$proj"; \
    dotnet publish "$proj" -c Release -o /app/out

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# 把发布产物复制到运行时镜像
COPY --from=build /app/out ./

# Render 会提供 PORT；暴露端口仅为声明性
EXPOSE 80

# 直接在 /app 目录运行 dll（dll 名称 = csproj 文件名）
ENTRYPOINT ["dotnet", "NewsRecommendation.Api.dll"]
