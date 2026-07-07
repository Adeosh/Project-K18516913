import { useState, useEffect } from 'react';
import type { FC, SyntheticEvent } from 'react';
import { useSearchParams } from 'react-router-dom';
import { apiClient } from '@/api/apiClient';
import { catalogApi } from '../api/catalogApi';
import { ProductCard } from './ProductCard';
import type { Product } from '../types/product';
import { useBasketStore } from '../../basket/store/basketStore';
import type { CategoryDto } from '../api/managerCatalogApi';

export const CatalogView: FC = () => {
    const [searchParams] = useSearchParams();
    const [selectedCategory, setSelectedCategory] = useState<string>(searchParams.get('categoryId') || '');
    const [products, setProducts] = useState<Product[]>([]);
    const [searchTerm, setSearchTerm] = useState('');
    const [categories, setCategories] = useState<CategoryDto[]>([]);
    const [page, setPage] = useState(1);
    const [totalCount, setTotalCount] = useState(0);
    const [isLoading, setIsLoading] = useState(false);
    const addItemToBasket = useBasketStore((state) => state.addItem);

    useEffect(() => {
        const init = async () => {
            try {
                const response = await apiClient.get<any>('/api/products/categories');
                const data = response.data?.value || response.data;
                if (Array.isArray(data)) {
                    setCategories(data);
                }
            } catch (e) {
                console.error('Ошибка загрузки категорий', e);
            }
        };
        void init();
    }, []);

    useEffect(() => {
        const categoryIdFromUrl = searchParams.get('categoryId') || '';
        if (categoryIdFromUrl !== selectedCategory) {
            setSelectedCategory(categoryIdFromUrl);
        }
    }, [searchParams]);

    const fetchProducts = async (pageNumber: number, query: string, catId: string) => {
        setIsLoading(true);
        try {
            const data = await catalogApi.search({
                searchTerm: query.trim() || undefined,
                categoryId: catId || undefined,
                page: pageNumber,
                pageSize: 8,
            });
            setProducts(data.items);
            setTotalCount(data.totalCount);
        } catch {
            setProducts([]);
            setTotalCount(0);
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        let isMounted = true;
        const loadData = async () => {
            if (isMounted) await fetchProducts(page, searchTerm, selectedCategory);
        };
        void loadData();
        return () => { isMounted = false; };
    }, [page, selectedCategory]);

    const handleSearchSubmit = (e: SyntheticEvent) => {
        e.preventDefault();
        setPage(1);
        navigate('/catalog', { replace: true });
        void fetchProducts(1, searchTerm, selectedCategory);
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
            imageUrl: targetProduct.imageUrls && targetProduct.imageUrls.length > 0
                ? targetProduct.imageUrls[0]
                : undefined,
        });
    };

    return (
        <div className="p-6 max-w-7xl mx-auto">
            <div className="mb-8 bg-surface p-4 rounded-2xl shadow-sm border border-border">
                <form onSubmit={handleSearchSubmit} className="flex flex-col sm:flex-row gap-4 items-center">
                    <div className="relative w-full sm:w-auto">
                        <select
                            value={selectedCategory}
                            onChange={(e) => setSelectedCategory(e.target.value)}
                            className="w-full sm:w-64 appearance-none px-4 py-3 bg-bg border-2 border-border rounded-xl text-text 
                       focus:border-accent outline-none cursor-pointer hover:border-accent/50 transition-colors"
                        >
                            <option value="">Все категории</option>
                            {Array.isArray(categories) && categories.map(cat => (
                                <option key={cat.id} value={cat.id}>{cat.name}</option>
                            ))}
                        </select>
                        <div className="absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none text-text-muted">
                            ▼
                        </div>
                    </div>

                    <div className="flex-1 w-full">
                        <input
                            type="text"
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            placeholder="Введите артикул или название..."
                            className="w-full px-4 py-3 bg-bg border-2 border-border rounded-xl focus:border-accent outline-none transition-all"
                        />
                    </div>

                    <button
                        type="submit"
                        className="w-full sm:w-auto px-8 py-3 bg-[#9E2F1F] hover:bg-[#85281a] text-white font-bold rounded-xl transition-all shadow-md active:scale-95"
                    >
                        Найти
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