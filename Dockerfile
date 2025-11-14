# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制 csproj 并 restore
COPY *.csproj ./ 
RUN dotnet restore "NewsRecommendation.Api.csproj"

# 复制其余文件并 publish
COPY . ./
RUN dotnet publish "NewsRecommendation.Api.csproj" -c Release -o /app/out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# 复制模型文件
COPY Models ./Models

# 设置端口
ENV DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080
ENTRYPOINT ["dotnet", "NewsRecommendation.Api.dll"]
