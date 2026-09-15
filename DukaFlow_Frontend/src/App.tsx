import { Navigate, Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './auth/ProtectedRoute'
import { DashboardLayout } from './layouts/DashboardLayout'
import { LoginPage } from './pages/Login/LoginPage'
import { RegisterPage } from './pages/Register/RegisterPage'
import { DashboardPage } from './pages/Dashboard/DashboardPage'
import { SettingsPage } from './pages/Settings/SettingsPage'
import { MenuCategoriesPage } from './pages/Menu/MenuCategoriesPage'
import { MenuItemsPage } from './pages/Menu/MenuItemsPage'

// AuthProvider lives in main.tsx, wrapping this component once at the root.

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />

      <Route
        element={
          <ProtectedRoute>
            <DashboardLayout />
          </ProtectedRoute>
        }
      >
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/menu" element={<Navigate to="/menu/categories" replace />} />
        <Route path="/menu/categories" element={<MenuCategoriesPage />} />
        <Route path="/menu/items" element={<MenuItemsPage />} />
        <Route path="/settings" element={<SettingsPage />} />
      </Route>

      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  )
}

export default App
