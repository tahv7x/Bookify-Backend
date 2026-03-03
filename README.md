# Bookify Backend

![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-512BD4?style=for-the-badge&logo=efcore&logoColor=white)
![MySQL](https://img.shields.io/badge/MySQL-4479A1?style=for-the-badge&logo=mysql&logoColor=white)

## Description
RESTful API backend for **Bookify project** built with ASP.NET Core, Entity Framework Core, and MySQL.  
Clean, layered architecture with proper validation, DTOs, and service-based logic.

## Features
- ASP.NET Core Web API
- Entity Framework Core with MySQL
- DTOs for clean data transfer
- Layered architecture (Controllers, Services, Models, Infrastructure)
- Dependency Injection for services
- Migrations for database versioning
- Environment-based configuration (`appsettings.json`, `appsettings.Development.json`, User Secrets)

## Installation
```bash
git clone https://github.com/USERNAME/bookify-backend.git
cd bookify-backend
dotnet restore
