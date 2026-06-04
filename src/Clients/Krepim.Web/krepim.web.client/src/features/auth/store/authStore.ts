import { create } from 'zustand';
import { apiClient } from '@/api/apiClient';
import type { AuthResponse, UserClaims, UserRole } from '../types/auth';

interface AuthState {
    token: string | null;
    user: UserClaims | null;
    isLoading: boolean;
    error: string | null;
    login: (email: string, password: string) => Promise<void>;
    logout: () => void;
    register: (email: string, password: string, phoneNumber: string) => Promise<void>;
}

const decodeJwt = (token: string): UserClaims | null => {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(
            window
                .atob(base64)
                .split('')
                .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
        );

        const payload = JSON.parse(jsonPayload);

        return {
            id: payload.nameid || payload.sub,
            email: payload.email,
            role: (payload.role || payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]) as UserRole,
            exp: payload.exp
        };
    } catch (e) {
        console.error("Ошибка парсинга JWT токена:", e);
        return null;
    }
};

const initialToken = localStorage.getItem('krepim_token');
const initialUser = initialToken ? decodeJwt(initialToken) : null;

export const useAuthStore = create<AuthState>((set, get) => ({
    token: initialToken,
    user: initialUser,
    isLoading: false,
    error: null,

    login: async (email, password) => {
        set({ isLoading: true, error: null });
        try {

            const response = await apiClient.post<any>('/api/identity/login', {
                email,
                password,
            });

            const token = typeof response.data === 'string' ? response.data : response.data?.token;

            if (!token) {
                throw new Error('Токен не получен от сервера.');
            }

            localStorage.setItem('krepim_token', token);

            const claims = decodeJwt(token);
            set({ token, user: claims, isLoading: false });
        } catch (err: unknown) {
            const errorObj = err as Record<string, string> | null;
            const errorMessage = errorObj?.detail || errorObj?.title || 'Ошибка авторизации системы.';
            set({ error: errorMessage, isLoading: false });
            throw err;
        }
    },

    logout: () => {
        localStorage.removeItem('krepim_token');
        set({ token: null, user: null, error: null });
        window.location.href = '/login';
    },

    register: async (email, password, phoneNumber) => {
        set({ isLoading: true, error: null });
        try {
            await apiClient.post('/api/identity/register', {
                email,
                password,
                phoneNumber
            });

            await get().login(email, password);
        } catch (err: unknown) {
            const errorObj = err as Record<string, string> | null;
            const errorMessage = errorObj?.detail || errorObj?.title || 'Ошибка регистрации в системе.';
            set({ error: errorMessage, isLoading: false });
            throw err;
        }
    },
}));