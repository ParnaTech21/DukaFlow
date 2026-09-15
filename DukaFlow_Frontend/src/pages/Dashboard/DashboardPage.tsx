import { useAuth } from '../../auth/AuthContext'

interface ChecklistItem {
  label: string
  done: boolean
}

const checklist: ChecklistItem[] = [
  { label: 'Create account', done: true },
  { label: 'Restaurant profile', done: true },
  { label: 'Add your menu', done: false },
  { label: 'Connect WhatsApp', done: false },
  { label: 'Receive your first order', done: false },
]

export function DashboardPage() {
  const { user } = useAuth()

  return (
    <div className="max-w-2xl">
      <h1 className="mb-1 text-2xl font-semibold text-slate-900">
        Welcome to DukaFlow, {user?.firstName}
      </h1>
      <p className="mb-8 text-slate-500">You're setting up your restaurant on DukaFlow.</p>

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
              <span className={item.done ? 'text-slate-700' : 'text-slate-400'}>{item.label}</span>
            </li>
          ))}
        </ul>
      </div>
    </div>
  )
}
