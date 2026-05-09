# syntax=docker/dockerfile:1

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all source files
COPY src/ ./src/

# Restore dependencies
RUN dotnet restore "src/InsuranceManager.Api/InsuranceManager.Api.csproj"

# Build the application
RUN dotnet build "src/InsuranceManager.Api/InsuranceManager.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "src/InsuranceManager.Api/InsuranceManager.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && \
    chown -R appuser /app

USER appuser

# Copy published output
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --retries=3 CMD curl -f http://localhost:8080/health || exit 1

# Expose port (ASPNETCORE_URLS env var controls this)
EXPOSE 8080

# Environment configuration
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "InsuranceManager.Api.dll"]