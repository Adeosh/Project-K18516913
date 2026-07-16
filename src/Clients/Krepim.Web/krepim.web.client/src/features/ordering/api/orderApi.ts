import { apiClient } from '@/api/apiClient';

export interface OrderItemDto {
    productId: string;
    unitPrice: number;
    quantity: number;
}

export interface OrderDto {
    id: string;
    status: string;
    totalPrice: number;
    customerEmail: string;
    customerPhone?: string | null;
    fullAddress: string;
    latitude: number;
    longitude: number;
    flat: string | null;
    createdAt: string;
    items: OrderItemDto[];
    qrCodeUrl?: string;
}

export const orderApi = {
    getMyOrders: async (): Promise<OrderDto[]> => {
        const response = await apiClient.get<OrderDto[]>('/api/orders');
        return response.data;
    },

    getById: async (id: string): Promise<OrderDto> => {
        const response = await apiClient.get<OrderDto>(`/api/orders/${id}`);
        return response.data;
    },

    getAllOrders: async (): Promise<OrderDto[]> => {
        const response = await apiClient.get<OrderDto[]>('/api/orders/all');
        return response.data;
    }
};