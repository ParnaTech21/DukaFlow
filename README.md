# DukaFlow

> **A WhatsApp-first ordering and business management platform for small businesses.**

DukaFlow helps small businesses manage customer orders, menus, payments, and daily operations through a simple web dashboard, while allowing customers to interact with businesses through **WhatsApp**.

The initial focus is on **small restaurants**, with the platform designed to expand to other small-business use cases in the future.

---

## 🚀 What Problem Does DukaFlow Solve?

Many small restaurants rely heavily on **WhatsApp, phone calls, and manual processes** to receive and manage customer orders.

This creates problems such as:

* Orders getting lost in conversations
* Customers struggling to view available menu items
* Manual calculation of order totals
* Difficulty tracking order status
* Payment confirmation problems
* Restaurant staff having no centralized order dashboard
* Repetitive work when responding to customers
* Limited digital tools designed specifically for small businesses

For a small restaurant, adopting a large and complicated business management system can also be expensive and difficult.

---

## 💡 The Solution

DukaFlow brings these processes together into one simple platform.

```text
Customer
   ↓
WhatsApp
   ↓
DukaFlow
   ↓
Restaurant Menu
   ↓
Cart
   ↓
Order
   ↓
Payment
   ↓
Restaurant Dashboard
```

A customer can interact with a restaurant through WhatsApp, browse its menu, build an order, provide the required information, and eventually pay using supported payment methods.

The restaurant receives and manages the order from the DukaFlow dashboard.

---

## 🍽️ What Can DukaFlow Do?

The initial platform is being developed around these capabilities:

### Restaurant Management

Restaurants can manage:

* Business profile
* Contact information
* Operating information
* Restaurant settings

### Menu Management

Restaurants can:

* Create menu categories
* Add menu items
* Set prices
* Add descriptions
* Add images
* Mark items as available/unavailable

### Ordering

Customers can:

* Browse the menu
* Select items
* Choose quantities
* Review their cart
* Place orders
* Receive order confirmations

Restaurants can:

* View incoming orders
* Confirm or reject orders
* Update order status
* View order history

### WhatsApp

WhatsApp will be the primary customer-facing channel for the MVP.

The goal is to make ordering as simple as:

```text
"Hi"
   ↓
View Menu
   ↓
Choose Food
   ↓
Add to Cart
   ↓
Checkout
   ↓
Pay
   ↓
Order Confirmed
```

### Payments

The platform is designed to support mobile-money payments, initially targeting providers such as:

* MTN Mobile Money
* Airtel Money

Payment integrations will be implemented through provider-specific adapters so additional providers can be added later.

---

# 🛠️ Technology Stack

DukaFlow is being developed as a **modular monolith** rather than a collection of microservices.

## Backend

* **C#**
* **ASP.NET Core Web API**
* **Entity Framework Core**
* **Microsoft SQL Server**
* **JWT Authentication**
* **Swagger / OpenAPI**

### Development Environment

* **Visual Studio Community 2022**
* SQL Server Management Studio
* Postman
* Git & GitHub

---

## Frontend

* **React**
* **TypeScript**
* **Vite**
* **Tailwind CSS**

The frontend communicates with the ASP.NET Core Web API.

```text
React + TypeScript + Vite
          ↓
   ASP.NET Core API
          ↓
    Entity Framework
          ↓
       SQL Server
```

---

# 🏗️ Architecture

DukaFlow uses a modular monolith architecture to keep the system simple while maintaining clear separation between business areas.

```text
DukaFlow
│
├── Authentication
├── Restaurant Management
├── Menu Management
├── Ordering
├── Conversations
├── WhatsApp Integration
└── Payments
```

External services such as WhatsApp and payment providers are isolated from the core business logic.

For example:

```text
WhatsApp
   ↓
WhatsApp Adapter
   ↓
DukaFlow Ordering Engine
```

This allows the same ordering engine to potentially support other channels in the future.

---

# 📁 Project Structure

```text
DukaFlow/
│
├── backend/
│   ├── DukaFlow.sln
│   └── src/
│       ├── DukaFlow.API/
│       ├── DukaFlow.Application/
│       ├── DukaFlow.Domain/
│       └── DukaFlow.Infrastructure/
│
├── frontend/
│   └── dukaflow-web/
│
├── tests/
│   ├── DukaFlow.UnitTests/
│   └── DukaFlow.IntegrationTests/
│
├── docs/
└── README.md
```

---

# 🗺️ Development Roadmap

DukaFlow is being developed incrementally.

### Phase 1 — Foundation

Authentication, restaurant accounts, database, API foundation and dashboard.

**Status:** 🟡 In Development

### Phase 2 — Restaurant & Menu

Restaurant settings, menu categories, menu items and availability.

**Status:** ⚪ Planned

### Phase 3 — Ordering Engine

Customers, carts, orders, checkout and order management.

**Status:** ⚪ Planned

### Phase 4 — WhatsApp

WhatsApp integration, conversations, menu browsing and WhatsApp ordering.

**Status:** ⚪ Planned

### Phase 5 — Payments

Mobile Money integrations and payment verification.

**Status:** ⚪ Planned

### Phase 6 — Production & Pilot

Deployment, monitoring, security improvements and testing with approximately five restaurants.

**Status:** ⚪ Planned

---

# 📸 Screenshots

Screenshots and product visuals will be added as the application UI develops.

Planned screenshots include:

* Login
* Restaurant dashboard
* Restaurant settings
* Menu management
* Menu preview
* Order management
* Order details
* WhatsApp ordering flow
* Payment flow

Example future structure:

```text
docs/
└── screenshots/
    ├── dashboard.png
    ├── menu-management.png
    ├── orders.png
    └── whatsapp-ordering.png
```

---

# 🎯 MVP Goal

The goal of the first DukaFlow MVP is simple:

> **Help a small restaurant receive and manage customer orders through WhatsApp without relying on scattered chats and manual processes.**

The MVP will be validated with real restaurants before expanding the platform into additional industries and more advanced features.

---

## 📌 Current Focus

**Current phase:** Phase 1 — Foundation

**Initial market:** Small restaurants

**Target market:** Small businesses

**Business model:** Monthly SaaS subscription

**Primary customer channel:** WhatsApp

**Business management interface:** Web dashboard

**Backend:** ASP.NET Core / C# / EF Core / SQL Server

**Frontend:** React / TypeScript / Vite / Tailwind CSS

**IDE:** Visual Studio Community 2022

---

## 📄 Documentation

This README provides the high-level overview of DukaFlow and will evolve as the product develops.
