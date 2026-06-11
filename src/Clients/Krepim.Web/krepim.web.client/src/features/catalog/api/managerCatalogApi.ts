import { apiClient } from '@/api/apiClient';
import type { Product, PagedList } from '../types/product';

export interface CreateProductCommand {
    name: string;
    description: string;
    sku: string;
    price: number;
    categoryId: string;
    imageUrls: string[];
}

export interface CategoryDto {
    id: string;
    name: string;
    description: string;
}

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
        const response = await apiClient.get<any>('/api/products/manager/search', {
            params: { term: searchTerm, page }
        });

        const data = response.data;
        const items = Array.isArray(data) ? data : (data?.value || data?.items || []);
        return {
            items: items,
            page: page,
            pageSize: 20,
            totalCount: items.length,
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