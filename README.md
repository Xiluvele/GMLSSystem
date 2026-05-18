# Global Logistics Management System (GLMS)

## Overview

The Global Logistics Management System (GLMS) is an enterprise-grade ASP.NET Core MVC application developed for TechMove Logistics. The purpose of the system is to replace the company’s outdated legacy workflow that relied on spreadsheets, emails, and manual communication processes.

The system centralises contract management, client management, service request processing, financial operations, and workflow validation into a single modern web application.

---

# Features

## Client Management

* Create and manage clients
* Store contact details and regional information
* Live search functionality

## Contract Management

* Create and manage contracts
* Contract status workflow:

  * Draft
  * Active
  * On Hold
  * Expired
* Upload and download signed PDF agreements
* Contract filtering using LINQ queries

## Service Request Processing

* Create service requests linked to contracts
* Business workflow validation
* Prevent requests for expired or suspended contracts

## Currency Conversion API Integration

* External exchange rate API integration
* Automatic USD to ZAR conversion
* Real-time financial calculations

## Role-Based Authentication & Authorization

Implemented using ASP.NET Identity.

Roles include:

* Administrator
* Contract Manager
* Logistics Staff
* Finance Staff
* Client Users

## Dashboard & Reporting

* Role-based dashboards
* Operational statistics
* Recent activity tracking

## Unit Testing

Implemented using xUnit.

Tests include:

* Currency conversion testing
* File validation testing
* Workflow validation
* Business logic testing
* Edge case testing

## DevOps & Continuous Integration

GitHub Actions CI pipeline configured to:

* Restore dependencies
* Build the solution
* Execute automated unit tests

---

# Technologies Used

* ASP.NET Core MVC
* Entity Framework Core
* SQL Server
* ASP.NET Identity
* LINQ
* xUnit
* Moq
* GitHub Actions
* Bootstrap 5

---

# System Architecture

The application follows the MVC architectural pattern:

* Models handle business entities and database logic
* Views manage the user interface
* Controllers manage application flow and business logic

The project also incorporates enterprise concepts such as:

* TOGAF architecture principles
* Design patterns:

  * Factory Method
  * Abstract Factory
  * Builder Pattern

---

# How to Run the Project

## Prerequisites

* Visual Studio 2022
* .NET 8 SDK
* SQL Server

## Steps

1. Clone the repository
2. Open the solution in Visual Studio
3. Restore NuGet packages
4. Update the database using Entity Framework migrations
5. Run the application

---

# Running Unit Tests

Open Test Explorer in Visual Studio and run all tests.

Or use:

```bash
dotnet test
```

---

# GitHub Actions CI/CD

The project includes automated Continuous Integration using GitHub Actions.

Workflow file location:

```text
.github/workflows/dotnet.yml
```

The pipeline automatically:

* Builds the project
* Runs all unit tests
* Validates project integration

---

# Project Demonstration

An unlisted YouTube demonstration video was created to showcase:

* System functionality
* Workflow logic
* Unit testing
* GitHub Actions CI pipeline
* Application architecture

---

# License

This project was developed for academic purposes.
