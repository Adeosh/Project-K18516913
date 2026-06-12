export enum SalesUnit {
    Pcs = 1,
    Pack = 2,
    Kg = 3
}

export interface PriceTierDto {
    minQuantity: number;
    amount: number;
    currency: string;
}

export interface Product {
    id: string;
    sku: string;
    name: string;
    brand: string;
    description: string;
    price: number;
    imageUrls: string[];
    isActive: boolean;
    standard?: string;
    salesUnit: SalesUnit;
    salesStep: number;
    attributes: Record<string, string>;
    priceTiers: PriceTierDto[];
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