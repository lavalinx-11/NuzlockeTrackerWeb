# Build stage: pull .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["NuzlockeTrackerWeb.csproj", "./"]
RUN dotnet restore "NuzlockeTrackerWeb.csproj"
COPY . .
RUN dotnet build "NuzlockeTrackerWeb.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "NuzlockeTrackerWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage: pull .NET 10 ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NuzlockeTrackerWeb.dll"]