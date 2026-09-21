import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useToast } from '../../components/Toast/ToastContext'
import { ordersApi } from '../../services/ordersApi'
import type { OrderStatus, OrderSummaryDto } from '../../types/api'

const STATUS_FILTERS: Array<OrderStatus | 'All'> = [
  'All',
  'Pending',
  'Confirmed',
  'Preparing',
  'Ready',
  'Completed',
  'Cancelled',
  'Rejected',
]

const STATUS_BADGE: Record<OrderStatus, string> = {
  Pending: 'bg-amber-100 text-amber-700',
  Confirmed: 'bg-blue-100 text-blue-700',
  Preparing: 'bg-blue-100 text-blue-700',
  Ready: 'bg-purple-100 text-purple-700',
  Completed: 'bg-green-100 text-green-700',
  Cancelled: 'bg-slate-200 text-slate-600',
  Rejected: 'bg-red-100 text-red-700',
}

const PAGE_SIZE = 20

export function OrdersPage() {
  const { showToast } = useToast()
  const [orders, setOrders] = useState<OrderSummaryDto[]>([])
  const [totalPages, setTotalPages] = useState(1)
  const [page, setPage] = useState(1)
  const [statusFilter, setStatusFilter] = useState<OrderStatus | 'All'>('All')
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, statusFilter])

  async function load() {
    setIsLoading(true)
    try {
      const result = await ordersApi.getOrders({
        page,
        pageSize: PAGE_SIZE,
        status: statusFilter === 'All' ? undefined : statusFilter,
      })
      setOrders(result.items)
      setTotalPages(Math.max(result.totalPages, 1))
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not load orders', 'error')
    } finally {
      setIsLoading(false)
    }
  }

  function handleFilterChange(status: OrderStatus | 'All') {
    setStatusFilter(status)
    setPage(1)
  }

  return (
    <div className="max-w-4xl">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-slate-900">Orders</h1>
      </div>

      <div className="mb-4 flex flex-wrap gap-1">
        {STATUS_FILTERS.map((status) => (
          <button
            key={status}
            onClick={() => handleFilterChange(status)}
            className={`rounded-full px-3 py-1 text-sm font-medium ${
              statusFilter === status
                ? 'bg-brand-500 text-white'
                : 'bg-white text-slate-600 hover:bg-slate-100'
            }`}
          >
            {status}
          </button>
        ))}
      </div>

      <div className="rounded-lg border border-slate-200 bg-white">
        {isLoading ? (
          <p className="p-6 text-sm text-slate-500">Loading orders...</p>
        ) : orders.length === 0 ? (
          <p className="p-6 text-sm text-slate-500">
            No orders {statusFilter === 'All' ? 'yet' : `with status "${statusFilter}"`}.
          </p>
        ) : (
          <ul className="divide-y divide-slate-100">
            {orders.map((order) => (
              <li key={order.id}>
                <Link
                  to={`/orders/${order.id}`}
                  className="flex items-center justify-between gap-4 p-4 hover:bg-slate-50"
                >
                  <div className="min-w-0">
                    <p className="flex items-center gap-2 text-sm font-medium text-slate-800">
                      {order.orderNumber}
                      <span
                        className={`rounded-full px-2 py-0.5 text-xs font-medium ${STATUS_BADGE[order.status]}`}
                      >
                        {order.status}
                      </span>
                    </p>
                    <p className="truncate text-sm text-slate-500">
                      {order.customerName} · {new Date(order.createdAt).toLocaleString()}
                    </p>
                  </div>
                  <p className="shrink-0 text-sm font-semibold text-slate-800">
                    UGX {order.totalAmount.toLocaleString()}
                  </p>
                </Link>
              </li>
            ))}
          </ul>
        )}
      </div>

      {totalPages > 1 && (
        <div className="mt-4 flex items-center justify-center gap-3 text-sm text-slate-600">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page <= 1}
            className="rounded-md border border-slate-200 px-3 py-1 disabled:opacity-40"
          >
            Previous
          </button>
          <span>
            Page {page} of {totalPages}
          </span>
          <button
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page >= totalPages}
            className="rounded-md border border-slate-200 px-3 py-1 disabled:opacity-40"
          >
            Next
          </button>
        </div>
      )}
    </div>
  )
}
