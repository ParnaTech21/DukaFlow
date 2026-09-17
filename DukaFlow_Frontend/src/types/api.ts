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