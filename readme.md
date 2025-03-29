# Chatbot Service

A simple ASP.NET Core web service with a `/hello` endpoint returning "World!", Dockerized for easy deployment.

![.NET Version](https://img.shields.io/badge/.NET-9.0-512bd4?logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-✓-blue?logo=docker)
![Docker Compose](https://img.shields.io/badge/Docker_Compose-✓-blue?logo=docker)

## Features
- Hugging Face LLM Integration (FLAN-T5 model)
- Docker Compose support
- ASP.NET Core 9.0
- Distroless container image
- Environment-specific configurations (Development/Production)

## Getting Started

### Prerequisites
- [.NET SDK 9.0](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/)

### Environment Configuration
The service supports two environments:
- Production (default)
- Development: verbose logging

### Build Locally
`dotnet run`

### Build and run Image manually
Build Image: `docker build -t chatbotservice .`  
Run in Production (default): `docker run -p 8080:8080 chatbotservice`  
Run in Development: `docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development chatbotservice`

### Build and run with Docker Compose
**Production**  
`docker-compose up --build`

**Development**
- Linux/maxOS: `ENVIRONMENT=Development docker-compose up --build`
- Windows: `$env:ENVIRONMENT="Development"; docker-compose up --build`

Visit http://localhost:8080/hello in your browser or  
http://localhost:8080/env to verify the environment or  
test chat endpoint:  http://localhost:8080/chat?message=What%is%the%capital%of%France?