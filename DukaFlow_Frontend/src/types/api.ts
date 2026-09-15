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