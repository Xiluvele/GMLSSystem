# Global Logistics Management System (GLMS)

## Overview

The Global Logistics Management System (GLMS) is a multi-layered enterprise application developed for TechMove Logistics using ASP.NET Core MVC, ASP.NET Core Web API, SQL Server, Entity Framework Core, JWT Authentication, Docker, and automated testing.

The system was designed to replace manual logistics processes that relied on spreadsheets, emails, and disconnected workflows. It centralises client management, contract administration, service request processing, document management, currency conversion, authentication, and reporting into a single integrated platform.

The solution follows a service-oriented architecture where the MVC application communicates with a separate Web API, allowing the frontend and backend to operate independently while sharing the same business processes and database.

---

## Features

### Client Management

* Create, update, view, and delete clients
* Store company information and contact details
* Regional client management
* Search and filtering functionality

### Contract Management

* Create and manage contracts
* Upload and view signed PDF agreements
* Contract lifecycle management:

  * Draft
  * Active
  * On Hold
  * Expired
* Advanced contract filtering and search
* Contract details and status tracking

### Service Request Management

* Create service requests linked to active contracts
* Business rule validation
* Service request status management
* Currency conversion support
* Cost tracking in USD and ZAR

### Currency Conversion Integration

* External Exchange Rate API integration
* Automatic USD to ZAR conversion
* Fallback exchange rates when external services are unavailable

### Authentication and Security

* ASP.NET Identity user management
* JWT (JSON Web Token) Authentication
* Role-based authorization
* Protected API endpoints
* User authentication and login services

### API Layer

* Separate ASP.NET Core Web API project
* RESTful API endpoints
* DTO-based communication
* Swagger API documentation and testing
* Secure endpoint access using JWT authentication

### Testing

* Unit testing using xUnit
* API Integration Testing
* Automated endpoint validation
* Authentication testing
* Contract and service request workflow testing

### Containerization

* Docker support
* Docker Compose multi-container deployment
* SQL Server container
* ASP.NET Core Web API container
* ASP.NET Core MVC container

### DevOps and Automation

* GitHub Actions Continuous Integration
* Automated build validation
* Automated test execution
* Source control integration through GitHub

---

## Solution Structure

```text
GMLSSystem
│
├── GMLSSystem                 (ASP.NET Core MVC Frontend)
├── GMLSSystem.API             (ASP.NET Core Web API)
├── GMLSSystem.Shared          (Shared DTOs and Models)
├── GMLSSystem.Tests           (Unit and Integration Tests)
└── docker-compose.yml         (Container Orchestration)
```

---

## Technologies Used

### Backend

* ASP.NET Core 8
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* ASP.NET Identity
* JWT Authentication

### Frontend

* ASP.NET Core MVC
* Razor Views
* Bootstrap 5
* JavaScript
* jQuery

### Testing

* xUnit
* ASP.NET Core Integration Testing
* Microsoft Test Host

### DevOps

* GitHub Actions
* Docker
* Docker Compose
* GitHub

### Additional Technologies

* Swagger / OpenAPI
* LINQ
* REST APIs

---

## Running the Project Locally

### Prerequisites

* Visual Studio 2022
* .NET 8 SDK
* SQL Server
* Docker Desktop (Optional)

### Steps

1. Clone the repository

```bash
git clone <repository-url>
```

2. Open the solution in Visual Studio

3. Restore NuGet packages

```bash
dotnet restore
```

4. Update the database

```bash
dotnet ef database update
```

5. Start both startup projects:

* GMLSSystem.API
* GMLSSystem

6. Run the solution

---

## Running Integration Tests

Using Visual Studio Test Explorer:

* Open Test Explorer
* Run All Tests

Using the command line:

```bash
dotnet test
```

The project includes automated API integration tests covering:

* Authentication
* Contract Management
* Service Requests
* API Endpoints
* Status Updates

---

## Docker Deployment

Build the containers:

```bash
docker compose build
```

Start the containers:

```bash
docker compose up
```

The Docker environment consists of:

* SQL Server Container
* ASP.NET Core Web API Container
* ASP.NET Core MVC Container

Docker Compose automatically creates the network required for communication between the containers.

---

## GitHub Actions CI

The project includes an automated GitHub Actions workflow.

Workflow location:

```text
.github/workflows/dotnet.yml
```

The workflow automatically:

* Restores dependencies
* Builds the solution
* Executes automated tests
* Validates project integrity

---

## API Documentation

Swagger is available when running the API project:

```text
https://localhost:<port>/swagger
```

Swagger can be used to:

* Test API endpoints
* Authenticate using JWT tokens
* Validate API responses
* Review endpoint documentation

---

## Project Demonstration

The project demonstration covers:

* MVC Application Functionality
* Web API Integration
* JWT Authentication
* Swagger API Testing
* Automated Integration Testing
* Docker Containerization
* GitHub Actions CI Workflow

---

## License

This project was developed for academic and educational purposes.
