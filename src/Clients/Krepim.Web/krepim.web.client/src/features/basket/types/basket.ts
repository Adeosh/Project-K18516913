export interface BasketItem {
    productId: string;
    sku: string;
    name: string;
    brand: string;
    price: number;
    quantity: number;
    imageUrl?: string;
}

export interface CustomerBasket {
    buyerId: string;
    items: BasketItem[];
}