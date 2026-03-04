
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src


COPY ["NuzlockeTrackerWeb.csproj", "./"]
RUN dotnet restore "NuzlockeTrackerWeb.csproj"


COPY . .
RUN dotnet publish "NuzlockeTrackerWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Port configuration
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "NuzlockeTrackerWeb.dll"]