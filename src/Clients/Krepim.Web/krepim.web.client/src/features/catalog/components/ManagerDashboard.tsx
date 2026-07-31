import { useState, useEffect, useCallback } from 'react';
import type { FC, SubmitEvent } from 'react';
import { managerCatalogApi, type CreateProductCommand, type CategoryDto } from '../api/managerCatalogApi';
import { SalesUnit, type Product, type PriceTierDto } from '../types/product';
import { getImageUrl } from '@/utils/imageUtils';
import { extractErrorMessage } from '@/utils/errorUtils';

interface LocalAttribute {
    id: string;
    k: string;
    v: string;
}

interface LocalPriceTier extends PriceTierDto {
    id: string;
}

export const ManagerDashboard: FC = () => {
    const [categories, setCategories] = useState<CategoryDto[]>([]);
    const [products, setProducts] = useState<Product[]>([]);

    const [formData, setFormData] = useState<CreateProductCommand>({
        name: '', sku: '', price: 0, description: '', categoryId: '', imageUrls: [],
        standard: '', salesUnit: SalesUnit.Pcs, salesStep: 1, attributes: {}, priceTiers: []
    });

    const [attrList, setAttrList] = useState<LocalAttribute[]>([]);
    const [tierList, setTierList] = useState<LocalPriceTier[]>([]);

    const [isCreateFormOpen, setIsCreateFormOpen] = useState(false);
    const [editingProductId, setEditingProductId] = useState<string | null>(null);

    const [isCategoryModalOpen, setIsCategoryModalOpen] = useState(false);
    const [newCategoryName, setNewCategoryName] = useState('');
    const [newCategoryDescription, setNewCategoryDescription] = useState('');
    const [isCategorySubmitting, setIsCategorySubmitting] = useState(false);

    const [searchQuery, setSearchQuery] = useState('');
    const [selectedCategoryFilter, setSelectedCategoryFilter] = useState('');
    const [currentPage, setCurrentPage] = useState(1);
    const [isListLoading, setIsListLoading] = useState(false);

    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isUploading, setIsUploading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const fetchProductsList = useCallback(async (page: number, term: string) => {
        setIsListLoading(true);
        try {
            const prodsData = await managerCatalogApi.searchManagedProducts(term, page);
            setProducts(prodsData.items);
        } catch (err: unknown) {
            console.error(extractErrorMessage(err, 'Ошибка загрузки товаров'));
        } finally {
            setIsListLoading(false);
        }
    }, []);

    const fetchCategoriesData = useCallback(async () => {
        try {
            return await managerCatalogApi.getCategories();
        } catch (err: unknown) {
            console.error(extractErrorMessage(err, 'Ошибка загрузки категорий'));
            return [];
        }
    }, []);

    useEffect(() => {
        let isMounted = true;

        const init = async () => {
            const catsData = await fetchCategoriesData();

            if (isMounted && catsData.length > 0) {
                setCategories(catsData);
                setFormData(prev => prev.categoryId ? prev : { ...prev, categoryId: catsData[0].id });
            }

            if (isMounted) {
                await fetchProductsList(1, '');
            }
        };

        void init();

        return () => { isMounted = false; };
    }, [fetchCategoriesData, fetchProductsList]);

    useEffect(() => {
        const timer = setTimeout(() => {
            setCurrentPage(1);
            void fetchProductsList(1, searchQuery);
        }, 500);
        return () => clearTimeout(timer);
    }, [searchQuery, fetchProductsList]);

    const handlePageChange = (newPage: number) => {
        setCurrentPage(newPage);
        void fetchProductsList(newPage, searchQuery);
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
        const { name, value, type } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === 'number' ? (parseFloat(value) || 0) : value
        }));
    };

    const addAttribute = () => setAttrList(prev => [...prev, { id: crypto.randomUUID(), k: '', v: '' }]);
    const removeAttribute = (id: string) => setAttrList(prev => prev.filter(a => a.id !== id));
    const updateAttribute = (id: string, field: 'k' | 'v', value: string) => {
        setAttrList(prev => prev.map(a => a.id === id ? { ...a, [field]: value } : a));
    };

    const addPriceTier = () => setTierList(prev => [...prev, { id: crypto.randomUUID(), minQuantity: 0, amount: 0, currency: 'RUB' }]);
    const removePriceTier = (id: string) => setTierList(prev => prev.filter(t => t.id !== id));
    const updatePriceTier = (id: string, field: keyof PriceTierDto, value: number | string) => {
        setTierList(prev => prev.map(t => t.id === id ? { ...t, [field]: value } : t));
    };

    const handleEditClick = (product: Product) => {
        setFormData({
            name: product.name,
            sku: product.sku,
            price: product.price,
            description: product.description || '',
            categoryId: product.categoryId || categories[0]?.id || '',
            imageUrls: product.imageUrls || [],
            standard: product.standard || '',
            salesUnit: product.salesUnit || SalesUnit.Pcs,
            salesStep: product.salesStep || 1,
        });

        setAttrList(Object.entries(product.attributes || {}).map(([k, v]) => ({ id: crypto.randomUUID(), k, v })));
        setTierList((product.priceTiers || []).map(t => ({ ...t, id: crypto.randomUUID() })));

        setEditingProductId(product.id);
        setIsCreateFormOpen(true);
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    const resetForm = () => {
        setFormData({
            name: '', sku: '', price: 0, description: '', categoryId: categories[0]?.id || '', imageUrls: [],
            standard: '', salesUnit: SalesUnit.Pcs, salesStep: 1
        });
        setAttrList([]);
        setTierList([]);
        setEditingProductId(null);
        setIsCreateFormOpen(false);
        setError(null);
    };

    const handleCreateCategory = async (e: SubmitEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!newCategoryName.trim()) return;
        setIsCategorySubmitting(true);
        try {
            const newId = await managerCatalogApi.createCategory(newCategoryName, newCategoryDescription);
            const catsData = await fetchCategoriesData();
            setCategories(catsData);
            setFormData(prev => ({ ...prev, categoryId: newId }));
            setIsCategoryModalOpen(false);
            setNewCategoryName('');
            setNewCategoryDescription('');
        } catch (err: unknown) {
            alert(extractErrorMessage(err, 'Ошибка создания категории'));
        } finally {
            setIsCategorySubmitting(false);
        }
    };

    const handleDeleteCategory = async (categoryId: string) => {
        if (!categoryId) return;
        if (!window.confirm('Удалить эту категорию? (Товары в ней останутся, но категория будет скрыта)')) return;

        try {
            await managerCatalogApi.deleteCategory(categoryId);
            const catsData = await fetchCategoriesData();
            setCategories(catsData);
            if (formData.categoryId === categoryId) {
                setFormData(prev => ({ ...prev, categoryId: catsData[0]?.id || '' }));
            }
        } catch (err: unknown) {
            alert(extractErrorMessage(err, 'Не удалось удалить категорию. Возможно, у вас нет прав.'));
        }
    };

    const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
        if (!e.target.files || e.target.files.length === 0) return;
        setIsUploading(true);
        setError(null);
        try {
            const urls = await managerCatalogApi.uploadImages(e.target.files);
            setFormData(prev => ({ ...prev, imageUrls: [...(prev.imageUrls || []), ...urls] }));
        } catch (err: unknown) {
            setError(extractErrorMessage(err, 'Ошибка загрузки изображений.'));
        } finally {
            setIsUploading(false);
            e.target.value = '';
        }
    };

    const removeImage = (indexToRemove: number) => {
        setFormData(prev => ({ ...prev, imageUrls: (prev.imageUrls || []).filter((_, index) => index !== indexToRemove) }));
    };

    const handleSubmit = async (e: SubmitEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError(null);
        if (!formData.name.trim() || !formData.sku.trim() || !formData.categoryId) {
            setError('Заполните обязательные поля');
            return;
        }

        const finalAttributes: Record<string, string> = {};
        attrList.forEach(a => {
            if (a.k.trim() && a.v.trim()) {
                finalAttributes[a.k.trim()] = a.v.trim();
            }
        });

        const finalPriceTiers = tierList
            .filter(t => t.minQuantity > 0 && (t.amount ?? 0) > 0)
            .map(({ id: _id, ...rest }) => rest);

        const payload: CreateProductCommand = {
            ...formData,
            salesUnit: Number(formData.salesUnit),
            attributes: finalAttributes,
            priceTiers: finalPriceTiers
        };

        setIsSubmitting(true);
        try {
            if (editingProductId) {
                await managerCatalogApi.updateProduct(editingProductId, payload);
            } else {
                await managerCatalogApi.createProduct(payload);
            }

            resetForm();
            setTimeout(() => {
                setCurrentPage(1);
                void fetchProductsList(1, searchQuery);
            }, 400);

        } catch (err: unknown) {
            setError(extractErrorMessage(err, 'Ошибка сохранения товара'));
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleToggleStatus = async (id: string, isActive: boolean) => {
        try {
            if (isActive) {
                await managerCatalogApi.deactivateProduct(id);
            } else {
                await managerCatalogApi.publishProduct(id);
            }
            setProducts(prev => prev.map(p => p.id === id ? { ...p, isActive: !isActive } : p));
        } catch (err: unknown) {
            alert(extractErrorMessage(err, 'Не удалось изменить статус товара.'));
        }
    };

    const handleDeleteProduct = async (id: string) => {
        if (!window.confirm('Вы уверены, что хотите безвозвратно удалить этот товар?')) return;
        try {
            await managerCatalogApi.deleteProduct(id);
            setProducts(prev => prev.filter(p => p.id !== id));
        } catch (err: unknown) {
            alert(extractErrorMessage(err, 'Ошибка удаления товара'));
        }
    };

    const filteredProducts = products.filter(product => {
        if (!selectedCategoryFilter) return true;
        return product.categoryId === selectedCategoryFilter;
    });

    return (
        <div className="max-w-7xl mx-auto p-4 space-y-6 relative">
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-surface p-6 rounded-2xl border border-border shadow-sm">
                <div>
                    <h2 className="text-2xl font-bold text-text">Управление каталогом</h2>
                    <p className="text-sm text-text-muted mt-1">Создание, редактирование и публикация номенклатуры</p>
                </div>
                <button
                    onClick={() => isCreateFormOpen ? resetForm() : setIsCreateFormOpen(true)}
                    className={`px-5 py-2.5 rounded-xl text-sm font-bold transition-all whitespace-nowrap ${isCreateFormOpen
                        ? 'bg-bg text-text hover:bg-border/50 border border-border'
                        : 'bg-accent text-surface shadow-md hover:scale-[1.02]'
                        }`}
                >
                    {isCreateFormOpen ? 'Скрыть панель ✕' : '+ Новый товар'}
                </button>
            </div>

            {isCreateFormOpen && (
                <section className={`p-6 rounded-2xl border-2 shadow-md animate-fadeIn ${editingProductId ? 'bg-orange-50/30 border-orange-200' : 'bg-accent/5 border-accent/20'}`}>
                    <h3 className="text-xl font-bold text-text mb-6">
                        {editingProductId ? 'Редактирование товара' : 'Создание новой карточки товара'}
                    </h3>

                    {error && <div className="mb-6 p-4 bg-error/10 text-error rounded-xl border border-error/20 text-sm font-bold">{error}</div>}

                    <form onSubmit={handleSubmit} className="space-y-8">
                        <div className="space-y-4">
                            <h4 className="font-bold text-text-muted uppercase text-xs tracking-wider">Базовая информация</h4>
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                                <div className="space-y-1.5">
                                    <label className="text-sm font-bold text-text-muted">Название товара <span className="text-error">*</span></label>
                                    <input type="text" name="name" value={formData.name} onChange={handleChange} className="w-full px-4 py-3 bg-surface border border-border rounded-xl text-text focus:border-accent outline-none" required />
                                </div>
                                <div className="space-y-1.5">
                                    <label className="text-sm font-bold text-text-muted">Артикул <span className="text-error">*</span></label>
                                    <input type="text" name="sku" value={formData.sku} onChange={handleChange} disabled={!!editingProductId} className="w-full px-4 py-3 bg-surface border border-border rounded-xl text-text focus:border-accent outline-none disabled:opacity-50" required />
                                </div>
                                <div className="space-y-1.5">
                                    <label className="text-sm font-bold text-text-muted">Базовая цена (₽) <span className="text-error">*</span></label>
                                    <input type="number" name="price" value={formData.price || ''} onChange={handleChange} className="w-full px-4 py-3 bg-surface border border-border rounded-xl text-text focus:border-accent outline-none" required min="0" step="0.01" />
                                </div>
                                <div className="space-y-1.5">
                                    <label className="text-sm font-bold text-text-muted">Категория <span className="text-error">*</span></label>
                                    <div className="flex gap-2">
                                        <select name="categoryId" value={formData.categoryId} onChange={handleChange} className="flex-1 px-4 py-3 bg-surface border border-border rounded-xl text-text focus:border-accent outline-none cursor-pointer" required>
                                            {categories.length === 0 ? <option value="" disabled>Нет категорий</option> : categories.map(cat => <option key={cat.id} value={cat.id}>{cat.name}</option>)}
                                        </select>
                                        <button type="button" onClick={() => setIsCategoryModalOpen(true)} className="px-4 py-3 bg-surface border border-border rounded-xl text-text-muted hover:border-accent hover:text-accent font-bold transition-all" title="Создать категорию">+</button>
                                        <button type="button" onClick={() => void handleDeleteCategory(formData.categoryId)} disabled={!formData.categoryId || categories.length === 0} className="px-4 py-3 bg-error/10 border border-error/20 text-error rounded-xl hover:bg-error/20 font-bold transition-all disabled:opacity-50" title="Удалить категорию">🗑️</button>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="space-y-4 pt-4 border-t border-border/50">
                            <h4 className="font-bold text-text-muted uppercase text-xs tracking-wider">Фасовка и стандарты</h4>
                            <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
                                <div className="space-y-1.5">
                                    <label className="text-sm font-bold text-text-muted">Стандарт (ГОСТ, DIN)</label>
                                    <input type="text" name="standard" value={formData.standard || ''} onChange={handleChange} placeholder="Напр: DIN 933" className="w-full px-4 py-3 bg-surface border border-border rounded-xl text-text focus:border-accent outline-none" />
                                </div>
                                <div className="space-y-1.5">
                                    <label className="text-sm font-bold text-text-muted">Ед. измерения</label>
                                    <select name="salesUnit" value={formData.salesUnit} onChange={handleChange} className="w-full px-4 py-3 bg-surface border border-border rounded-xl text-text focus:border-accent outline-none cursor-pointer">
                                        <option value={SalesUnit.Pcs}>Штуки (шт)</option>
                                        <option value={SalesUnit.Pack}>Упаковки (упак)</option>
                                        <option value={SalesUnit.Kg}>Килограммы (кг)</option>
                                    </select>
                                </div>
                                <div className="space-y-1.5">
                                    <label className="text-sm font-bold text-text-muted">Шаг добавления (кратность)</label>
                                    <input type="number" name="salesStep" value={formData.salesStep || ''} onChange={handleChange} className="w-full px-4 py-3 bg-surface border border-border rounded-xl text-text focus:border-accent outline-none" min="0.001" step="0.001" />
                                </div>
                            </div>
                        </div>

                        <div className="space-y-4 pt-4 border-t border-border/50">
                            <div className="flex justify-between items-center">
                                <h4 className="font-bold text-text-muted uppercase text-xs tracking-wider">Оптовые цены</h4>
                                <button type="button" onClick={addPriceTier} className="text-xs bg-bg border border-border px-3 py-1.5 rounded-lg font-bold hover:text-accent hover:border-accent transition-colors">+ Добавить уровень цен</button>
                            </div>
                            {tierList.length === 0 ? (
                                <p className="text-sm text-text-muted italic">Оптовые цены не заданы. Будет использоваться базовая цена.</p>
                            ) : (
                                <div className="space-y-3">
                                    {tierList.map((tier) => (
                                        <div key={tier.id} className="flex gap-3 items-center bg-surface p-3 border border-border rounded-xl">
                                            <div className="flex-1 space-y-1">
                                                <label className="text-xs font-bold text-text-muted">От кол-ва</label>
                                                <input type="number" value={tier.minQuantity || ''} onChange={(e) => updatePriceTier(tier.id, 'minQuantity', parseFloat(e.target.value) || 0)} className="w-full px-3 py-2 bg-bg border border-border rounded-lg text-sm outline-none focus:border-accent" min="1" placeholder="Напр: 1000" />
                                            </div>
                                            <div className="flex-1 space-y-1">
                                                <label className="text-xs font-bold text-text-muted">Цена за ед.</label>
                                                <input type="number" value={tier.amount || ''} onChange={(e) => updatePriceTier(tier.id, 'amount', parseFloat(e.target.value) || 0)} className="w-full px-3 py-2 bg-bg border border-border rounded-lg text-sm outline-none focus:border-accent" min="0" step="0.01" placeholder="Напр: 1.50" />
                                            </div>
                                            <button type="button" onClick={() => removePriceTier(tier.id)} className="mt-5 w-10 h-10 flex items-center justify-center bg-error/10 text-error rounded-lg hover:bg-error hover:text-surface transition-colors">✕</button>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>

                        <div className="space-y-4 pt-4 border-t border-border/50">
                            <div className="flex justify-between items-center">
                                <h4 className="font-bold text-text-muted uppercase text-xs tracking-wider">Характеристики</h4>
                                <button type="button" onClick={addAttribute} className="text-xs bg-bg border border-border px-3 py-1.5 rounded-lg font-bold hover:text-accent hover:border-accent transition-colors">+ Добавить свойство</button>
                            </div>
                            {attrList.length === 0 ? (
                                <p className="text-sm text-text-muted italic">Характеристики не заданы.</p>
                            ) : (
                                <div className="space-y-3">
                                    {attrList.map((attr) => (
                                        <div key={attr.id} className="flex gap-3">
                                            <input type="text" value={attr.k} onChange={(e) => updateAttribute(attr.id, 'k', e.target.value)} placeholder="Свойство (напр. Диаметр)" className="flex-1 px-4 py-2 bg-surface border border-border rounded-xl text-sm outline-none focus:border-accent" />
                                            <input type="text" value={attr.v} onChange={(e) => updateAttribute(attr.id, 'v', e.target.value)} placeholder="Значение (напр. M8)" className="flex-1 px-4 py-2 bg-surface border border-border rounded-xl text-sm outline-none focus:border-accent" />
                                            <button type="button" onClick={() => removeAttribute(attr.id)} className="w-10 h-10 flex items-center justify-center bg-error/10 text-error rounded-xl hover:bg-error hover:text-surface transition-colors">✕</button>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>

                        <div className="space-y-4 pt-4 border-t border-border/50">
                            <h4 className="font-bold text-text-muted uppercase text-xs tracking-wider">Медиа и Описание</h4>

                            <div className="space-y-3 p-5 border-2 border-dashed border-border rounded-xl bg-surface">
                                <div className="flex justify-between items-center">
                                    <label className="text-sm font-bold text-text-muted">Изображения товара</label>
                                    <label className="cursor-pointer bg-bg border border-border px-4 py-2 rounded-lg hover:border-accent text-sm font-bold transition-all text-text">
                                        {isUploading ? 'Загрузка...' : 'Выбрать файлы'}
                                        <input type="file" multiple accept="image/*" onChange={(e) => void handleImageUpload(e)} className="hidden" disabled={isUploading} />
                                    </label>
                                </div>
                                {(formData.imageUrls || []).length > 0 && (
                                    <div className="flex gap-4 overflow-x-auto py-2">
                                        {(formData.imageUrls || []).map((url, idx) => (
                                            <div key={idx} className="relative w-24 h-24 flex-shrink-0 border border-border rounded-lg overflow-hidden group">
                                                <img src={getImageUrl(url)} alt="Preview" className="w-full h-full object-cover" />
                                                <button type="button" onClick={() => removeImage(idx)} className="absolute top-1 right-1 w-6 h-6 bg-error text-surface rounded-full opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center text-xs font-bold">✕</button>
                                            </div>
                                        ))}
                                    </div>
                                )}
                            </div>

                            <textarea name="description" value={formData.description} onChange={handleChange} rows={4} placeholder="Подробное описание товара..." className="w-full px-4 py-3 bg-surface border border-border rounded-xl text-text focus:border-accent outline-none resize-none" />
                        </div>

                        <div className="flex justify-end gap-3 pt-4 border-t border-border/50">
                            <button type="button" onClick={resetForm} className="px-6 py-3 border border-border rounded-xl text-sm font-bold hover:bg-bg transition-colors">Отмена</button>
                            <button type="submit" disabled={isSubmitting || isUploading} className={`px-8 py-3 text-surface font-bold rounded-xl shadow-md hover:shadow-lg transition-all ${editingProductId ? 'bg-orange-500 hover:bg-orange-600' : 'bg-accent hover:bg-accent/90'}`}>
                                {isSubmitting ? 'Сохранение...' : (editingProductId ? 'Обновить товар' : 'Сохранить товар')}
                            </button>
                        </div>
                    </form>
                </section>
            )}

            <section className="bg-surface p-6 rounded-2xl border border-border shadow-sm">
                <div className="flex flex-col md:flex-row justify-between items-start md:items-center mb-6 gap-4">
                    <h3 className="text-lg font-bold text-text">Номенклатурный список</h3>
                    <div className="flex flex-col sm:flex-row gap-3 w-full md:w-auto">
                        <select value={selectedCategoryFilter} onChange={(e) => setSelectedCategoryFilter(e.target.value)} className="px-4 py-2 bg-bg border-2 border-border rounded-xl text-sm focus:border-accent outline-none cursor-pointer">
                            <option value="">Все категории</option>
                            {categories.map(cat => <option key={cat.id} value={cat.id}>{cat.name}</option>)}
                        </select>
                        <input type="text" placeholder="Поиск по артикулу..." value={searchQuery} onChange={(e) => setSearchQuery(e.target.value)} className="px-4 py-2 bg-bg border-2 border-border rounded-xl text-sm focus:border-accent outline-none" />
                    </div>
                </div>

                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="border-b-2 border-border text-sm text-text-muted">
                                <th className="pb-3 pl-2">Артикул</th>
                                <th className="pb-3">Название</th>
                                <th className="pb-3">Цена</th>
                                <th className="pb-3">Статус</th>
                                <th className="pb-3 pr-2 text-right">Действия</th>
                            </tr>
                        </thead>
                        <tbody className="text-sm">
                            {isListLoading ? (
                                <tr><td colSpan={5} className="text-center py-10 text-text-muted">Загрузка данных...</td></tr>
                            ) : filteredProducts.length === 0 ? (
                                <tr><td colSpan={5} className="text-center py-10 text-text-muted">Товары не найдены</td></tr>
                            ) : (
                                filteredProducts.map(product => (
                                    <tr key={product.id} className={`border-b border-border hover:bg-bg/50 transition-colors ${editingProductId === product.id ? 'bg-orange-50/50' : ''}`}>
                                        <td className="py-4 pl-2 font-mono text-xs">{product.sku}</td>
                                        <td className="py-4 font-semibold text-text max-w-[200px] truncate" title={product.name}>{product.name}</td>
                                        <td className="py-4 font-medium">{product.price.toLocaleString('ru-RU')} ₽</td>
                                        <td className="py-4">
                                            {product.isActive ? (
                                                <span className="bg-green-100 text-green-700 border border-green-200 px-2 py-1 rounded-md text-xs font-bold">Опубликован</span>
                                            ) : (
                                                <span className="bg-orange-100 text-orange-700 border border-orange-200 px-2 py-1 rounded-md text-xs font-bold">Скрыт</span>
                                            )}
                                        </td>
                                        <td className="py-4 pr-2 text-right space-x-2 whitespace-nowrap">
                                            <button onClick={() => handleEditClick(product)} className="text-xs bg-bg text-text border border-border px-3 py-1.5 rounded-lg hover:border-accent font-semibold transition-colors">
                                                Ред.
                                            </button>
                                            {product.isActive ? (
                                                <button onClick={() => void handleToggleStatus(product.id, true)} className="text-xs bg-orange-50 text-orange-600 border border-orange-200 px-3 py-1.5 rounded-lg hover:bg-orange-100 font-semibold transition-colors">
                                                    Скрыть
                                                </button>
                                            ) : (
                                                <button onClick={() => void handleToggleStatus(product.id, false)} className="text-xs bg-green-50 text-green-600 border border-green-200 px-3 py-1.5 rounded-lg hover:bg-green-100 font-semibold transition-colors">
                                                    Опубл.
                                                </button>
                                            )}
                                            <button onClick={() => void handleDeleteProduct(product.id)} className="text-xs bg-error/10 text-error px-3 py-1.5 rounded-lg hover:bg-error/20 font-semibold transition-colors">
                                                Удалить
                                            </button>
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>

                <div className="flex justify-between items-center mt-6">
                    <button onClick={() => handlePageChange(Math.max(currentPage - 1, 1))} disabled={currentPage === 1 || isListLoading} className="px-5 py-2.5 border border-border rounded-lg text-sm font-semibold disabled:opacity-50 hover:bg-bg transition-colors">Назад</button>
                    <span className="text-sm font-medium text-text-muted">Страница <span className="text-text font-bold">{currentPage}</span></span>
                    <button onClick={() => handlePageChange(currentPage + 1)} disabled={products.length < 20 || isListLoading} className="px-5 py-2.5 border border-border rounded-lg text-sm font-semibold disabled:opacity-50 hover:bg-bg transition-colors">Вперед</button>
                </div>
            </section>

            {isCategoryModalOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-text/20 backdrop-blur-sm">
                    <div className="bg-surface w-full max-w-sm rounded-3xl shadow-2xl border border-border p-6 relative animate-fadeIn">
                        <button
                            onClick={() => setIsCategoryModalOpen(false)}
                            className="absolute top-5 right-5 text-text-muted hover:text-error text-xl transition-colors"
                        >
                            ✕
                        </button>
                        <h3 className="text-xl font-bold text-text mb-6">Новая категория</h3>

                        <form onSubmit={handleCreateCategory} className="space-y-5">
                            <div>
                                <label className="block text-sm font-semibold text-text mb-1.5">Название</label>
                                <input
                                    type="text"
                                    value={newCategoryName}
                                    onChange={(e) => setNewCategoryName(e.target.value)}
                                    autoFocus
                                    className="w-full px-4 py-3 bg-bg border-2 border-border rounded-xl focus:border-accent outline-none transition-colors"
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-semibold text-text mb-1.5">Описание</label>
                                <textarea
                                    value={newCategoryDescription}
                                    onChange={(e) => setNewCategoryDescription(e.target.value)}
                                    rows={3}
                                    className="w-full px-4 py-3 bg-bg border-2 border-border rounded-xl focus:border-accent outline-none resize-none transition-colors"
                                />
                            </div>
                            <button
                                type="submit"
                                disabled={!newCategoryName.trim() || isCategorySubmitting}
                                className="w-full mt-2 py-3.5 bg-text text-surface font-bold rounded-xl hover:bg-accent disabled:opacity-50 transition-all"
                            >
                                {isCategorySubmitting ? 'Создание...' : 'Добавить категорию'}
                            </button>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
};