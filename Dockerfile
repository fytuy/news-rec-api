# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN set -eux; \
    proj=$(find . -name '*.csproj' -print -quit); \
    echo "Found project: $proj"; \
    if [ -z "$proj" ]; then echo "No .csproj found" >&2; exit 1; fi; \
    dotnet restore "$proj"

RUN set -eux; \
    proj=$(find . -name '*.csproj' -print -quit); \
    dotnet publish "$proj" -c Release -o /app/out

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./out

EXPOSE 80

ENTRYPOINT ["bash", "-lc", "dotnet out/$(ls out | grep -m1 '\\.dll$')"]
