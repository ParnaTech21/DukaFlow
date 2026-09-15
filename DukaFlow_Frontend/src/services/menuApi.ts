import { api } from "./api";
import type {
  ApiEnvelope,
  CreateMenuCategoryRequest,
  CreateMenuItemRequest,
  MenuCategoryDto,
  MenuItemDto,
  UpdateMenuCategoryRequest,
  UpdateMenuItemRequest,
} from "../types/api";

// The backend menu endpoints wrap their payload as { success, data } (unlike
// /auth and /restaurants/me, which return the DTO directly). Unwrap here so
// the rest of the frontend only ever deals in plain typed data.
function unwrap<T>(promise: Promise<{ data: ApiEnvelope<T> }>): Promise<T> {
  return promise.then((r) => r.data.data);
}

export const menuApi = {
  // Categories
  getCategories: () => unwrap<MenuCategoryDto[]>(api.get("/menu/categories")),

  createCategory: (data: CreateMenuCategoryRequest) =>
    unwrap<MenuCategoryDto>(api.post("/menu/categories", data)),

  updateCategory: (id: string, data: UpdateMenuCategoryRequest) =>
    unwrap<MenuCategoryDto>(api.put(`/menu/categories/${id}`, data)),

  deleteCategory: (id: string) => api.delete(`/menu/categories/${id}`).then(() => undefined),

  // Items
  getItems: (categoryId?: string) =>
    unwrap<MenuItemDto[]>(
      api.get("/menu/items", { params: categoryId ? { categoryId } : undefined })
    ),

  getItem: (id: string) => unwrap<MenuItemDto>(api.get(`/menu/items/${id}`)),

  createItem: (data: CreateMenuItemRequest) => unwrap<MenuItemDto>(api.post("/menu/items", data)),

  updateItem: (id: string, data: UpdateMenuItemRequest) =>
    unwrap<MenuItemDto>(api.put(`/menu/items/${id}`, data)),

  setItemAvailability: (id: string, isAvailable: boolean) =>
    unwrap<MenuItemDto>(api.patch(`/menu/items/${id}/availability`, isAvailable)),

  deleteItem: (id: string) => api.delete(`/menu/items/${id}`).then(() => undefined),
};
