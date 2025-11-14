# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# 复制 csproj 并 restore
COPY *.csproj ./ 
RUN dotnet restore "NewsRecommendation.Api.csproj"

# 复制其余文件并 publish
COPY . ./
RUN dotnet publish "NewsRecommendation.Api.csproj" -c Release -o /app/out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# 如果有模型文件，需要也COPY过去
COPY Models ./Models

# 设置端口
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV DOTNET_ENABLE_PREVIEW_FEATURES=1

EXPOSE 8080
ENTRYPOINT ["dotnet", "NewsRecommendation.Api.dll"]
