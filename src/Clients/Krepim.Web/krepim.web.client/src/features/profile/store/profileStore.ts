import { create } from 'zustand';
import { profileApi, type UserProfile, type UpdateProfilePayload } from '../api/profileApi';

interface ProfileState {
    profile: UserProfile | null;
    isLoading: boolean;
    error: string | null;
    fetchProfile: () => Promise<void>;
    updateProfile: (payload: UpdateProfilePayload) => Promise<void>;
}

export const useProfileStore = create<ProfileState>((set) => ({
    profile: null,
    isLoading: false,
    error: null,

    fetchProfile: async () => {
        set({ isLoading: true, error: null });
        try {
            const profile = await profileApi.getProfile();
            set({ profile, isLoading: false });
        } catch (error: any) {
            set({ error: error.message, isLoading: false });
        }
    },

    updateProfile: async (payload: UpdateProfilePayload) => {
        set({ isLoading: true, error: null });
        try {
            await profileApi.updateProfile(payload);
            const updatedProfile = await profileApi.getProfile();
            set({ profile: updatedProfile, isLoading: false });
        } catch (error: any) {
            set({ error: error.message, isLoading: false });
            throw error;
        }
    }
}));