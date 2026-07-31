import { useState, useEffect } from 'react';
import type { FC, SubmitEvent, ChangeEvent } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { apiClient } from '@/api/apiClient';
import { catalogApi } from '../api/catalogApi';
import { ProductCard } from './ProductCard';
import type { Product } from '../types/product';
import { useBasketStore } from '../../basket/store/basketStore';
import type { CategoryDto } from '../api/managerCatalogApi';
import { extractErrorMessage } from '@/utils/errorUtils';

interface CategoryResponse {
    value?: CategoryDto[];
    items?: CategoryDto[];
}

export const CatalogView: FC = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();

    const categoryIdFromUrl = searchParams.get('categoryId') || '';
    const [selectedCategory, setSelectedCategory] = useState<string>(categoryIdFromUrl);
    const [searchTerm, setSearchTerm] = useState('');
    const [appliedSearchTerm, setAppliedSearchTerm] = useState('');

    const [categories, setCategories] = useState<CategoryDto[]>([]);
    const [products, setProducts] = useState<Product[]>([]);
    const [page, setPage] = useState(1);
    const [totalCount, setTotalCount] = useState(0);
    const [isLoading, setIsLoading] = useState(false);

    const addItemToBasket = useBasketStore((state) => state.addItem);

    const [prevUrlCategoryId, setPrevUrlCategoryId] = useState(categoryIdFromUrl);
    if (categoryIdFromUrl !== prevUrlCategoryId) {
        setPrevUrlCategoryId(categoryIdFromUrl);
        setSelectedCategory(categoryIdFromUrl);
        setPage(1);
    }

    useEffect(() => {
        const fetchCategories = async () => {
            try {
                const response = await apiClient.get<CategoryDto[] | CategoryResponse>('/api/products/categories');
                const data = response.data;
                const categoriesData = Array.isArray(data) ? data : (data?.value || data?.items || []);
                setCategories(categoriesData);
            } catch (err: unknown) {
                console.error(extractErrorMessage(err, 'Ошибка загрузки категорий'));
            }
        };
        void fetchCategories();
    }, []);

    useEffect(() => {
        let isMounted = true;

        const loadProducts = async () => {
            setIsLoading(true);
            try {
                const data = await catalogApi.search({
                    searchTerm: appliedSearchTerm.trim() || undefined,
                    categoryId: selectedCategory || undefined,
                    page: page,
                    pageSize: 8,
                });

                if (isMounted) {
                    setProducts(data.items);
                    setTotalCount(data.totalCount);
                }
            } catch (err: unknown) {
                if (isMounted) {
                    console.error(extractErrorMessage(err, 'Ошибка поиска товаров'));
                    setProducts([]);
                    setTotalCount(0);
                }
            } finally {
                if (isMounted) setIsLoading(false);
            }
        };

        void loadProducts();

        return () => { isMounted = false; };
    }, [page, selectedCategory, appliedSearchTerm]);

    const handleSearchSubmit = (e: SubmitEvent<HTMLFormElement>) => {
        e.preventDefault();
        setPage(1);
        setAppliedSearchTerm(searchTerm);
        navigate('/catalog', { replace: true });
    };

    const handleCategoryChange = (e: ChangeEvent<HTMLSelectElement>) => {
        const newCatId = e.target.value;
        setSelectedCategory(newCatId);
        setPage(1);

        if (newCatId) {
            searchParams.set('categoryId', newCatId);
        } else {
            searchParams.delete('categoryId');
        }
        navigate(`/catalog?${searchParams.toString()}`, { replace: true });
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
                            onChange={handleCategoryChange}
                            className="w-full sm:w-64 appearance-none px-4 py-3 bg-bg border-2 border-border rounded-xl text-text focus:border-accent outline-none cursor-pointer hover:border-accent/50 transition-colors"
                        >
                            <option value="">Все категории</option>
                            {categories.map(cat => (
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