import { useEffect, useRef, useState, type ChangeEvent, type FormEvent } from 'react'
import { useToast } from '../../components/Toast/ToastContext'
import { FormField } from '../../components/FormField'
import { menuApi } from '../../services/menuApi'
import type { MenuCategoryDto, MenuItemDto } from '../../types/api'
import { MenuTabs } from './MenuTabs'

interface ItemForm {
  menuCategoryId: string
  name: string
  description: string
  price: string
  imageUrl: string
  displayOrder: number
}

function emptyForm(defaultCategoryId: string, nextOrder: number): ItemForm {
  return {
    menuCategoryId: defaultCategoryId,
    name: '',
    description: '',
    price: '',
    imageUrl: '',
    displayOrder: nextOrder,
  }
}

function formatPrice(price: number) {
  return new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 }).format(price)
}

export function MenuItemsPage() {
  const { showToast } = useToast()
  const [categories, setCategories] = useState<MenuCategoryDto[]>([])
  const [items, setItems] = useState<MenuItemDto[]>([])
  const [categoryFilter, setCategoryFilter] = useState<string>('all')
  const [isLoading, setIsLoading] = useState(true)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [form, setForm] = useState<ItemForm>(emptyForm('', 0))
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [showForm, setShowForm] = useState(false)
  const [showArchived, setShowArchived] = useState(false)
  const [isUploadingImage, setIsUploadingImage] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    if (isLoading) return // initial load() already fetches items for the default filter
    loadItems()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [categoryFilter])

  async function load() {
    setIsLoading(true)
    try {
      const [categoryResult, itemResult] = await Promise.all([
        menuApi.getCategories(),
        menuApi.getItems(),
      ])
      setCategories(categoryResult.sort((a, b) => a.displayOrder - b.displayOrder))
      setItems(itemResult)
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not load menu items', 'error')
    } finally {
      setIsLoading(false)
    }
  }

  async function loadItems() {
    try {
      const result = await menuApi.getItems(categoryFilter === 'all' ? undefined : categoryFilter)
      setItems(result)
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not load menu items', 'error')
    }
  }

  function categoryName(id: string) {
    return categories.find((c) => c.id === id)?.name ?? 'Uncategorized'
  }

  function startCreate() {
    if (categories.length === 0) {
      showToast('Add a menu category first', 'error')
      return
    }
    setEditingId(null)
    setForm(emptyForm(categories[0].id, items.length))
    setShowForm(true)
  }

  function startEdit(item: MenuItemDto) {
    setEditingId(item.id)
    setForm({
      menuCategoryId: item.menuCategoryId,
      name: item.name,
      description: item.description ?? '',
      price: String(item.price),
      imageUrl: item.imageUrl ?? '',
      displayOrder: item.displayOrder,
    })
    setShowForm(true)
  }

  function cancelForm() {
    setShowForm(false)
    setEditingId(null)
  }

  function updateField<K extends keyof ItemForm>(field: K, value: ItemForm[K]) {
    setForm((prev) => ({ ...prev, [field]: value }))
  }

  async function handleImageSelected(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (!file) return

    setIsUploadingImage(true)
    try {
      const result = await menuApi.uploadImage(file)
      updateField('imageUrl', result.url)
      showToast('Image uploaded', 'success')
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not upload image', 'error')
    } finally {
      setIsUploadingImage(false)
      if (fileInputRef.current) fileInputRef.current.value = ''
    }
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    const price = Number(form.price)
    if (Number.isNaN(price) || price < 0) {
      showToast('Enter a valid price', 'error')
      return
    }

    setIsSubmitting(true)
    try {
      if (editingId) {
        const existing = items.find((i) => i.id === editingId)
        await menuApi.updateItem(editingId, {
          menuCategoryId: form.menuCategoryId,
          name: form.name,
          description: form.description || null,
          price,
          imageUrl: form.imageUrl || null,
          isAvailable: existing?.isAvailable ?? true,
          isActive: existing?.isActive ?? true,
          displayOrder: form.displayOrder,
        })
        showToast('Item updated', 'success')
      } else {
        await menuApi.createItem({
          menuCategoryId: form.menuCategoryId,
          name: form.name,
          description: form.description || null,
          price,
          imageUrl: form.imageUrl || null,
          displayOrder: form.displayOrder,
        })
        showToast('Item created', 'success')
      }
      setShowForm(false)
      setEditingId(null)
      await loadItems()
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not save item', 'error')
    } finally {
      setIsSubmitting(false)
    }
  }

  async function handleToggleAvailability(item: MenuItemDto) {
    try {
      await menuApi.setItemAvailability(item.id, !item.isAvailable)
      showToast(item.isAvailable ? 'Marked unavailable' : 'Marked available', 'success')
      await loadItems()
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not update item', 'error')
    }
  }

  async function handleArchive(item: MenuItemDto) {
    if (
      !window.confirm(
        `Archive "${item.name}"? It will be hidden from your menu, and you can restore it anytime from "Show archived".`
      )
    )
      return
    try {
      await menuApi.deleteItem(item.id)
      showToast('Item archived', 'success')
      await loadItems()
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not archive item', 'error')
    }
  }

  async function handleRestore(item: MenuItemDto) {
    try {
      await menuApi.updateItem(item.id, {
        menuCategoryId: item.menuCategoryId,
        name: item.name,
        description: item.description ?? null,
        price: item.price,
        imageUrl: item.imageUrl ?? null,
        isAvailable: item.isAvailable,
        isActive: true,
        displayOrder: item.displayOrder,
      })
      showToast('Item restored', 'success')
      await loadItems()
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not restore item', 'error')
    }
  }

  const visibleItems = items.filter((i) => showArchived || i.isActive)
  const archivedCount = items.filter((i) => !i.isActive).length

  return (
    <div className="max-w-4xl">
      <div className="mb-2 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-slate-900">Menu</h1>
        {!showForm && (
          <button
            onClick={startCreate}
            className="rounded-md bg-brand-500 px-4 py-2 text-sm font-medium text-white hover:bg-brand-600"
          >
            New item
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
            {editingId ? 'Edit item' : 'New item'}
          </h2>

          <div className="flex flex-col gap-1">
            <label htmlFor="itemCategory" className="text-sm font-medium text-slate-700">
              Category
            </label>
            <select
              id="itemCategory"
              value={form.menuCategoryId}
              onChange={(e) => updateField('menuCategoryId', e.target.value)}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-1 focus:ring-brand-500"
              required
            >
              {categories.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>
          </div>

          <FormField
            id="itemName"
            label="Name"
            value={form.name}
            onChange={(e) => updateField('name', e.target.value)}
            required
          />
          <FormField
            id="itemDescription"
            label="Description (optional)"
            value={form.description}
            onChange={(e) => updateField('description', e.target.value)}
          />
          <div className="grid grid-cols-2 gap-3">
            <FormField
              id="itemPrice"
              label="Price (UGX)"
              type="number"
              min="0"
              step="0.01"
              value={form.price}
              onChange={(e) => updateField('price', e.target.value)}
              required
            />
            <FormField
              id="itemDisplayOrder"
              label="Display order"
              type="number"
              value={form.displayOrder}
              onChange={(e) => updateField('displayOrder', Number(e.target.value))}
            />
          </div>

          <div className="flex flex-col gap-2">
            <label className="text-sm font-medium text-slate-700">Image (optional)</label>
            <div className="flex items-center gap-3">
              {form.imageUrl && (
                <img
                  src={form.imageUrl}
                  alt="Item preview"
                  className="h-16 w-16 rounded-md border border-slate-200 object-cover"
                />
              )}
              <div className="flex flex-col gap-1">
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="image/jpeg,image/png,image/webp"
                  onChange={handleImageSelected}
                  disabled={isUploadingImage}
                  className="text-sm text-slate-600"
                />
                {isUploadingImage && <span className="text-xs text-slate-400">Uploading...</span>}
              </div>
            </div>
            <FormField
              id="itemImageUrl"
              label="Or paste an image URL"
              value={form.imageUrl}
              onChange={(e) => updateField('imageUrl', e.target.value)}
            />
          </div>

          <div className="flex gap-2">
            <button
              type="submit"
              disabled={isSubmitting || isUploadingImage}
              className="rounded-md bg-brand-500 px-4 py-2 text-sm font-medium text-white hover:bg-brand-600 disabled:opacity-60"
            >
              {isSubmitting ? 'Saving...' : 'Save item'}
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

      <div className="mb-4 flex flex-wrap items-center gap-4">
        <div className="flex items-center gap-2">
          <label htmlFor="categoryFilter" className="text-sm text-slate-600">
            Filter by category:
          </label>
          <select
            id="categoryFilter"
            value={categoryFilter}
            onChange={(e) => setCategoryFilter(e.target.value)}
            className="rounded-md border border-slate-300 px-2 py-1 text-sm outline-none focus:border-brand-500"
          >
            <option value="all">All categories</option>
            {categories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
        </div>

        {archivedCount > 0 && (
          <label className="flex items-center gap-2 text-sm text-slate-600">
            <input
              type="checkbox"
              checked={showArchived}
              onChange={(e) => setShowArchived(e.target.checked)}
              className="rounded border-slate-300"
            />
            Show archived ({archivedCount})
          </label>
        )}
      </div>

      <div className="rounded-lg border border-slate-200 bg-white">
        {isLoading ? (
          <p className="p-6 text-sm text-slate-500">Loading items...</p>
        ) : visibleItems.length === 0 ? (
          <p className="p-6 text-sm text-slate-500">
            {categories.length === 0
              ? 'Add a category first, then add your menu items.'
              : items.length === 0
                ? 'No items yet in this view.'
                : 'No active items. Check "Show archived" to see archived ones.'}
          </p>
        ) : (
          <ul className="divide-y divide-slate-100">
            {visibleItems.map((item) => (
              <li key={item.id} className="flex items-center justify-between gap-4 p-4">
                <div className="flex min-w-0 items-center gap-3">
                  {item.imageUrl && (
                    <img
                      src={item.imageUrl}
                      alt={item.name}
                      className="h-12 w-12 shrink-0 rounded-md border border-slate-200 object-cover"
                    />
                  )}
                  <div className="min-w-0">
                    <p className="flex items-center gap-2 text-sm font-medium text-slate-800">
                      {item.name}
                      {!item.isAvailable && (
                        <span className="rounded-full bg-amber-100 px-2 py-0.5 text-xs font-normal text-amber-700">
                          Unavailable
                        </span>
                      )}
                      {!item.isActive && (
                        <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs font-normal text-slate-500">
                          Archived
                        </span>
                      )}
                    </p>
                    <p className="text-sm text-slate-500">
                      {categoryName(item.menuCategoryId)} · UGX {formatPrice(item.price)}
                    </p>
                  </div>
                </div>
                <div className="flex shrink-0 gap-2">
                  {item.isActive ? (
                    <>
                      <button
                        onClick={() => startEdit(item)}
                        className="rounded-md border border-slate-200 px-3 py-1 text-sm text-slate-600 hover:bg-slate-100"
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => handleToggleAvailability(item)}
                        className="rounded-md border border-slate-200 px-3 py-1 text-sm text-slate-600 hover:bg-slate-100"
                      >
                        {item.isAvailable ? 'Mark unavailable' : 'Mark available'}
                      </button>
                      <button
                        onClick={() => handleArchive(item)}
                        className="rounded-md border border-red-200 px-3 py-1 text-sm text-red-600 hover:bg-red-50"
                      >
                        Archive
                      </button>
                    </>
                  ) : (
                    <button
                      onClick={() => handleRestore(item)}
                      className="rounded-md border border-slate-200 px-3 py-1 text-sm text-slate-600 hover:bg-slate-100"
                    >
                      Restore
                    </button>
                  )}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  )
}
