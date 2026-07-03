import { apiClient } from '@/api/apiClient';

export const paymentApi = {
    getPaymentUrl: async (orderId: string): Promise<string> => {
        const response = await apiClient.get<string>(`/api/payments/${orderId}/url`);
        return response.data;
    }
};