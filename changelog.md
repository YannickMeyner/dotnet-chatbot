# Changelog  
Alle wichtigen Änderungen an diesem Projekt werden in dieser Datei dokumentiert.  
Format basiert auf [Keep a Changelog](https://keepachangelog.com/de/1.1.0/).

## [1.0.1] - 2024-05-13
### Added
- Liveness-Probe unter `/liveness`
- Readiness-Probe unter `/readiness`, die den Hugging Face API-Key prüft

## [1.0.0] - 07-04-2025
### Added
- Anbindung an HuggingFace (LLM).
- Bereitstellung des `/chat`-Endpunkts.
- Integration von CI/CD.
- Logging.

### Changed  
*(Keine Änderungen in dieser Version)*  

### Fixed  
*(Keine Fehlerbehebungen in dieser Version)*

### Build  
- Neue HuggingFace API-Key Variable in `docker-compose.yaml` hinzugefügt.

### Documentation 
- Beschreibung von `README.md` aktualisiert (chat-Endpunkt).


## [0.1.0] - 14-03-2025
### Added  
- Initiale Implementierung des `/hello`-Endpunkts.  

### Changed  
*(Keine Änderungen in dieser Version)*  

### Fixed  
*(Keine Fehlerbehebungen in dieser Version)*  

### Build  
- Docker-Infrastruktur mit Multi-Stage-Build und `docker-compose.yaml` hinzugefügt.  

### Documentation  
- Grundlegendes `README.md` zur Projekteinrichtung erstellt.  
- Kommentare in `Program.cs` hinzugefügt.  

### Chores  
- .NET-Projekt initialisiert und `.gitignore`-Datei konfiguriert.