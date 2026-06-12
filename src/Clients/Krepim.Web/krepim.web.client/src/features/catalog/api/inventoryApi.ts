import { apiClient } from '@/api/apiClient';

export interface StockDto {
    productId: string;
    availableQuantity: number;
}

export interface CreditStockCommand {
    productId: string;
    quantity: number;
}

export const inventoryApi = {
    getStock: async (productId: string): Promise<number> => {
        try {
            const response = await apiClient.get<StockDto>(`/api/inventory/${productId}`);
            return response.data?.availableQuantity ?? 0;
        } catch (error: any) {
            if (error.response?.status === 404) return 0;
            console.error('Ошибка при получении остатков:', error);
            return 0;
        }
    },

    creditStock: async (command: CreditStockCommand): Promise<void> => {
        await apiClient.post('/api/inventory/credit', command);
    }
};