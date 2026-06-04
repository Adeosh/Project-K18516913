export interface Product {
    id: string;
    sku: string;
    name: string;
    brand: string;
    description: string;
    price: number;
    imageUrl?: string;
    attributes: Record<string, string>;
}

export interface CatalogSearchFilters {
    searchTerm?: string;
    brand?: string;
    page: number;
    pageSize: number;
}

export interface PagedList<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalCount: number;
    hasNextPage: boolean;
}