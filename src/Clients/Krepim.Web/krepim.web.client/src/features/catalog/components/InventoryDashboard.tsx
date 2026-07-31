import { useState, useEffect } from 'react';
import type { FC } from 'react';
import { managerCatalogApi } from '../api/managerCatalogApi';
import { inventoryApi } from '../api/inventoryApi';
import type { Product } from '../types/product';

interface InventoryProduct extends Product {
    stock: number | null;
}

export const InventoryDashboard: FC = () => {
    const [products, setProducts] = useState<InventoryProduct[]>([]);
    const [searchQuery, setSearchQuery] = useState('');
    const [currentPage, setCurrentPage] = useState(1);
    const [isLoading, setIsLoading] = useState(false);
    const [selectedProduct, setSelectedProduct] = useState<InventoryProduct | null>(null);
    const [addQuantity, setAddQuantity] = useState<string>('');
    const [isSubmitting, setIsSubmitting] = useState(false);

    const loadProductsAndStock = async (page: number, term: string) => {
        setIsLoading(true);
        try {
            const prodsData = await managerCatalogApi.searchManagedProducts(term, page);
            const productsWithStock: InventoryProduct[] = await Promise.all(
                prodsData.items.map(async (p) => {
                    const stock = await inventoryApi.getStock(p.id);
                    return { ...p, stock };
                })
            );

            setProducts(productsWithStock);
        } catch (err) {
            console.error('Ошибка загрузки склада', err);
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        const timer = setTimeout(() => {
            setCurrentPage(1);
            void loadProductsAndStock(1, searchQuery);
        }, 500);
        return () => clearTimeout(timer);
    }, [searchQuery]);

    const handlePageChange = (newPage: number) => {
        setCurrentPage(newPage);
        void loadProductsAndStock(newPage, searchQuery);
    };

    const handleCreditStock = async (e: React.SubmitEvent) => {
        e.preventDefault();
        if (!selectedProduct) return;

        const qty = parseInt(addQuantity, 10);
        if (isNaN(qty) || qty <= 0) {
            alert('Введите корректное количество (больше 0)');
            return;
        }

        setIsSubmitting(true);
        try {
            await inventoryApi.creditStock({ productId: selectedProduct.id, quantity: qty });

            setProducts(prev => prev.map(p =>
                p.id === selectedProduct.id ? { ...p, stock: (p.stock || 0) + qty } : p
            ));

            closeModal();
            alert(`Успешно оприходовано: ${qty} шт. товара ${selectedProduct.sku}`);
        } catch (error) {
            console.error('Ошибка приходования:', error);
            alert('Не удалось оформить поступление. Проверьте сервер.');
        } finally {
            setIsSubmitting(false);
        }
    };

    const closeModal = () => {
        setSelectedProduct(null);
        setAddQuantity('');
    };

    return (
        <div className="max-w-7xl mx-auto p-4 space-y-6">
            <div className="bg-surface p-6 rounded-2xl border border-border shadow-sm">
                <div>
                    <h2 className="text-2xl font-bold text-text">Складской учет (Приемка)</h2>
                    <p className="text-sm text-text-muted mt-1">Оформление поступлений и контроль остатков</p>
                </div>
            </div>

            <section className="bg-surface p-6 rounded-2xl border border-border shadow-sm">
                <div className="flex justify-between items-center mb-6">
                    <input
                        type="text"
                        placeholder="Поиск по артикулу или названию..."
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        className="w-full max-w-md px-4 py-2 bg-bg border-2 border-border rounded-xl text-sm focus:border-accent outline-none"
                    />
                </div>

                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="border-b-2 border-border text-sm text-text-muted">
                                <th className="pb-3 pl-2">Артикул</th>
                                <th className="pb-3">Название</th>
                                <th className="pb-3 text-center">Остаток на складе</th>
                                <th className="pb-3 pr-2 text-right">Действия</th>
                            </tr>
                        </thead>
                        <tbody className="text-sm">
                            {isLoading ? (
                                <tr><td colSpan={4} className="text-center py-10 text-text-muted animate-pulse">Синхронизация с базой...</td></tr>
                            ) : products.length === 0 ? (
                                <tr><td colSpan={4} className="text-center py-10 text-text-muted">Товары не найдены</td></tr>
                            ) : (
                                products.map(product => (
                                    <tr key={product.id} className="border-b border-border hover:bg-bg/50 transition-colors">
                                        <td className="py-4 pl-2 font-mono text-xs font-bold text-text-muted">{product.sku}</td>
                                        <td className="py-4 font-semibold text-text max-w-[300px] truncate" title={product.name}>{product.name}</td>
                                        <td className="py-4 text-center">
                                            {product.stock !== null ? (
                                                <span className={`px-3 py-1 rounded-lg font-bold ${product.stock > 0 ? 'bg-success/10 text-success' : 'bg-error/10 text-error'}`}>
                                                    {product.stock.toLocaleString('ru-RU')}
                                                </span>
                                            ) : (
                                                <span className="text-text-muted italic text-xs">Загрузка...</span>
                                            )}
                                        </td>
                                        <td className="py-4 pr-2 text-right">
                                            <button
                                                onClick={() => setSelectedProduct(product)}
                                                className="text-xs bg-accent/10 text-accent border border-accent/20 px-4 py-2 rounded-lg hover:bg-accent hover:text-surface font-bold transition-all"
                                            >
                                                + Оформить приход
                                            </button>
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>

                <div className="flex justify-between items-center mt-6">
                    <button onClick={() => handlePageChange(Math.max(currentPage - 1, 1))} disabled={currentPage === 1 || isLoading} className="px-5 py-2.5 border border-border rounded-lg text-sm font-semibold disabled:opacity-50 hover:bg-bg transition-colors">Назад</button>
                    <span className="text-sm font-medium text-text-muted">Страница <span className="text-text font-bold">{currentPage}</span></span>
                    <button onClick={() => handlePageChange(currentPage + 1)} disabled={products.length < 20 || isLoading} className="px-5 py-2.5 border border-border rounded-lg text-sm font-semibold disabled:opacity-50 hover:bg-bg transition-colors">Вперед</button>
                </div>
            </section>

            {selectedProduct && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-text/20 backdrop-blur-sm">
                    <div className="bg-surface w-full max-w-md rounded-3xl shadow-2xl border border-border p-6 relative animate-fadeIn">
                        <button onClick={closeModal} className="absolute top-5 right-5 text-text-muted hover:text-error text-xl transition-colors">✕</button>
                        <h3 className="text-xl font-bold text-text mb-2">Оприходование товара</h3>
                        <div className="text-sm text-text-muted mb-6 bg-bg p-3 rounded-lg border border-border">
                            Арт: <span className="font-mono font-bold text-text">{selectedProduct.sku}</span><br />
                            {selectedProduct.name}
                        </div>

                        <form onSubmit={handleCreditStock} className="space-y-5">
                            <div>
                                <label className="block text-sm font-semibold text-text mb-1.5">Количество для зачисления на склад</label>
                                <input
                                    type="number"
                                    value={addQuantity}
                                    onChange={(e) => setAddQuantity(e.target.value)}
                                    autoFocus
                                    min="1"
                                    placeholder="Например: 500"
                                    className="w-full px-4 py-3 bg-bg border-2 border-border rounded-xl focus:border-accent outline-none text-lg font-bold"
                                    required
                                />
                            </div>
                            <button
                                type="submit"
                                disabled={!addQuantity || isSubmitting}
                                className="w-full mt-2 py-3.5 bg-accent text-surface font-bold rounded-xl hover:bg-accent/90 disabled:opacity-50 transition-all shadow-md"
                            >
                                {isSubmitting ? 'Обработка...' : 'Провести документ'}
                            </button>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
};