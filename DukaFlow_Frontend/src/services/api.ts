import axios, { AxiosError } from "axios";
import type { ApiErrorResponse } from "../types/api";

const TOKEN_KEY = "dukaflow_token";

export const tokenStorage = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (token: string) => localStorage.setItem(TOKEN_KEY, token),
  clear: () => localStorage.removeItem(TOKEN_KEY),
};

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});

api.interceptors.request.use((config) => {
  const token = tokenStorage.get();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Normalizes backend error shape { success, message, errors } into a plain Error
api.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiErrorResponse>) => {
    const message = error.response?.data?.message ?? "Something went wrong. Please try again.";
    return Promise.reject(new Error(message));
  }
);