import { apiClient } from '@/api/apiClient';
import type { Product, PagedList, SalesUnit, PriceTierDto } from '../types/product';

export interface CreateProductCommand {
    name: string;
    sku: string;
    price: number;
    description: string;
    categoryId: string;
    imageUrls: string[];
    standard?: string;
    salesUnit: SalesUnit;
    salesStep: number;
    attributes?: Record<string, string>;
    priceTiers?: PriceTierDto[];
}

export interface CategoryDto {
    id: string;
    name: string;
    description: string;
}

interface RawManagedProductDto extends Omit<Partial<Product>, 'priceTiers'> {
    id: string;
    PriceTiers?: string | unknown[];
    priceTiers?: string | unknown[];
}

type ManagerSearchResponse = RawManagedProductDto[] | { value?: RawManagedProductDto[]; items?: RawManagedProductDto[] };

export const managerCatalogApi = {
    getCategories: async (): Promise<CategoryDto[]> => {
        const response = await apiClient.get<CategoryDto[]>('/api/products/categories');
        return response.data;
    },

    createCategory: async (name: string, description: string): Promise<string> => {
        const response = await apiClient.post<string>('/api/products/categories', { name, description });
        return response.data;
    },

    deleteCategory: async (id: string): Promise<void> => {
        await apiClient.delete(`/api/products/categories/${id}`);
    },

    uploadImages: async (files: FileList | File[]): Promise<string[]> => {
        const formData = new FormData();
        Array.from(files).forEach(file => {
            formData.append('file', file);
        });

        const response = await apiClient.post<{ urls: string[] }>('/api/products/manager/images', formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
        });
        return response.data.urls;
    },

    createProduct: async (command: CreateProductCommand): Promise<string> => {
        const response = await apiClient.post<string>('/api/products', command);
        return response.data;
    },

    searchManagedProducts: async (searchTerm: string = '', page: number = 1): Promise<PagedList<Product>> => {
        const response = await apiClient.get<ManagerSearchResponse>('/api/products/manager/search', {
            params: { term: searchTerm, page }
        });

        const data = response.data;
        const rawItems = Array.isArray(data) ? data : (data?.value || data?.items || []);
        const safeItems: Product[] = rawItems.map(item => ({
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
            page: page,
            pageSize: 20,
            totalCount: safeItems.length,
            hasNextPage: false
        };
    },

    publishProduct: async (id: string): Promise<void> => {
        await apiClient.post(`/api/products/manager/${id}/publish`);
    },

    updateProduct: async (id: string, command: CreateProductCommand): Promise<void> => {
        await apiClient.put(`/api/products/manager/${id}`, command);
    },

    deactivateProduct: async (id: string): Promise<void> => {
        await apiClient.post(`/api/products/manager/${id}/deactivate`);
    },

    deleteProduct: async (id: string): Promise<void> => {
        await apiClient.delete(`/api/products/manager/${id}`);
    }
};