# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore WebApi.sln
WORKDIR /src/WebApi.Client
RUN dotnet publish WebApi.Client.csproj -c Release -o /publish -p:DebugType=none -p:DebugSymbols=false -p:IsContainerBuild=true

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /publish .
COPY --from=build /src/WebApi.Client/assets/https/*.crt /usr/local/share/ca-certificates/
RUN apk add --no-cache ca-certificates
RUN update-ca-certificates
ENTRYPOINT ["sleep", "infinity"]