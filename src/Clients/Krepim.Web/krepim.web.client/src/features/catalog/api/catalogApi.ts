import { apiClient } from '@/api/apiClient';
import type { Product, CatalogSearchFilters, PagedList } from '../types/product';

interface RawProductDto {
    id: string;
    sku?: string;
    name?: string;
    description?: string;
    price?: number;
    brand?: string;
    imageUrls?: string[];
    attributes?: Record<string, string>;
    isActive?: boolean;
    standard?: string;
    salesUnit?: number;
    salesStep?: number;
    priceTiers?: string | unknown[];
    PriceTiers?: string | unknown[];
}

type RawSearchResponse = RawProductDto[] | { value?: RawProductDto[]; items?: RawProductDto[]; totalCount?: number };
type RawGetByIdResponse = RawProductDto | { value?: RawProductDto };

export const catalogApi = {
    search: async (filters: CatalogSearchFilters): Promise<PagedList<Product>> => {
        const response = await apiClient.get<RawSearchResponse>('/api/products/public/search', {
            params: {
                term: filters.searchTerm,
                page: filters.page,
                categoryId: filters.categoryId,
            },
        });

        const data = response.data;

        let rawItems: RawProductDto[] = [];
        let totalCount = 0;

        if (Array.isArray(data)) {
            rawItems = data;
            totalCount = data.length;
        } else if (data) {
            rawItems = data.value || data.items || [];
            totalCount = data.totalCount || rawItems.length;
        }

        const safeItems: Product[] = rawItems.map((item) => ({
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
            priceTiers: (item.priceTiers ?? item.PriceTiers ?? []) as Product['priceTiers']
        }));

        return {
            items: safeItems,
            page: filters.page,
            pageSize: filters.pageSize || 8,
            totalCount,
            hasNextPage: false
        };
    },

    getById: async (id: string): Promise<Product> => {
        const response = await apiClient.get<RawGetByIdResponse>(`/api/products/public/${id}`);
        const data = response.data;
        const item = (data && 'value' in data && data.value) ? data.value : (data as RawProductDto);

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
            priceTiers: (item.priceTiers ?? item.PriceTiers ?? []) as Product['priceTiers']
        };
    },
};