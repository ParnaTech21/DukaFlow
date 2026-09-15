import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { authApi } from "../services/authApi";
import { restaurantApi } from "../services/restaurantApi";
import { tokenStorage } from "../services/api";
import type { LoginRequest, RegisterRequest, RestaurantResponse, UserDto } from "../types/api";

interface AuthContextValue {
  user: UserDto | null;
  restaurant: RestaurantResponse | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (data: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
  refreshRestaurant: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(null);
  const [restaurant, setRestaurant] = useState<RestaurantResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const token = tokenStorage.get();
    if (!token) {
      setIsLoading(false);
      return;
    }

    Promise.all([authApi.me(), restaurantApi.getMine()])
      .then(([userResult, restaurantResult]) => {
        setUser(userResult);
        setRestaurant(restaurantResult);
      })
      .catch(() => tokenStorage.clear())
      .finally(() => setIsLoading(false));
  }, []);

  async function login(data: LoginRequest) {
    const result = await authApi.login(data);
    tokenStorage.set(result.accessToken);
    setUser(result.user);
    const restaurantResult = await restaurantApi.getMine();
    setRestaurant(restaurantResult);
  }

  async function register(data: RegisterRequest) {
    const result = await authApi.register(data);
    tokenStorage.set(result.accessToken);
    setUser(result.user);
    const restaurantResult = await restaurantApi.getMine();
    setRestaurant(restaurantResult);
  }

  function logout() {
    tokenStorage.clear();
    setUser(null);
    setRestaurant(null);
  }

  async function refreshRestaurant() {
    const restaurantResult = await restaurantApi.getMine();
    setRestaurant(restaurantResult);
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        restaurant,
        isLoading,
        isAuthenticated: !!user,
        login,
        register,
        logout,
        refreshRestaurant,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}