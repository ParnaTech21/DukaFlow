import { api } from "./api";
import type { RestaurantResponse, UpdateRestaurantRequest } from "../types/api";

export const restaurantApi = {
  getMine: () => api.get<RestaurantResponse>("/restaurants/me").then((r) => r.data),

  updateMine: (data: UpdateRestaurantRequest) =>
    api.put<RestaurantResponse>("/restaurants/me", data).then((r) => r.data),
};