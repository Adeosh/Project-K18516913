import { apiClient } from '@/api/apiClient';
import type { Product, CatalogSearchFilters, PagedList, SalesUnit } from '../types/product';

export const catalogApi = {
    search: async (filters: CatalogSearchFilters): Promise<PagedList<Product>> => {
        const response = await apiClient.get<any>('/api/products/public/search', {
            params: {
                term: filters.searchTerm,
                page: filters.page,
            },
        });

        const data = response.data;
        const rawItems = Array.isArray(data) ? data : (data?.value || data?.items || []);

        const safeItems: Product[] = rawItems.map((item: any) => ({
            id: item.id,
            sku: item.sku || 'N/A',
            name: item.name || 'Без названия',
            description: item.description || '',
            price: item.price || 0,
            brand: item.brand || 'Крепим.PRO',
            imageUrls: item.imageUrls || [],
            attributes: item.attributes || {},
            isActive: item.isActive ?? true,
            standard: item.standard,
            salesUnit: item.salesUnit ?? 1,
            salesStep: item.salesStep ?? 1,
            priceTiers: item.priceTiers ?? item.PriceTiers ?? []
        }));

        return {
            items: safeItems,
            page: filters.page,
            pageSize: filters.pageSize || 8,
            totalCount: Array.isArray(data) ? safeItems.length : (data?.totalCount || safeItems.length),
            hasNextPage: false
        };
    },

    getById: async (id: string): Promise<Product> => {
        const response = await apiClient.get<any>(`/api/products/public/${id}`);
        const item = response.data?.value || response.data;

        return {
            id: item.id,
            sku: item.sku || 'N/A',
            name: item.name || 'Без названия',
            description: item.description || '',
            price: item.price || 0,
            brand: item.brand || 'Крепим.PRO',
            imageUrls: item.imageUrls || [],
            attributes: item.attributes || {},
            isActive: item.isActive ?? true,
            standard: item.standard,
            salesUnit: item.salesUnit ?? 1,
            salesStep: item.salesStep ?? 1,
            priceTiers: item.priceTiers ?? item.PriceTiers ?? []
        };
    },
};