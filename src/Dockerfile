# syntax=docker/dockerfile:1.7

# Build: Solution
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /proj
RUN --mount=type=bind,source=.,target=/context \
    find /context -name "*.csproj" -print0 | \
    xargs -0 -I {} sh -c " \
    rel=\$(echo {} | sed 's|^/context/||'); \
    dir=\$(dirname \"\$rel\"); \
    mkdir -p \"\$dir\"; \
    cp {} \"\$dir\"; \
    " && \
    cp /context/WebApi.sln .
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore WebApi.sln
COPY --link . .

# Publish: Issuer
FROM build AS issuer-publish
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish --no-restore \
    WebApi.Issuer/WebApi.Issuer.csproj \
    -c Release \
    -o /publish/issuer \
    -p:DebugType=none \
    -p:DebugSymbols=false \
    -p:IsContainerBuild=true

# Publish: API
FROM build AS api-publish
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish --no-restore \
    WebApi.Service/WebApi.Service.csproj \
    -c Release \
    -o /publish/api \
    -p:DebugType=none \
    -p:DebugSymbols=false \
    -p:IsContainerBuild=true

# Publish: Client
FROM build AS client-publish
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish --no-restore \
    WebApi.Client/WebApi.Client.csproj \
    -c Release \
    -o /publish/client \
    -p:DebugType=none \
    -p:DebugSymbols=false \
    -p:IsContainerBuild=true

# Runtime: Issuer
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS issuer-runtime
USER $APP_UID
WORKDIR /app
COPY --link --from=issuer-publish /publish/issuer .
ENTRYPOINT ["./WebApi.Issuer"]

# Runtime: API
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS api-runtime
USER $APP_UID
WORKDIR /app
COPY --link --from=api-publish /publish/api .
ENTRYPOINT ["./WebApi.Service"]

# Runtime: Client
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS client-runtime
USER $APP_UID
WORKDIR /app
COPY --link --from=client-publish /publish/client .
ENTRYPOINT ["sleep", "infinity"]