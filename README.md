# Billetterie-Spectacles

Online ticketing platform for shows and cultural events.

**EPSI Academic Project**

---

## Table of Contents

- [Technologies](#technologies)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [API Documentation](#api-documentation)
- [Database](#database)

---

## Technologies

### Backend
- **Framework**: .NET 8.0
- **Architecture**: Clean Architecture (4 layers)
- **ORM**: Entity Framework Core 8
- **Database**: SQL Server Express (dev) / Azure SQL Database (prod)
- **Authentication**: JWT Bearer Token
- **API Documentation**: Swagger/OpenAPI
- **Payment**: Stripe API (dedicated microservice)

### Frontend *(developed by another team member)*
- Modern framework (React/Angular/Vue)
- REST communication with API

### Infrastructure
- **Hosting**: Azure App Service
- **Database**: Azure SQL Database
- **Version Control**: Git (Azure DevOps)
- **CI/CD**: Azure Pipelines *(coming soon)*

---

## Prerequisites

### Required
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server Express 2017+](https://www.microsoft.com/sql-server/sql-server-downloads) or full version
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Recommended
- [SQL Server Management Studio (SSMS)](https://docs.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms) or [Azure Data Studio](https://docs.microsoft.com/sql/azure-data-studio/download-azure-data-studio)
- [Postman](https://www.postman.com/) or equivalent (for API testing)

---

## Installation

### 1. Clone the repository
```bash
git clone https://dev.azure.com/your-organization/Billetterie-Spectacles/_git/Billetterie-Spectacles
cd Billetterie-Spectacles
```

### 2. Restore dependencies
```bash
dotnet restore
```

### 3. Configure the database

#### Option A: Use SQL Server Express (local)

The database will be created automatically on first launch via Entity Framework migrations.

#### Option B: Create database manually
```sql
CREATE DATABASE BilleterieSpectacles;
```

### 4. Apply migrations
```bash
cd Billetterie-Spectacles.Presentation
dotnet ef database update
```

---

## Configuration

### Secrets and Sensitive Data

**IMPORTANT: This project uses User Secrets to protect sensitive data in development.**

Secrets are **never** versioned in Git. Each developer must configure their own local secrets.

#### Initialize User Secrets
```bash
# In the Billetterie-Spectacles.Presentation folder
dotnet user-secrets init
```

Or via Visual Studio: 
- Right-click on Presentation project → **Manage User Secrets**

#### Configure required secrets
```bash
# 1. JWT Secret Key (ask team for the value to ensure token compatibility)
dotnet user-secrets set "JwtSettings:SecretKey" "VALUE_TO_GET_FROM_TEAM"

# 2. SQL Server connection string (adapt to your local instance)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\\SQLEXPRESS;Database=BilleterieSpectacles;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"

# 3. Stripe API key (test mode) - optional if testing payments
dotnet user-secrets set "Stripe:SecretKey" "sk_test_YOUR_TEST_KEY"
```

#### Verify configuration
```bash
dotnet user-secrets list
```

You should see your secrets listed (values will be displayed in plain text).

### 📝 Configuration Files

| File | Content | Versioned? |
|------|---------|------------|
| `appsettings.json` | Common configuration (public) | Yes |
| `appsettings.Development.json` | Development overrides | Yes |
| `appsettings.Production.json` | Production overrides | Yes |
| `secrets.json` (User Secrets) | Local secrets (JWT, DB, API keys) |  No |

---

## 🎮 Usage

### Run the application

#### Via Visual Studio
1. Open solution `Billetterie-Spectacles.sln`
2. Set `Billetterie-Spectacles.Presentation` as startup project
3. Select **"http (localhost)"** profile in dropdown menu
4. Press `F5` or click ▶️

#### Via CLI
```bash
cd Billetterie-Spectacles.Presentation
dotnet run
```

### Access the application

- **API**: http://localhost:5293
- **Swagger UI**: http://localhost:5293/swagger

### Available launch profiles

- **http (localhost)**: For Swagger and classic API development
- **http (WSL compatible)**: To allow access from frontend in WSL (listens on `0.0.0.0`)

---

## 📖 API Documentation

### Swagger/OpenAPI

Interactive documentation available at: **http://localhost:5293/swagger**

### Authentication

The API uses JWT Bearer Token for authentication.

#### 1. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}
```

#### 2. Get JWT token
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiration": "2024-01-15T10:30:00Z"
}
```

#### 3. Use token in requests
```http
GET /api/spectacles
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

---

## 🗄️ Database

### Entity Framework Migrations
```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Data Seeding

On first launch, the application automatically creates:
- A default administrator account
- Demo shows and performances
- Show categories

---

**Last updated:** February 2025