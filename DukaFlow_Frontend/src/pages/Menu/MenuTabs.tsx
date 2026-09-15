import { NavLink } from 'react-router-dom'

export function MenuTabs() {
  return (
    <div className="mb-6 flex gap-1 border-b border-slate-200">
      <NavLink
        to="/menu/categories"
        className={({ isActive }) =>
          `px-4 py-2 text-sm font-medium ${
            isActive
              ? 'border-b-2 border-brand-500 text-brand-700'
              : 'text-slate-500 hover:text-slate-700'
          }`
        }
      >
        Categories
      </NavLink>
      <NavLink
        to="/menu/items"
        className={({ isActive }) =>
          `px-4 py-2 text-sm font-medium ${
            isActive
              ? 'border-b-2 border-brand-500 text-brand-700'
              : 'text-slate-500 hover:text-slate-700'
          }`
        }
      >
        Items
      </NavLink>
    </div>
  )
}
