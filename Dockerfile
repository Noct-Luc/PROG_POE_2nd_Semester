# Multi-stage build for ASP.NET Core 8 MVC application
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["RaceDay.MVC/RaceDay.MVC.csproj", "RaceDay.MVC/"]

# Restore dependencies
RUN dotnet restore "RaceDay.MVC/RaceDay.MVC.csproj"

# Copy application code
COPY . .

# Build the application
RUN dotnet build "RaceDay.MVC/RaceDay.MVC.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "RaceDay.MVC/RaceDay.MVC.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Copy published application from build stage
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "RaceDay.MVC.dll"]