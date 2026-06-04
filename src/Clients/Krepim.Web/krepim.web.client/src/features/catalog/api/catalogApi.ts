import { apiClient } from '@/api/apiClient';
import type { Product, CatalogSearchFilters, PagedList } from '../types/product';

export const catalogApi = {
    search: async (filters: CatalogSearchFilters): Promise<PagedList<Product>> => {
        const response = await apiClient.get<PagedList<Product>>('/api/products/public/search', {
            params: {
                term: filters.searchTerm,
                page: filters.page,
            },
        });
        return response.data;
    },

    getById: async (id: string): Promise<Product> => {
        const response = await apiClient.get<Product>(`/api/products/public/${id}`);
        return response.data;
    },
};