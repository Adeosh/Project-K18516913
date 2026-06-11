import type { FC } from 'react';
import { Link } from 'react-router-dom';
import type { Product } from '../types/product';

interface ProductCardProps {
    product: Product;
    onAddToBasket: (productId: string) => void;
}

export const ProductCard: FC<ProductCardProps> = ({ product, onAddToBasket }) => {
    return (
        <div className="bg-surface rounded-2xl p-5 flex flex-col justify-between border border-border shadow-sm hover:shadow-md hover:border-border-focus transition-all group">

            <Link to={`/product/${product.id}`} className="block">
                <div className="flex justify-between items-center text-xs text-text-muted mb-3">
                    <span className="text-text-muted">Арт: {product.sku}</span>
                    <span className="bg-sand/30 text-text-muted px-2 py-1 rounded-md font-semibold">{product.brand}</span>
                </div>

                <div className="w-full h-40 bg-bg rounded-xl mb-4 flex items-center justify-center text-sm text-text-muted overflow-hidden">
                    {product.imageUrls && product.imageUrls.length > 0 ? (
                        <img
                            src={product.imageUrls[0]}
                            alt={product.name}
                            className="w-full h-full object-contain p-2 group-hover:scale-105 transition-transform"
                        />
                    ) : (
                        'Нет изображения'
                    )}
                </div>

                <h3 className="font-bold text-text text-lg mb-3 line-clamp-2 leading-tight group-hover:text-accent transition-colors">
                    {product.name}
                </h3>

                <div className="space-y-1.5 mb-5">
                    {Object.entries(product.attributes || {}).slice(0, 2).map(([key, value]) => (
                        <div key={key} className="text-sm flex justify-between items-center border-b border-border/50 pb-1 last:border-0">
                            <span className="text-text-muted">{key}:</span>
                            <span className="text-text font-medium truncate max-w-[140px] text-right">{String(value)}</span>
                        </div>
                    ))}
                </div>
            </Link>

            <div className="pt-4 flex items-center justify-between mt-auto border-t border-border/50">
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