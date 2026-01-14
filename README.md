# MAG.TOF - Time-Off Request Management System

A modern web application built with Blazor Server and MudBlazor for managing employee time-off requests with approval workflows.

## 🎯 Overview

MAG.TOF (Time-Off) is an enterprise-grade time-off request management system that enables employees to submit time-off requests and managers to review and approve them. The application features role-based dashboards, real-time notifications, and reporting capabilities.

## ✨ Key Features

- **Employee Self-Service**
  - Submit new time-off requests
  - Track personal request history
  - View request status in real-time
  - Dashboard with pending and approved requests overview

- **Manager Dashboard**
  - Review and approve/reject pending requests
  - Multi-manager support with role-based access

- **Authentication & Authorization**
  - Secure user authentication
  - Role-based access control (Employee, Manager, Admin)
  - Account management features

- **Reports & Analytics**
  - Generate time-off reports
  - Export capabilities

## 🏗️ Architecture

The project follows Clean Architecture principles with clear separation of concerns:

```
MAG.TOF/
│
├── MAG.TOF.Domain/          # Core business logic and entities
│   ├── Entities/            # Domain entities
│   ├── Enums/              # Domain enumerations
│   └── Services/           # Domain services
│
├── MAG.TOF.Application/     # Application business rules
│   ├── CQRS/               # Commands and Queries (MediatR)
│   ├── DTOs/               # Data Transfer Objects
│   └── Interfaces/         # Application interfaces
│
├── MAG.TOF.Infrastructure/  # External concerns
│   └── Services/           # Infrastructure services (Cache, etc.)
│
└── MAG.TOF.Web/            # Presentation layer (Blazor Server)
    ├── Components/
    │   ├── Pages/          # Page components
    │   ├── Layout/         # Layout components
    │   └── Account/        # Authentication pages
    └── Endpoints/          # API endpoints
```

## 🛠️ Technology Stack

- **.NET 10** - Latest .NET framework
- **C# 14.0** - Modern C# features
- **Blazor Server** - Interactive web UI framework
- **MudBlazor** - Material Design component library
- **MediatR** - CQRS pattern implementation
- **ASP.NET Core Identity** - Authentication and authorization

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022 (17.12+) or Visual Studio Code
- SQL Server (or SQL Server Express)

## 📖 Usage

### For Employees

1. **Register/Login** - Create an account or log in
2. **Submit Request** - Click "New Request" button in the navigation menu
3. **Track Requests** - View your requests under "My Requests"
4. **Dashboard** - View your time-off summary on the main dashboard

### For Managers

1. **Manager Dashboard** - Access pending requests for your team
2. **Review Requests** - Approve or reject time-off requests
3. **Reports** - Generate reports for team availability

## 🔧 Configuration

Key configuration options in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your database connection string"
  },
  "Authentication": {
    // Authentication settings
  }
}
```

## 🧪 Build and Test

Build the solution:

```bash
dotnet build
```

Run the test suite:

```bash
dotnet test
```

## 📦 Deployment

### Azure Deployment

The application is configured for Azure DevOps CI/CD:

1. Builds are automatically triggered on push to main branches
2. Tests run as part of the CI pipeline
3. Deployment to staging/production environments is managed through Azure DevOps

## 🤝 Contributing

1. Create a feature branch from `main`
2. Make your changes following the existing code style
3. Ensure all tests pass
4. Submit a pull request

### Coding Standards

- Follow Clean Architecture principles
- Use CQRS pattern for business operations
- Write unit tests for new features
- Follow existing naming conventions
- Add XML documentation for public APIs

## 📝 Project Structure Details

### Domain Layer
- **Entities**: Core business objects (Request, User, Approval)
- **Enums**: Request status, approval status, etc.
- **Services**: Business logic validation

### Application Layer
- **CQRS**: Separates read and write operations
- **Queries**: Get pending requests, get user requests, etc.
- **Commands**: Create request, approve request, reject request
- **DTOs**: Data contracts for API and UI

### Infrastructure Layer
- **Persistence**: Database context and repositories
- **Services**: Caching, external data services
- **Identity**: User authentication implementation

### Web Layer
- **Pages**: Blazor page components (Dashboard, Manager Dashboard, Reports)
- **Layout**: Navigation menu, main layout
- **Account**: Login, register, account management
- **Endpoints**: API endpoints for external access


## 📚 Additional Resources

- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor/)
- [MudBlazor Components](https://mudblazor.com/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://docs.microsoft.com/azure/architecture/patterns/cqrs)

---

**Version**: 1.0.0  
**Last Updated**: 2024