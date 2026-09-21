import { api } from "./api";
import type {
  ApiEnvelope,
  OrderDto,
  OrderFilterQuery,
  OrderSummaryDto,
  PagedResult,
  UpdateOrderStatusRequest,
} from "../types/api";

// Orders endpoints wrap their payload as { success, data }, same as
// menuApi.ts - see the note there.
function unwrap<T>(promise: Promise<{ data: ApiEnvelope<T> }>): Promise<T> {
  return promise.then((r) => r.data.data);
}

export const ordersApi = {
  getOrders: (query?: OrderFilterQuery) =>
    unwrap<PagedResult<OrderSummaryDto>>(
      api.get("/orders", {
        params: query,
      })
    ),

  getOrder: (id: string) => unwrap<OrderDto>(api.get(`/orders/${id}`)),

  updateStatus: (id: string, request: UpdateOrderStatusRequest) =>
    unwrap<OrderDto>(api.patch(`/orders/${id}/status`, request)),
};
