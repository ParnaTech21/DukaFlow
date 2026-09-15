# DukaFlow Backend

The DukaFlow backend provides the REST API and core business logic powering the DukaFlow platform.

It is responsible for authentication, restaurant management, menus, orders, payments, WhatsApp integration, and other backend services.

---

## 🛠️ Technology Stack

* **C#**
* **ASP.NET Core Web API**
* **Entity Framework Core**
* **Microsoft SQL Server**
* **JWT Authentication**
* **Swagger / OpenAPI**
* **Visual Studio Community 2022**

---

## 🏗️ Architecture

DukaFlow uses a **modular monolith architecture**.

```text
DukaFlow.API
      ↓
DukaFlow.Application
      ↓
DukaFlow.Domain
      ↑
DukaFlow.Infrastructure
      ↓
SQL Server
```

### Projects

```text
backend/
├── DukaFlow.sln
│
└── src/
    ├── DukaFlow.API/
    ├── DukaFlow.Application/
    ├── DukaFlow.Domain/
    └── DukaFlow.Infrastructure/
```

### DukaFlow.API

Responsible for:

* Controllers
* HTTP endpoints
* Middleware
* Authentication configuration
* Dependency injection
* Swagger

### DukaFlow.Application

Contains:

* Application services
* DTOs
* Business workflows
* Validation
* Interfaces
* Use cases

### DukaFlow.Domain

Contains the core business entities and rules.

Examples:

```text
User
Restaurant
MenuCategory
MenuItem
Customer
Cart
Order
OrderItem
Payment
Conversation
Message
```

### DukaFlow.Infrastructure

Responsible for:

* Entity Framework Core
* SQL Server
* WhatsApp integration
* Payment provider integrations
* External services
* File storage

---

## 🗄️ Database

DukaFlow uses:

**Microsoft SQL Server**

Entity Framework Core is used as the ORM.

Development database tools:

* SQL Server
* SQL Server Management Studio
* EF Core migrations

Example:

```powershell
Add-Migration InitialCreate
Update-Database
```

---

## 🔐 Authentication

The API uses JWT-based authentication.

Initial roles include:

```text
RestaurantOwner
RestaurantStaff
PlatformAdmin
```

Restaurant data is isolated using tenant-aware authorization.

A restaurant must never be able to access another restaurant's data.

---

## ▶️ Running the Backend

Open:

```text
backend/DukaFlow.sln
```

using:

**Visual Studio Community 2022**

Set:

```text
DukaFlow.API
```

as the startup project.

Configure the SQL Server connection string and required development secrets.

Then run the API using the configured HTTPS profile.

Swagger is available during development for testing the API.

---

## 🔑 Configuration

Sensitive configuration should not be committed to Git.

Examples include:

```text
Database connection strings
JWT signing keys
WhatsApp credentials
Payment provider credentials
API keys
```

Use .NET User Secrets for local development and secure environment configuration for production.

---

## 🧪 Testing

Backend testing includes:

* Unit tests
* Integration tests
* Authentication tests
* Authorization tests
* Tenant isolation tests
* Business logic tests

Test projects:

```text
tests/
├── DukaFlow.UnitTests/
└── DukaFlow.IntegrationTests/
```

---

## 📁 Backend Development Principles

* Keep controllers thin.
* Use DTOs.
* Keep business logic out of controllers.
* Validate all external input.
* Use `decimal` for monetary values.
* Use asynchronous database operations.
* Protect tenant boundaries.
* Never expose secrets.
* Do not trust client-side totals.
* Keep external integrations behind abstractions.
* Avoid premature complexity.

---

## 🔌 External Integrations

External services should be isolated from the core application.

Examples:

```text
WhatsApp
   ↓
WhatsApp Adapter
   ↓
Application Layer
```

and:

```text
Payment Service
   ↓
IPaymentProvider
   ├── MTN
   └── Airtel
```

This allows external providers to change without rewriting the core ordering system.
