export interface UserDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
}

export interface RestaurantSummaryDto {
  id: string;
  name: string;
}

export interface AuthResponse {
  user: UserDto;
  restaurant: RestaurantSummaryDto;
  accessToken: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  restaurantName: string;
  phoneNumber?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RestaurantResponse {
  id: string;
  name: string;
  description?: string | null;
  phoneNumber?: string | null;
  whatsAppNumber?: string | null;
  address?: string | null;
  isActive: boolean;
}

export interface UpdateRestaurantRequest {
  name: string;
  description?: string | null;
  phoneNumber?: string | null;
  whatsAppNumber?: string | null;
  address?: string | null;
}

export interface ApiErrorResponse {
  success: false;
  message: string;
  errors: string[];
}

// ---- Menu (Phase 2) ----

export interface MenuCategoryDto {
  id: string;
  name: string;
  description?: string | null;
  displayOrder: number;
  isActive: boolean;
}

export interface CreateMenuCategoryRequest {
  name: string;
  description?: string | null;
  displayOrder: number;
}

export interface UpdateMenuCategoryRequest {
  name: string;
  description?: string | null;
  displayOrder: number;
  isActive: boolean;
}

export interface MenuItemDto {
  id: string;
  menuCategoryId: string;
  name: string;
  description?: string | null;
  price: number;
  imageUrl?: string | null;
  isAvailable: boolean;
  isActive: boolean;
  displayOrder: number;
}

export interface CreateMenuItemRequest {
  menuCategoryId: string;
  name: string;
  description?: string | null;
  price: number;
  imageUrl?: string | null;
  displayOrder: number;
}

export interface UpdateMenuItemRequest {
  menuCategoryId: string;
  name: string;
  description?: string | null;
  price: number;
  imageUrl?: string | null;
  isAvailable: boolean;
  isActive: boolean;
  displayOrder: number;
}

export interface UploadImageResponse {
  url: string;
}

// The Menu controllers wrap their payload as { success, data }, unlike
// Auth/Restaurant endpoints which return the DTO directly. See menuApi.ts.
export interface ApiEnvelope<T> {
  success: boolean;
  data: T;
}

// ---- Ordering (Phase 3) ----

// Matches DukaFlow.Domain.Enums.OrderStatus - keep in sync with the backend.
export type OrderStatus =
  | "Pending"
  | "Confirmed"
  | "Preparing"
  | "Ready"
  | "Completed"
  | "Cancelled"
  | "Rejected";

export type FulfillmentType = "Pickup" | "Delivery" | "DineIn";

export interface OrderItemDto {
  id: string;
  menuItemId: string;
  itemName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface OrderDto {
  id: string;
  orderNumber: string;
  status: OrderStatus;
  fulfillmentType: FulfillmentType;
  subtotal: number;
  deliveryFee: number;
  discountAmount: number;
  totalAmount: number;
  paymentStatus: string;
  customerName: string;
  customerPhoneNumber?: string | null;
  deliveryAddress?: string | null;
  customerNotes?: string | null;
  createdAt: string;
  placedAt?: string | null;
  items: OrderItemDto[];
}

export interface OrderSummaryDto {
  id: string;
  orderNumber: string;
  status: OrderStatus;
  totalAmount: number;
  customerName: string;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface OrderFilterQuery {
  status?: OrderStatus;
  page?: number;
  pageSize?: number;
}

export interface UpdateOrderStatusRequest {
  status: OrderStatus;
}

// The next legal status per current status - mirrors
// DukaFlow.Domain.Ordering.OrderStatusTransitions on the backend. Used
// only to decide which action buttons to show; the backend re-validates
// regardless, so this list drifting slightly out of sync is a UX
// annoyance, not a security issue.
export const ORDER_STATUS_TRANSITIONS: Record<OrderStatus, OrderStatus[]> = {
  Pending: ["Confirmed", "Rejected", "Cancelled"],
  Confirmed: ["Preparing", "Cancelled"],
  Preparing: ["Ready", "Cancelled"],
  Ready: ["Completed"],
  Completed: [],
  Cancelled: [],
  Rejected: [],
};
