import { api } from "./api";
import type { AuthResponse, LoginRequest, RegisterRequest, UserDto } from "../types/api";

export const authApi = {
  register: (data: RegisterRequest) =>
    api.post<AuthResponse>("/auth/register", data).then((r) => r.data),

  login: (data: LoginRequest) =>
    api.post<AuthResponse>("/auth/login", data).then((r) => r.data),

  me: () => api.get<UserDto>("/auth/me").then((r) => r.data),
};