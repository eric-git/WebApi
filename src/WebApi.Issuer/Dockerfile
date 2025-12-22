# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src
COPY . .
RUN cat WebApi.Issuer/assets/https/*.crt > /tmp/custom-ca-bundle.crt
RUN dotnet restore WebApi.sln
WORKDIR /src/WebApi.Issuer
RUN dotnet publish WebApi.Issuer.csproj -c Release -o /publish -p:DebugType=none -p:DebugSymbols=false -p:IsContainerBuild=true

# Stage 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS runtime
COPY --from=build /tmp/custom-ca-bundle.crt /etc/ssl/certs/ca-certificates.crt
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["./WebApi.Issuer"]
