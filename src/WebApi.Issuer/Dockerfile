# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore WebApi.sln
WORKDIR /src/WebApi.Issuer
RUN dotnet publish WebApi.Issuer.csproj -c Release -o /publish /p:DebugType=none /p:DebugSymbols=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
COPY --from=build /src/setup/extra-runtime-setup.sh /usr/local/bin/extra-runtime-setup.sh
RUN chmod +x /usr/local/bin/extra-runtime-setup.sh
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["bash","-c","extra-runtime-setup.sh && exec dotnet WebApi.Issuer.dll"]
