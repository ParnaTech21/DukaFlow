import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useToast } from '../../components/Toast/ToastContext'
import { ordersApi } from '../../services/ordersApi'
import { ORDER_STATUS_TRANSITIONS } from '../../types/api'
import type { OrderDto, OrderStatus } from '../../types/api'

const STATUS_BADGE: Record<OrderStatus, string> = {
  Pending: 'bg-amber-100 text-amber-700',
  Confirmed: 'bg-blue-100 text-blue-700',
  Preparing: 'bg-blue-100 text-blue-700',
  Ready: 'bg-purple-100 text-purple-700',
  Completed: 'bg-green-100 text-green-700',
  Cancelled: 'bg-slate-200 text-slate-600',
  Rejected: 'bg-red-100 text-red-700',
}

// Buttons are secondary (grey) for a plain forward step, and red for a
// cancel/reject action - purely visual, the backend enforces the real
// transition rules regardless of what's shown here.
const DESTRUCTIVE_STATUSES: OrderStatus[] = ['Cancelled', 'Rejected']

export function OrderDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { showToast } = useToast()
  const [order, setOrder] = useState<OrderDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isUpdating, setIsUpdating] = useState(false)

  useEffect(() => {
    if (id) load(id)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id])

  async function load(orderId: string) {
    setIsLoading(true)
    try {
      const result = await ordersApi.getOrder(orderId)
      setOrder(result)
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not load order', 'error')
    } finally {
      setIsLoading(false)
    }
  }

  async function handleStatusChange(nextStatus: OrderStatus) {
    if (!order) return
    if (
      DESTRUCTIVE_STATUSES.includes(nextStatus) &&
      !window.confirm(`Mark order ${order.orderNumber} as ${nextStatus}? This cannot be undone.`)
    ) {
      return
    }

    setIsUpdating(true)
    try {
      const updated = await ordersApi.updateStatus(order.id, { status: nextStatus })
      setOrder(updated)
      showToast(`Order marked ${nextStatus}`, 'success')
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not update order status', 'error')
    } finally {
      setIsUpdating(false)
    }
  }

  if (isLoading) {
    return <p className="text-sm text-slate-500">Loading order...</p>
  }

  if (!order) {
    return (
      <div>
        <p className="text-sm text-slate-500">Order not found.</p>
        <Link to="/orders" className="text-sm text-brand-600 hover:underline">
          Back to orders
        </Link>
      </div>
    )
  }

  const nextStatuses = ORDER_STATUS_TRANSITIONS[order.status]

  return (
    <div className="max-w-2xl">
      <Link to="/orders" className="mb-4 inline-block text-sm text-brand-600 hover:underline">
        ← Back to orders
      </Link>

      <div className="mb-6 flex items-start justify-between">
        <div>
          <h1 className="flex items-center gap-2 text-2xl font-semibold text-slate-900">
            {order.orderNumber}
            <span
              className={`rounded-full px-2 py-0.5 text-sm font-medium ${STATUS_BADGE[order.status]}`}
            >
              {order.status}
            </span>
          </h1>
          <p className="mt-1 text-sm text-slate-500">
            {order.placedAt ? new Date(order.placedAt).toLocaleString() : ''} ·{' '}
            {order.fulfillmentType}
          </p>
        </div>
      </div>

      {nextStatuses.length > 0 && (
        <div className="mb-6 flex flex-wrap gap-2">
          {nextStatuses.map((status) => (
            <button
              key={status}
              onClick={() => handleStatusChange(status)}
              disabled={isUpdating}
              className={`rounded-md px-4 py-2 text-sm font-medium disabled:opacity-60 ${
                DESTRUCTIVE_STATUSES.includes(status)
                  ? 'border border-red-200 text-red-600 hover:bg-red-50'
                  : 'bg-brand-500 text-white hover:bg-brand-600'
              }`}
            >
              Mark as {status}
            </button>
          ))}
        </div>
      )}

      <div className="mb-6 rounded-lg border border-slate-200 bg-white p-6">
        <h2 className="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-500">
          Customer
        </h2>
        <p className="text-sm text-slate-800">{order.customerName}</p>
        {order.customerPhoneNumber && (
          <p className="text-sm text-slate-600">{order.customerPhoneNumber}</p>
        )}
        {order.deliveryAddress && (
          <p className="mt-2 text-sm text-slate-600">
            <span className="font-medium text-slate-700">Delivery address: </span>
            {order.deliveryAddress}
          </p>
        )}
        {order.customerNotes && (
          <p className="mt-2 text-sm text-slate-600">
            <span className="font-medium text-slate-700">Notes: </span>
            {order.customerNotes}
          </p>
        )}
      </div>

      <div className="rounded-lg border border-slate-200 bg-white">
        <h2 className="border-b border-slate-100 p-4 text-sm font-semibold uppercase tracking-wide text-slate-500">
          Items
        </h2>
        <ul className="divide-y divide-slate-100">
          {order.items.map((item) => (
            <li key={item.id} className="flex items-center justify-between gap-4 p-4">
              <div>
                <p className="text-sm font-medium text-slate-800">{item.itemName}</p>
                <p className="text-sm text-slate-500">
                  {item.quantity} × UGX {item.unitPrice.toLocaleString()}
                </p>
              </div>
              <p className="text-sm font-medium text-slate-800">
                UGX {item.lineTotal.toLocaleString()}
              </p>
            </li>
          ))}
        </ul>
        <div className="flex flex-col gap-1 border-t border-slate-100 p-4 text-sm">
          <div className="flex justify-between text-slate-600">
            <span>Subtotal</span>
            <span>UGX {order.subtotal.toLocaleString()}</span>
          </div>
          {order.deliveryFee > 0 && (
            <div className="flex justify-between text-slate-600">
              <span>Delivery fee</span>
              <span>UGX {order.deliveryFee.toLocaleString()}</span>
            </div>
          )}
          {order.discountAmount > 0 && (
            <div className="flex justify-between text-slate-600">
              <span>Discount</span>
              <span>-UGX {order.discountAmount.toLocaleString()}</span>
            </div>
          )}
          <div className="flex justify-between text-base font-semibold text-slate-900">
            <span>Total</span>
            <span>UGX {order.totalAmount.toLocaleString()}</span>
          </div>
          <div className="mt-1 flex justify-between text-xs text-slate-400">
            <span>Payment</span>
            <span>{order.paymentStatus}</span>
          </div>
        </div>
      </div>
    </div>
  )
}
