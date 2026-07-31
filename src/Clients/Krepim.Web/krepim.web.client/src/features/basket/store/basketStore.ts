import { create } from 'zustand';
import { apiClient } from '@/api/apiClient';
import { extractErrorMessage } from '@/utils/errorUtils';
import type { CustomerBasket, BasketItem } from '../types/basket';

interface BasketState {
    basket: CustomerBasket | null;
    isLoading: boolean;
    error: string | null;
    fetchBasket: () => Promise<void>;
    addItem: (item: Omit<BasketItem, 'quantity'> & { quantity?: number }) => Promise<void>;
    updateQuantity: (productId: string, quantity: number, price: number) => Promise<void>;
    removeItem: (productId: string) => Promise<void>;
    clearBasket: () => void;
    checkout: (addressData: {
        customerEmail: string;
        customerPhone: string | null;
        fullAddress: string;
        latitude: number;
        longitude: number;
        flat: string | null
    }) => Promise<string | null>;
}

export const useBasketStore = create<BasketState>((set, get) => ({
    basket: null,
    isLoading: false,
    error: null,

    fetchBasket: async () => {
        set({ isLoading: true, error: null });
        try {
            const response = await apiClient.get<CustomerBasket>('/api/basket');
            set({ basket: response.data, isLoading: false });
        } catch (err: unknown) {
            set({ error: extractErrorMessage(err, 'Не удалось загрузить корзину.'), isLoading: false });
        }
    },

    addItem: async (newItem) => {
        const qtyToAdd = newItem.quantity || 1;
        const currentBasket = get().basket;
        const items = currentBasket ? [...currentBasket.items] : [];
        const existingItemIndex = items.findIndex((i) => i.productId === newItem.productId);

        if (existingItemIndex > -1) {
            items[existingItemIndex].quantity += qtyToAdd;
        } else {
            items.push({ ...newItem, quantity: qtyToAdd });
        }

        const updatedBasket: CustomerBasket = {
            buyerId: currentBasket?.buyerId || '',
            items,
        };

        set({ basket: updatedBasket });

        try {
            await apiClient.post('/api/basket/items', {
                productId: newItem.productId,
                productName: newItem.name,
                sku: newItem.sku,
                unitPrice: newItem.price,
                quantity: qtyToAdd
            });
        } catch {
            set({ error: 'Ошибка добавления товара в корзину.' });
        }
    },

    updateQuantity: async (productId, quantity, price) => {
        const currentBasket = get().basket;
        if (!currentBasket) return;

        if (quantity <= 0) {
            await get().removeItem(productId);
            return;
        }

        const items = currentBasket.items.map((item) =>
            item.productId === productId ? { ...item, quantity, price } : item
        );

        set({ basket: { ...currentBasket, items } });

        try {
            await apiClient.put(`/api/basket/items/${productId}`, { quantity, price });
        } catch {
            set({ error: 'Ошибка обновления количества.' });
        }
    },

    removeItem: async (productId) => {
        const currentBasket = get().basket;
        if (!currentBasket) return;

        const items = currentBasket.items.filter((item) => item.productId !== productId);
        const updatedBasket = { ...currentBasket, items };
        set({ basket: updatedBasket });

        try {
            await apiClient.delete(`/api/basket/${productId}`);
        } catch {
            set({ error: 'Не удалось удалить товар из корзины.' });
        }
    },

    clearBasket: () => set({ basket: null, error: null }),

    checkout: async (addressData) => {
        set({ isLoading: true, error: null });
        try {
            const response = await apiClient.post<{ orderId: string }>('/api/basket/checkout', addressData);
            
            set({ basket: null, isLoading: false });
            
            return response.data.orderId;
        } catch (err: unknown) {
            set({ error: extractErrorMessage(err, 'Ошибка при оформлении заказа'), isLoading: false });
            return null;
        }
    },
}));