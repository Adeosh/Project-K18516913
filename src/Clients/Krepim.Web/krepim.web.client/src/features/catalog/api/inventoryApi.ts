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
        } catch (error: unknown) {
            const err = error as { response?: { status?: number } };

            if (err.response?.status === 404) {
                return 0;
            }

            if (error instanceof Error) {
                console.error('Ошибка при получении остатков:', error.message);
            } else {
                console.error('Неизвестная ошибка при получении остатков');
            }

            return 0;
        }
    },

    creditStock: async (command: CreditStockCommand): Promise<void> => {
        await apiClient.post('/api/inventory/credit', command);
    }
};