import type { FC } from 'react';
import type { Product } from '../types/product';

interface ProductCardProps {
    product: Product;
    onAddToBasket: (productId: string) => void;
}

export const ProductCard: FC<ProductCardProps> = ({ product, onAddToBasket }) => {
    return (
        <div className="bg-surface rounded-2xl p-5 flex flex-col justify-between border border-border shadow-sm hover:shadow-md hover:border-border-focus transition-all">
            <div>
                <div className="flex justify-between items-center text-xs text-text-muted mb-3">
                    <span className="text-text-muted">Арт: {product.sku}</span>
                    <span className="bg-sand/30 text-text-muted px-2 py-1 rounded-md font-semibold">{product.brand}</span>
                </div>

                <div className="w-full h-40 bg-bg rounded-xl mb-4 flex items-center justify-center text-sm text-text-muted overflow-hidden">
                    {product.imageUrl ? (
                        <img src={product.imageUrl} alt={product.name} className="w-full h-full object-contain p-2 hover:scale-105 transition-transform" />
                    ) : (
                        'Нет изображения'
                    )}
                </div>

                <h3 className="font-bold text-text text-lg mb-3 line-clamp-2 leading-tight">
                    {product.name}
                </h3>

                <div className="space-y-1.5 mb-5">
                    {Object.entries(product.attributes).slice(0, 2).map(([key, value]) => (
                        <div key={key} className="text-sm flex justify-between items-center border-b border-border/50 pb-1 last:border-0">
                            <span className="text-text-muted">{key}:</span>
                            <span className="text-text font-medium truncate max-w-[140px] text-right">{value}</span>
                        </div>
                    ))}
                </div>
            </div>

            <div className="pt-4 flex items-center justify-between mt-auto">
                <div className="text-xl font-bold text-text">
                    {product.price.toLocaleString('ru-RU')} ₽
                </div>
                <button
                    onClick={() => onAddToBasket(product.id)}
                    className="px-4 py-2 bg-accent text-surface text-sm font-bold rounded-xl shadow-sm hover:shadow-md hover:scale-105 transition-all"
                >
                    В корзину
                </button>
            </div>
        </div>
    );
};