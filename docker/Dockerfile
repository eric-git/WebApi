# syntax=docker/dockerfile:1

# ------------------------------------------------------------
# Build: Solution
# ------------------------------------------------------------
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
    cp /context/WebApi.sln . && \
    if [ -f /context/Directory.Build.props ]; then cp /context/Directory.Build.props .; fi && \
    if [ -f /context/Directory.Build.targets ]; then cp /context/Directory.Build.targets .; fi && \
    if [ -f /context/Directory.Packages.props ]; then cp /context/Directory.Packages.props .; fi
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore WebApi.sln
COPY --link . .

# ------------------------------------------------------------
# Publish: Issuer
# ------------------------------------------------------------
FROM build AS issuer-publish
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish \
    WebApi.Issuer/WebApi.Issuer.csproj \
    -c Release \
    -o /publish/issuer \
    -p:IsContainerBuild=true

# ------------------------------------------------------------
# Publish: API
# ------------------------------------------------------------
FROM build AS api-publish
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish \
    WebApi.Service/WebApi.Service.csproj \
    -c Release \
    -o /publish/api \
    -p:IsContainerBuild=true

# ------------------------------------------------------------
# Publish: Client
# ------------------------------------------------------------
FROM build AS client-publish
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish \
    WebApi.Client/WebApi.Client.csproj \
    -c Release \
    -o /publish/client \
    -p:IsContainerBuild=true

# ------------------------------------------------------------
# Runtime: Issuer
# ------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine-composite AS issuer-runtime
USER $APP_UID
WORKDIR /app
COPY --link --from=issuer-publish /publish/issuer .
ENTRYPOINT ["dotnet", "WebApi.Issuer.dll"]

# ------------------------------------------------------------
# Runtime: API
# ------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine-composite AS api-runtime
USER $APP_UID
WORKDIR /app
COPY --link --from=api-publish /publish/api .
ENTRYPOINT ["dotnet", "WebApi.Service.dll"]

# ------------------------------------------------------------
# Runtime: Client
# ------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS client-runtime
USER $APP_UID
WORKDIR /app
COPY --link --from=client-publish /publish/client .
ENTRYPOINT ["sleep", "infinity"]
