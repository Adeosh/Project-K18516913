import { apiClient } from '@/api/apiClient';

export interface Address {
    fullAddress: string;
    latitude: number;
    longitude: number;
    flat: string | null;
}

export interface UserProfile {
    email: string;
    role: string;
    phoneNumber: string | null;
    defaultAddress: Address | null;
}

export interface UpdateProfilePayload {
    email: string;
    phoneNumber: string | null;
    defaultAddress: Address | null;
}

export const profileApi = {
    getProfile: async (): Promise<UserProfile> => {
        const response = await apiClient.get<UserProfile>('/api/identity/profile');
        return response.data;
    },

    updateProfile: async (payload: UpdateProfilePayload): Promise<void> => {
        await apiClient.put('/api/identity/profile', payload);
    },

    changePassword: async (oldPassword: string, newPassword: string): Promise<void> => {
        await apiClient.post('/api/identity/profile/change-password', {
            oldPassword,
            newPassword
        });
    }
};