# **Stage 1: Build**
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Projektdateien kopieren und Wiederherstellen der Abhängigkeiten (NuGet-Pakete) als separater Schritt.
COPY *.csproj ./
RUN dotnet restore

# Restlichen Quellcode kopieren
COPY . . 

# Anwendung im Release-Modus veröffentlichen (Publish). 
# --no-restore stellt sicher, dass nicht erneut versucht wird, NuGet-Pakete zu laden (bereits im vorherigen Schritt erledigt).
RUN dotnet publish -c Release --no-restore -o /app/publish

# **Stage 2: Runtime**
# Verwende ein schlankes Distroless Runtime-Image für .NET 9 (ASP.NET Core). 
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled AS final
WORKDIR /app

# Setze Default-Environment auf Production
ENV ASPNETCORE_ENVIRONMENT=Production

# Kopiere die veröffentlichten Dateien aus dem Build-Stage in das Runtime-Image.
COPY --from=build /app/publish . 

# Exponiere Port 8080 im Container
EXPOSE 8080

# Startbefehl: Anwendung starten. 
ENTRYPOINT ["dotnet", "ChatbotService.dll"]