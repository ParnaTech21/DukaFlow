export type UserRole = 'RestaurantOwner' | 'RestaurantStaff' | 'PlatformAdmin'

export interface User {
  id: string
  firstName: string
  lastName: string
  email: string
  role: UserRole
}

export interface Restaurant {
  id: string
  name: string
  description?: string
  phoneNumber?: string
  whatsAppNumber?: string
  address?: string
}

export interface AuthResponse {
  user: User
  restaurant: Restaurant
  accessToken: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  firstName: string
  lastName: string
  email: string
  password: string
  restaurantName: string
  phoneNumber?: string
}
