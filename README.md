# DukaFlow

> **A simple WhatsApp-first ordering and business management platform for small businesses.**

DukaFlow helps small businesses, starting with **restaurants**, manage customer orders, menus, payments, and daily operations through one simple platform.

Instead of relying on scattered WhatsApp conversations, phone calls, and manual order tracking, DukaFlow brings the process into a structured system.

---

## 💡 The Problem

Many small restaurants rely heavily on WhatsApp to communicate with customers and receive orders.

This can lead to:

* Lost or forgotten orders
* Manual order-taking
* Difficulty managing menus and prices
* Confusion when handling multiple customers
* Payment tracking challenges
* Lack of a simple system for managing orders

Existing business management systems can also be too complex or expensive for small businesses.

---

## 🚀 The Solution

DukaFlow provides a simple digital workflow:

```text
Customer
   ↓
WhatsApp
   ↓
Browse Menu
   ↓
Add to Cart
   ↓
Place Order
   ↓
Payment
   ↓
Restaurant Dashboard
   ↓
Order Fulfillment
```

Restaurant owners can manage their business from a web dashboard, while customers can interact with the restaurant through WhatsApp.

The goal is to make digital ordering **simple, affordable, and accessible to small businesses**.

---

## ✨ Core Features

* 🏪 Restaurant management
* 🍔 Menu and category management
* 🛒 Customer cart and ordering
* 💬 WhatsApp ordering
* 💳 Mobile Money payments
* 📦 Order management
* 👥 Customer management
* 📊 Basic business operations dashboard

> Features are being implemented incrementally as part of the DukaFlow MVP roadmap.

---

## 🛠️ Technology Stack

### Frontend

* React
* TypeScript
* Vite
* Tailwind CSS

### Backend

* C#
* ASP.NET Core Web API
* Entity Framework Core
* Microsoft SQL Server
* JWT Authentication
* Swagger / OpenAPI

### Development

* Visual Studio Community 2022
* SQL Server Management Studio
* Git & GitHub
* Postman

---

## 🏗️ Architecture

DukaFlow is initially being developed as a **modular monolith**.

```text
React + TypeScript
       ↓
ASP.NET Core Web API
       ↓
Entity Framework Core
       ↓
SQL Server
```

External services such as WhatsApp and payment providers are integrated through dedicated application/infrastructure abstractions.

This keeps the core business logic independent from external platforms.

---

## 🗺️ MVP Roadmap

```text
Phase 1 → Foundation
Phase 2 → Restaurant & Menu Management
Phase 3 → Ordering Engine
Phase 4 → WhatsApp Integration
Phase 5 → Payments
Phase 6 → Production & Pilot
```

**Current status:** 🚧 In Development

---

## 📸 Screenshots & Demo

> Screenshots, UI previews, architecture diagrams, and demo videos will be added as development progresses.

<!-- Future screenshots can be added here -->

---

## 🎯 Initial Target

DukaFlow is initially being developed for **small restaurants in Uganda**, with the architecture designed to support additional businesses and services in the future.

Potential future expansion may include:

* Retail businesses
* Delivery services
* Ride/transport ordering
* Healthcare services
* Other small businesses

The initial focus remains **restaurants**.

---

## 📚 Documentation

More detailed technical documentation is available in:

* [`backend/README.md`](backend/README.md) — Backend setup and architecture
* [`frontend/README.md`](frontend/README.md) — Frontend setup and architecture
* `docs/` — Additional project documentation

---

## 📌 Project Status

DukaFlow is currently under active development.

The project will evolve through development, testing, and feedback from real businesses.

---

## 👨‍💻 Development

Built with a focus on:

**Simplicity • Reliability • Scalability • Real-world usability**

---
