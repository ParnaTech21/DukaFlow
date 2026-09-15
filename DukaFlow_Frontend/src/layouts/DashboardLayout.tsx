import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

const navItems = [
  { label: 'Dashboard', to: '/dashboard', enabled: true },
  { label: 'Menu', to: '/menu', enabled: true },
  { label: 'Orders', to: '/orders', enabled: false },
  { label: 'Customers', to: '/customers', enabled: false },
  { label: 'Payments', to: '/payments', enabled: false },
  { label: 'Settings', to: '/settings', enabled: true },
]

export function DashboardLayout() {
  const { user, restaurant, logout } = useAuth()

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="flex items-center justify-between border-b border-slate-200 bg-white px-6 py-3">
        <span className="text-lg font-semibold text-brand-600">DukaFlow</span>
        <div className="flex items-center gap-3 text-sm text-slate-600">
          <span>{user?.firstName ?? 'Account'}</span>
          <button
            onClick={logout}
            className="rounded-md border border-slate-200 px-3 py-1 text-slate-600 hover:bg-slate-100"
          >
            Log out
          </button>
        </div>
      </header>

      <div className="flex">
        <aside className="w-56 shrink-0 border-r border-slate-200 bg-white p-4">
          <nav className="flex flex-col gap-1">
            {navItems.map((item) =>
              item.enabled ? (
                <NavLink
                  key={item.label}
                  to={item.to}
                  className={({ isActive }) =>
                    `rounded-md px-3 py-2 text-sm font-medium ${
                      isActive ? 'bg-brand-50 text-brand-700' : 'text-slate-600 hover:bg-slate-100'
                    }`
                  }
                >
                  {item.label}
                </NavLink>
              ) : (
                <span
                  key={item.label}
                  className="cursor-not-allowed rounded-md px-3 py-2 text-sm text-slate-300"
                  title="Coming in a later phase"
                >
                  {item.label}
                </span>
              )
            )}
          </nav>
        </aside>

        <main className="flex-1 p-8">
          <p className="mb-4 text-sm text-slate-500">
            Restaurant: <span className="font-medium text-slate-700">{restaurant?.name}</span>
          </p>
          <Outlet />
        </main>
      </div>
    </div>
  )
}
