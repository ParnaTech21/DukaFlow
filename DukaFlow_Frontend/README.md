# DukaFlow Frontend

The DukaFlow frontend is the web-based dashboard used by restaurant owners and staff to manage their business.

It communicates with the DukaFlow ASP.NET Core Web API.

---

## 🛠️ Technology Stack

* **React**
* **TypeScript**
* **Vite**
* **Tailwind CSS**
* **REST API**
* **Axios** or the project's centralized HTTP client
* **Git & GitHub**

---

## 🎯 Purpose

The dashboard allows restaurant users to manage:

* Restaurant information
* Menu categories
* Menu items
* Prices
* Item availability
* Customer orders
* Payments
* Conversations
* Business operations

Features will be introduced progressively throughout the DukaFlow roadmap.

---

## 🏗️ Architecture

The frontend follows a feature-oriented React structure.

```text
src/
├── api/
├── auth/
├── components/
├── features/
│   ├── restaurant/
│   ├── menu/
│   ├── orders/
│   ├── payments/
│   └── conversations/
├── hooks/
├── pages/
├── routes/
├── types/
└── App.tsx
```

The exact structure may evolve as the application grows.

---

## 🔗 Backend Communication

The frontend communicates with the ASP.NET Core API.

```text
React
   ↓
API Client
   ↓
ASP.NET Core Web API
   ↓
SQL Server
```

API calls should be centralized rather than scattered throughout UI components.

---

## 🔐 Authentication

The frontend uses the backend's JWT authentication system.

Authentication-related functionality includes:

* Registration
* Login
* Logout
* Protected routes
* Current-user state
* Authorized API requests

Protected application areas should not be accessible to unauthenticated users.

---

## 🖥️ Main Dashboard Areas

The dashboard will progressively include:

```text
Dashboard
├── Restaurant
├── Menu
│   ├── Categories
│   └── Items
├── Orders
├── Payments
├── Conversations
└── Settings
```

Only functionality belonging to the current development phase should be implemented.

---

## ▶️ Running the Frontend

Navigate to:

```bash
cd frontend/dukaflow-web
```

Install dependencies:

```bash
npm install
```

Start the development server:

```bash
npm run dev
```

Build for production:

```bash
npm run build
```

---

## ⚙️ Environment Configuration

Frontend environment variables should contain configuration such as the API base URL.

Example:

```text
VITE_API_BASE_URL=...
```

Do not place secrets in frontend environment variables.

Anything shipped to a React frontend should be considered publicly accessible.

---

## 🎨 UI Principles

The dashboard should prioritize:

* simplicity,
* clear navigation,
* responsive design,
* accessibility,
* consistent components,
* readable typography,
* useful feedback states,
* mobile-friendly layouts.

Restaurant staff should be able to understand the interface without technical knowledge.

---

## 🧪 Frontend Testing

Where appropriate, test:

* authentication flows,
* protected routes,
* forms,
* menu management,
* order interfaces,
* API error states,
* loading states,
* important user workflows.

---

## 📸 Screenshots

Screenshots and UI previews will be added as the dashboard develops.

Future documentation may include:

```text
Dashboard
Menu Management
Order Management
WhatsApp Order Flow
Payment Flow
```

---

## 📁 Frontend Development Principles

* Use TypeScript properly.
* Avoid unnecessary `any`.
* Keep components focused.
* Reuse common UI components.
* Centralize API communication.
* Handle loading/error/empty states.
* Do not duplicate business logic that belongs to the backend.
* Do not trust frontend calculations for critical business values.
* Keep the UI consistent across features.
