# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/PodMD.Api/PodMD.Api.csproj", "src/PodMD.Api/"]
COPY ["src/PodMD.Application/PodMD.Application.csproj", "src/PodMD.Application/"]
COPY ["src/PodMD.Domain/PodMD.Domain.csproj", "src/PodMD.Domain/"]
COPY ["src/PodMD.Infrastructure/PodMD.Infrastructure.csproj", "src/PodMD.Infrastructure/"]
COPY ["src/PodMD.Shared/PodMD.Shared.csproj", "src/PodMD.Shared/"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

RUN dotnet restore "src/PodMD.Api/PodMD.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/PodMD.Api"
RUN dotnet build "PodMD.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PodMD.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "PodMD.Api.dll"]
