import { useEffect, useState, type FormEvent } from 'react'
import { useToast } from '../../components/Toast/ToastContext'
import { FormField } from '../../components/FormField'
import { menuApi } from '../../services/menuApi'
import type { MenuCategoryDto } from '../../types/api'
import { MenuTabs } from './MenuTabs'

interface CategoryForm {
  name: string
  description: string
  displayOrder: number
}

function emptyForm(nextOrder: number): CategoryForm {
  return { name: '', description: '', displayOrder: nextOrder }
}

export function MenuCategoriesPage() {
  const { showToast } = useToast()
  const [categories, setCategories] = useState<MenuCategoryDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [form, setForm] = useState<CategoryForm>(emptyForm(0))
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [showForm, setShowForm] = useState(false)

  useEffect(() => {
    load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function load() {
    setIsLoading(true)
    try {
      const result = await menuApi.getCategories()
      setCategories(result.sort((a, b) => a.displayOrder - b.displayOrder))
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not load categories', 'error')
    } finally {
      setIsLoading(false)
    }
  }

  function startCreate() {
    setEditingId(null)
    setForm(emptyForm(categories.length))
    setShowForm(true)
  }

  function startEdit(category: MenuCategoryDto) {
    setEditingId(category.id)
    setForm({
      name: category.name,
      description: category.description ?? '',
      displayOrder: category.displayOrder,
    })
    setShowForm(true)
  }

  function cancelForm() {
    setShowForm(false)
    setEditingId(null)
  }

  function updateField<K extends keyof CategoryForm>(field: K, value: CategoryForm[K]) {
    setForm((prev) => ({ ...prev, [field]: value }))
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setIsSubmitting(true)
    try {
      if (editingId) {
        const existing = categories.find((c) => c.id === editingId)
        await menuApi.updateCategory(editingId, {
          name: form.name,
          description: form.description || null,
          displayOrder: form.displayOrder,
          isActive: existing?.isActive ?? true,
        })
        showToast('Category updated', 'success')
      } else {
        await menuApi.createCategory({
          name: form.name,
          description: form.description || null,
          displayOrder: form.displayOrder,
        })
        showToast('Category created', 'success')
      }
      setShowForm(false)
      setEditingId(null)
      await load()
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not save category', 'error')
    } finally {
      setIsSubmitting(false)
    }
  }

  async function handleToggleActive(category: MenuCategoryDto) {
    try {
      await menuApi.updateCategory(category.id, {
        name: category.name,
        description: category.description ?? null,
        displayOrder: category.displayOrder,
        isActive: !category.isActive,
      })
      showToast(category.isActive ? 'Category deactivated' : 'Category activated', 'success')
      await load()
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not update category', 'error')
    }
  }

  async function handleDelete(category: MenuCategoryDto) {
    if (!window.confirm(`Delete "${category.name}"? This cannot be undone.`)) return
    try {
      await menuApi.deleteCategory(category.id)
      showToast('Category deleted', 'success')
      await load()
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not delete category', 'error')
    }
  }

  return (
    <div className="max-w-3xl">
      <div className="mb-2 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-slate-900">Menu</h1>
        {!showForm && (
          <button
            onClick={startCreate}
            className="rounded-md bg-brand-500 px-4 py-2 text-sm font-medium text-white hover:bg-brand-600"
          >
            New category
          </button>
        )}
      </div>

      <MenuTabs />

      {showForm && (
        <form
          onSubmit={handleSubmit}
          className="mb-6 flex flex-col gap-4 rounded-lg border border-slate-200 bg-white p-6"
        >
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">
            {editingId ? 'Edit category' : 'New category'}
          </h2>
          <FormField
            id="categoryName"
            label="Name"
            value={form.name}
            onChange={(e) => updateField('name', e.target.value)}
            required
          />
          <FormField
            id="categoryDescription"
            label="Description (optional)"
            value={form.description}
            onChange={(e) => updateField('description', e.target.value)}
          />
          <FormField
            id="categoryDisplayOrder"
            label="Display order"
            type="number"
            value={form.displayOrder}
            onChange={(e) => updateField('displayOrder', Number(e.target.value))}
          />
          <div className="flex gap-2">
            <button
              type="submit"
              disabled={isSubmitting}
              className="rounded-md bg-brand-500 px-4 py-2 text-sm font-medium text-white hover:bg-brand-600 disabled:opacity-60"
            >
              {isSubmitting ? 'Saving...' : 'Save category'}
            </button>
            <button
              type="button"
              onClick={cancelForm}
              className="rounded-md border border-slate-200 px-4 py-2 text-sm text-slate-600 hover:bg-slate-100"
            >
              Cancel
            </button>
          </div>
        </form>
      )}

      <div className="rounded-lg border border-slate-200 bg-white">
        {isLoading ? (
          <p className="p-6 text-sm text-slate-500">Loading categories...</p>
        ) : categories.length === 0 ? (
          <p className="p-6 text-sm text-slate-500">
            No categories yet. Start by adding one, e.g. "Starters" or "Drinks".
          </p>
        ) : (
          <ul className="divide-y divide-slate-100">
            {categories.map((category) => (
              <li key={category.id} className="flex items-center justify-between gap-4 p-4">
                <div className="min-w-0">
                  <p className="flex items-center gap-2 text-sm font-medium text-slate-800">
                    {category.name}
                    {!category.isActive && (
                      <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs font-normal text-slate-500">
                        Inactive
                      </span>
                    )}
                  </p>
                  {category.description && (
                    <p className="truncate text-sm text-slate-500">{category.description}</p>
                  )}
                </div>
                <div className="flex shrink-0 gap-2">
                  <button
                    onClick={() => startEdit(category)}
                    className="rounded-md border border-slate-200 px-3 py-1 text-sm text-slate-600 hover:bg-slate-100"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => handleToggleActive(category)}
                    className="rounded-md border border-slate-200 px-3 py-1 text-sm text-slate-600 hover:bg-slate-100"
                  >
                    {category.isActive ? 'Deactivate' : 'Activate'}
                  </button>
                  <button
                    onClick={() => handleDelete(category)}
                    className="rounded-md border border-red-200 px-3 py-1 text-sm text-red-600 hover:bg-red-50"
                  >
                    Delete
                  </button>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  )
}
