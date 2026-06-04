import { useState, useEffect } from 'react';
import type { FC, SyntheticEvent } from 'react';
import { catalogApi } from '../api/catalogApi';
import { ProductCard } from './ProductCard';
import type { Product } from '../types/product';
import { useBasketStore } from '../../basket/store/basketStore';

export const CatalogView: FC = () => {
    const [products, setProducts] = useState<Product[]>([]);
    const [searchTerm, setSearchTerm] = useState('');
    const [page, setPage] = useState(1);
    const [totalCount, setTotalCount] = useState(0);
    const [isLoading, setIsLoading] = useState(false);
    const addItemToBasket = useBasketStore((state) => state.addItem);

    const fetchProducts = async (pageNumber: number, query: string) => {
        setIsLoading(true);
        try {
            const data = await catalogApi.search({
                searchTerm: query.trim() || undefined,
                page: pageNumber,
                pageSize: 8,
            });
            setProducts(data?.items || []);
            setTotalCount(data?.totalCount || 0);
        } catch {
            console.error('Не удалось загрузить данные каталога');
            setProducts([]);
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        let isMounted = true;

        const loadData = async () => {
            if (isMounted) {
                await fetchProducts(page, searchTerm);
            }
        };

        void loadData();

        return () => {
            isMounted = false;
        };
    }, [page]);

    const handleSearchSubmit = (e: SyntheticEvent) => {
        e.preventDefault();
        setPage(1);
        void fetchProducts(1, searchTerm);
    };

    const handleAddToBasket = (productId: string) => {
        const targetProduct = products.find((p) => p.id === productId);
        if (!targetProduct) return;

        void addItemToBasket({
            productId: targetProduct.id,
            sku: targetProduct.sku,
            name: targetProduct.name,
            brand: targetProduct.brand,
            price: targetProduct.price,
            imageUrl: targetProduct.imageUrl,
        });
    };

    return (
        <div className="p-6 max-w-7xl mx-auto">
            <div className="mb-8 bg-surface p-4 rounded-2xl shadow-sm border border-border">
                <form onSubmit={handleSearchSubmit} className="flex flex-col sm:flex-row gap-4">
                    <div className="flex-1">
                        <input
                            type="text"
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            placeholder="Введите артикул или название запчасти..."
                            className="w-full px-4 py-3 bg-bg border-2 border-transparent rounded-xl text-text placeholder-text-placeholder focus:border-border-focus focus:ring-4 focus:ring-accent/10 focus:outline-none transition-all"
                        />
                    </div>
                    <button
                        type="submit"
                        disabled={isLoading}
                        className="px-8 py-3 bg-accent text-surface font-bold rounded-xl shadow-sm hover:shadow-md hover:scale-[1.02] transition-all disabled:opacity-70 disabled:hover:scale-100"
                    >
                        {isLoading ? 'Поиск...' : 'Найти'}
                    </button>
                </form>
                <div className="text-sm text-text-muted mt-3 px-2">
                    Найдено товаров: <span className="font-semibold">{totalCount}</span>
                </div>
            </div>

            {isLoading ? (
                <div className="text-center py-20 text-lg font-medium text-text-muted animate-pulse">
                    Загрузка каталога...
                </div>
            ) : products.length === 0 ? (
                <div className="text-center py-20 bg-surface rounded-2xl border border-dashed border-border text-text-muted">
                    По вашему запросу ничего не найдено
                </div>
            ) : (
                <>
                    <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
                        {products.map((product) => (
                            <ProductCard
                                key={product.id}
                                product={product}
                                onAddToBasket={handleAddToBasket}
                            />
                        ))}
                    </div>

                    <div className="mt-10 flex justify-center items-center gap-6 text-sm">
                        <button
                            onClick={() => setPage((p) => Math.max(p - 1, 1))}
                            disabled={page === 1 || isLoading}
                            className="px-5 py-2.5 bg-surface border border-border rounded-xl font-semibold disabled:opacity-50 hover:border-border-focus hover:text-accent transition-colors text-text"
                        >
                            Назад
                        </button>
                        <span className="text-text-muted font-medium">
                            Страница <span className="text-text font-bold">{page}</span>
                        </span>
                        <button
                            onClick={() => setPage((p) => p + 1)}
                            disabled={products.length < 8 || isLoading}
                            className="px-5 py-2.5 bg-surface border border-border rounded-xl font-semibold disabled:opacity-50 hover:border-border-focus hover:text-accent transition-colors text-text"
                        >
                            Вперед
                        </button>
                    </div>
                </>
            )}
        </div>
    );
};