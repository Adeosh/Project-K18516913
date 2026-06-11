import { useState, useEffect } from 'react';
import type { FC } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { catalogApi } from '../api/catalogApi';
import type { Product } from '../types/product';
import { useBasketStore } from '../../basket/store/basketStore';

export const ProductDetailView: FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [product, setProduct] = useState<Product | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [mainImage, setMainImage] = useState<string | undefined>(undefined);

    const addItemToBasket = useBasketStore((state) => state.addItem);

    useEffect(() => {
        if (!id) return;

        const loadProduct = async () => {
            setIsLoading(true);
            try {
                const data = await catalogApi.getById(id);
                setProduct(data);
                if (data.imageUrls && data.imageUrls.length > 0) {
                    setMainImage(data.imageUrls[0]);
                }
            } catch (err) {
                console.error('Ошибка загрузки товара', err);
            } finally {
                setIsLoading(false);
            }
        };

        void loadProduct();
    }, [id]);

    if (isLoading) {
        return <div className="p-10 text-center text-text-muted animate-pulse font-medium">Загрузка карточки товара...</div>;
    }

    if (!product) {
        return (
            <div className="p-10 text-center">
                <h2 className="text-2xl font-bold text-text mb-4">Товар не найден</h2>
                <button onClick={() => navigate(-1)} className="text-accent hover:underline">Вернуться назад</button>
            </div>
        );
    }

    const handleAddToCart = () => {
        void addItemToBasket({
            productId: product.id,
            sku: product.sku,
            name: product.name,
            brand: product.brand,
            price: product.price,
            imageUrl: product.imageUrls?.[0]
        });
    };

    return (
        <div className="max-w-7xl mx-auto p-4 sm:p-6 space-y-6">
            <nav className="text-sm font-medium text-text-muted mb-4">
                <Link to="/" className="hover:text-accent transition-colors">Главная</Link>
                <span className="mx-2">/</span>
                <Link to="/" className="hover:text-accent transition-colors">Каталог</Link>
                <span className="mx-2">/</span>
                <span className="text-text">{product.sku}</span>
            </nav>

            <div className="bg-surface rounded-3xl border border-border shadow-sm overflow-hidden">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-8 p-6 md:p-10">

                    <div className="space-y-4">
                        <div className="aspect-square bg-bg rounded-2xl border border-border flex items-center justify-center overflow-hidden relative">
                            {mainImage ? (
                                <img src={mainImage} alt={product.name} className="w-full h-full object-contain p-4" />
                            ) : (
                                <span className="text-text-muted font-medium">Нет фото</span>
                            )}
                        </div>

                        {product.imageUrls && product.imageUrls.length > 1 && (
                            <div className="flex gap-3 overflow-x-auto pb-2">
                                {product.imageUrls.map((url, idx) => (
                                    <button
                                        key={idx}
                                        onClick={() => setMainImage(url)}
                                        className={`w-20 h-20 flex-shrink-0 rounded-xl border-2 overflow-hidden transition-all ${mainImage === url ? 'border-accent' : 'border-border opacity-70 hover:opacity-100'}`}
                                    >
                                        <img src={url} alt={`thumb-${idx}`} className="w-full h-full object-cover" />
                                    </button>
                                ))}
                            </div>
                        )}
                    </div>

                    <div className="flex flex-col">
                        <div className="mb-2 flex items-center gap-3">
                            <span className="bg-bg text-text-muted px-3 py-1 rounded-lg text-xs font-mono font-bold border border-border">Арт: {product.sku}</span>
                            {product.brand && <span className="text-accent text-sm font-bold">{product.brand}</span>}
                        </div>

                        <h1 className="text-3xl sm:text-4xl font-extrabold text-text mb-6">{product.name}</h1>

                        <div className="text-4xl font-black text-text mb-8">
                            {product.price.toLocaleString('ru-RU')} <span className="text-2xl text-text-muted">₽</span>
                        </div>

                        <button
                            onClick={handleAddToCart}
                            className="w-full sm:w-auto px-10 py-4 bg-accent text-surface font-bold text-lg rounded-xl shadow-lg hover:shadow-xl hover:scale-[1.02] transition-all mb-10"
                        >
                            Добавить в корзину
                        </button>

                        <div className="space-y-4 flex-1">
                            <h3 className="text-xl font-bold text-text border-b border-border pb-2">Описание</h3>
                            <p className="text-text-muted leading-relaxed whitespace-pre-wrap">
                                {product.description || 'Описание товара пока не добавлено.'}
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};