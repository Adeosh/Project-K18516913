import { create } from 'zustand';
import { apiClient } from '@/api/apiClient';
import { extractErrorMessage } from '@/utils/errorUtils';
import type { AuthResponse, UserClaims, UserRole } from '../types/auth';

interface AuthState {
    token: string | null;
    user: UserClaims | null;
    isLoading: boolean;
    error: string | null;
    login: (email: string, password: string) => Promise<void>;
    logout: () => void;
    register: (email: string, password: string, phoneNumber?: string) => Promise<void>;
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
    } catch (e: unknown) {
        if (e instanceof Error) {
            console.error("Ошибка парсинга JWT токена:", e.message);
        }
        return null;
    }
};

let initialToken = localStorage.getItem('krepim_token');
let initialUser = initialToken ? decodeJwt(initialToken) : null;

if (initialUser && initialUser.exp * 1000 < Date.now()) {
    localStorage.removeItem('krepim_token');
    initialToken = null;
    initialUser = null;
}

export const useAuthStore = create<AuthState>((set, get) => ({
    token: initialToken,
    user: initialUser,
    isLoading: false,
    error: null,

    login: async (email, password) => {
        set({ isLoading: true, error: null });
        try {

            const response = await apiClient.post<AuthResponse | string>('/api/identity/login', {
                email,
                password,
            });

            const data = response.data;
            const token = typeof data === 'string' ? data : (data as AuthResponse)?.token;

            if (!token) {
                throw new Error('Токен не получен от сервера.');
            }

            localStorage.setItem('krepim_token', token);

            const claims = decodeJwt(token);
            set({ token, user: claims, isLoading: false });
        } catch (err: unknown) {
            const errorMessage = extractErrorMessage(err, 'Ошибка авторизации системы.');
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
            const errorMessage = extractErrorMessage(err, 'Ошибка регистрации в системе.');
            set({ error: errorMessage, isLoading: false });
            throw err;
        }
    },
}));