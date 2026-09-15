import { useState, type FormEvent } from 'react'
import { useAuth } from '../../auth/AuthContext'
import { FormField } from '../../components/FormField'
import { restaurantApi } from '../../services/restaurantApi'
import type { UpdateRestaurantRequest } from '../../types/api'

const emptyForm: UpdateRestaurantRequest = {
  name: '',
  description: '',
  phoneNumber: '',
  whatsAppNumber: '',
  address: '',
}

export function SettingsPage() {
  const { restaurant, refreshRestaurant } = useAuth()
  const [form, setForm] = useState<UpdateRestaurantRequest>(
    restaurant ? { ...emptyForm, ...restaurant } : emptyForm
  )
  const [status, setStatus] = useState<'idle' | 'saving' | 'saved' | 'error'>('idle')

  function updateField<K extends keyof UpdateRestaurantRequest>(field: K, value: string) {
    setForm((prev) => ({ ...prev, [field]: value }))
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setStatus('saving')
    try {
      await restaurantApi.updateMine(form)
      await refreshRestaurant()
      setStatus('saved')
    } catch {
      setStatus('error')
    }
  }

  return (
    <div className="max-w-lg">
      <h1 className="mb-6 text-2xl font-semibold text-slate-900">Restaurant settings</h1>

      <form onSubmit={handleSubmit} className="flex flex-col gap-4 rounded-lg border border-slate-200 bg-white p-6">
        <FormField
          id="name"
          label="Restaurant name"
          value={form.name ?? ''}
          onChange={(e) => updateField('name', e.target.value)}
          required
        />
        <FormField
          id="description"
          label="Description"
          value={form.description ?? ''}
          onChange={(e) => updateField('description', e.target.value)}
        />
        <FormField
          id="phoneNumber"
          label="Phone number"
          value={form.phoneNumber ?? ''}
          onChange={(e) => updateField('phoneNumber', e.target.value)}
        />
        <FormField
          id="whatsAppNumber"
          label="WhatsApp number"
          value={form.whatsAppNumber ?? ''}
          onChange={(e) => updateField('whatsAppNumber', e.target.value)}
        />
        <FormField
          id="address"
          label="Address"
          value={form.address ?? ''}
          onChange={(e) => updateField('address', e.target.value)}
        />

        <button
          type="submit"
          disabled={status === 'saving'}
          className="mt-2 self-start rounded-md bg-brand-500 px-4 py-2 text-sm font-medium text-white hover:bg-brand-600 disabled:opacity-60"
        >
          {status === 'saving' ? 'Saving...' : 'Save changes'}
        </button>

        {status === 'saved' && <p className="text-sm text-green-600">Saved.</p>}
        {status === 'error' && <p className="text-sm text-red-600">Something went wrong. Try again.</p>}
      </form>
    </div>
  )
}
