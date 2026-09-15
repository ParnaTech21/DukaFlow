import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useToast } from '../../components/Toast/ToastContext'
import { useAuth } from '../../auth/AuthContext'
import { FormField } from '../../components/FormField'
import type { RegisterRequest } from '../../types/auth'

const initialForm: RegisterRequest = {
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  restaurantName: '',
  phoneNumber: '',
}

export function RegisterPage() {
  const { register } = useAuth()
  const { showToast } = useToast()
  const navigate = useNavigate()
  const [form, setForm] = useState<RegisterRequest>(initialForm)
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  function updateField<K extends keyof RegisterRequest>(field: K, value: string) {
    setForm((prev) => ({ ...prev, [field]: value }))
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setIsSubmitting(true)
    try {
      await register(form)
      showToast('Account created — welcome to DukaFlow!', 'success')
      navigate('/dashboard')
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Could not create your account. Check your details and try again.'
      setError(message)
      showToast(message, 'error')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-50 px-4 py-10">
      <div className="w-full max-w-md rounded-lg border border-slate-200 bg-white p-8 shadow-sm">
        <h1 className="mb-1 text-xl font-semibold text-slate-900">Create your DukaFlow account</h1>
        <p className="mb-6 text-sm text-slate-500">Set up your restaurant profile in a couple of minutes.</p>

        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <div className="grid grid-cols-2 gap-3">
            <FormField
              id="firstName"
              label="First name"
              value={form.firstName}
              onChange={(e) => updateField('firstName', e.target.value)}
              required
            />
            <FormField
              id="lastName"
              label="Last name"
              value={form.lastName}
              onChange={(e) => updateField('lastName', e.target.value)}
              required
            />
          </div>

          <FormField
            id="email"
            label="Email"
            type="email"
            value={form.email}
            onChange={(e) => updateField('email', e.target.value)}
            required
          />
          <FormField
            id="password"
            label="Password"
            type="password"
            value={form.password}
            onChange={(e) => updateField('password', e.target.value)}
            required
          />
          <FormField
            id="restaurantName"
            label="Restaurant name"
            value={form.restaurantName}
            onChange={(e) => updateField('restaurantName', e.target.value)}
            required
          />
          <FormField
            id="phoneNumber"
            label="Phone number (optional)"
            value={form.phoneNumber}
            onChange={(e) => updateField('phoneNumber', e.target.value)}
          />

          {error && <p className="text-sm text-red-600">{error}</p>}

          <button
            type="submit"
            disabled={isSubmitting}
            className="mt-2 rounded-md bg-brand-500 px-4 py-2 text-sm font-medium text-white hover:bg-brand-600 disabled:opacity-60"
          >
            {isSubmitting ? 'Creating account...' : 'Create account'}
          </button>
        </form>

        <p className="mt-6 text-center text-sm text-slate-500">
          Already have an account?{' '}
          <Link to="/login" className="font-medium text-brand-600 hover:underline">
            Log in
          </Link>
        </p>
      </div>
    </div>
  )
}