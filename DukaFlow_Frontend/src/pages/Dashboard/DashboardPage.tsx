import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../../auth/AuthContext'
import { menuApi } from '../../services/menuApi'

export function DashboardPage() {
  const { user, restaurant } = useAuth()
  const [categoryCount, setCategoryCount] = useState<number | null>(null)
  const [itemCount, setItemCount] = useState<number | null>(null)
  const [unavailableCount, setUnavailableCount] = useState<number | null>(null)

  useEffect(() => {
    Promise.all([menuApi.getCategories(), menuApi.getItems()])
      .then(([categories, items]) => {
        setCategoryCount(categories.length)
        setItemCount(items.length)
        setUnavailableCount(items.filter((i) => !i.isAvailable).length)
      })
      .catch(() => {
        // Non-critical for the dashboard; leave counts as "..." rather than blocking the page.
      })
  }, [])

  const hasMenu = (categoryCount ?? 0) > 0 && (itemCount ?? 0) > 0

  const checklist = [
    { label: 'Create account', done: true },
    { label: 'Restaurant profile', done: !!restaurant },
    { label: 'Add your menu', done: hasMenu, to: '/menu/categories' },
    { label: 'Connect WhatsApp', done: false },
    { label: 'Receive your first order', done: false },
  ]

  return (
    <div className="max-w-2xl">
      <h1 className="mb-1 text-2xl font-semibold text-slate-900">
        Welcome to DukaFlow, {user?.firstName}
      </h1>
      <p className="mb-8 text-slate-500">
        You're setting up {restaurant?.name ?? 'your restaurant'} on DukaFlow.
      </p>

      <div className="mb-6 grid grid-cols-3 gap-4">
        <div className="rounded-lg border border-slate-200 bg-white p-4">
          <p className="text-xs font-medium uppercase tracking-wide text-slate-500">Categories</p>
          <p className="mt-1 text-2xl font-semibold text-slate-900">{categoryCount ?? '...'}</p>
        </div>
        <div className="rounded-lg border border-slate-200 bg-white p-4">
          <p className="text-xs font-medium uppercase tracking-wide text-slate-500">Menu items</p>
          <p className="mt-1 text-2xl font-semibold text-slate-900">{itemCount ?? '...'}</p>
        </div>
        <div className="rounded-lg border border-slate-200 bg-white p-4">
          <p className="text-xs font-medium uppercase tracking-wide text-slate-500">Unavailable</p>
          <p className="mt-1 text-2xl font-semibold text-slate-900">{unavailableCount ?? '...'}</p>
        </div>
      </div>

      <div className="rounded-lg border border-slate-200 bg-white p-6">
        <h2 className="mb-4 text-sm font-semibold uppercase tracking-wide text-slate-500">
          Setup progress
        </h2>
        <ul className="flex flex-col gap-3">
          {checklist.map((item) => (
            <li key={item.label} className="flex items-center gap-3 text-sm">
              <span
                className={`flex h-5 w-5 items-center justify-center rounded-full text-xs ${
                  item.done ? 'bg-brand-500 text-white' : 'border border-slate-300 text-transparent'
                }`}
              >
                ✓
              </span>
              {item.to && !item.done ? (
                <Link to={item.to} className="text-brand-600 hover:underline">
                  {item.label}
                </Link>
              ) : (
                <span className={item.done ? 'text-slate-700' : 'text-slate-400'}>{item.label}</span>
              )}
            </li>
          ))}
        </ul>
      </div>

      <p className="mt-6 text-sm text-slate-400">
        Ordering system status: Not yet enabled · WhatsApp integration: Coming in a later phase
      </p>
    </div>
  )
}
