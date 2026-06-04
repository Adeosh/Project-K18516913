export type UserRole = 'Client' | 'Manager';

export interface UserClaims {
    id: string;
    email: string;
    role: UserRole;
    exp: number;
}

export interface AuthResponse {
    token: string;
    refreshToken?: string;
}