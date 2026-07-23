import { useState, useEffect, useMemo } from 'react';
import type { FC } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { catalogApi } from '../api/catalogApi';
import { inventoryApi } from '../api/inventoryApi';
import type { Product } from '../types/product';
import { useBasketStore } from '../../basket/store/basketStore';
import { getImageUrl } from '@/utils/imageUtils';

const parseJsonField = (field: any, fallback: any) => {
    if (typeof field === 'string') {
        try { return JSON.parse(field); } catch { return fallback; }
    }
    return field || fallback;
};

const getToggleLabels = (unit: number) => {
    switch (unit) {
        case 1: return ['Поштучно', 'Оптом'];
        case 2: return ['В розницу', 'Крупным оптом'];
        case 3: return ['Свой вес', 'Фасовкой'];
        default: return ['Поштучно', 'Оптом'];
    }
};

export const ProductDetailView: FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [product, setProduct] = useState<Product | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [mainImage, setMainImage] = useState<string | undefined>(undefined);

    const [inputQty, setInputQty] = useState<string>('0');
    const [isPackageMode, setIsPackageMode] = useState(false);
    const [availableStock, setAvailableStock] = useState<number | null>(null);

    const addItemToBasket = useBasketStore((state) => state.addItem);

    useEffect(() => {
        if (!id) return;
        const loadData = async () => {
            setIsLoading(true);
            try {
                const [catalogData, stockData] = await Promise.all([
                    catalogApi.getById(id),
                    inventoryApi.getStock(id)
                ]);

                catalogData.priceTiers = parseJsonField(catalogData.priceTiers ?? (catalogData as any).PriceTiers, []);
                catalogData.attributes = parseJsonField(catalogData.attributes ?? (catalogData as any).Attributes, {});

                setProduct(catalogData);
                setAvailableStock(stockData);

                if (catalogData.imageUrls && catalogData.imageUrls.length > 0) {
                    setMainImage(catalogData.imageUrls[0]);
                }

                setInputQty('0');
                setIsPackageMode(false);
            } catch (err) {
                console.error('Ошибка загрузки данных товара', err);
            } finally {
                setIsLoading(false);
            }
        };
        void loadData();
    }, [id]);

    const quantity = useMemo(() => {
        if (!product) return 0;
        const parsed = parseInt(inputQty, 10);
        return isNaN(parsed) ? 0 : parsed;
    }, [inputQty, product]);

    const activeTier = useMemo(() => {
        if (!product) return null;
        const tiers = product.priceTiers || (product as any).PriceTiers || [];
        if (!Array.isArray(tiers) || tiers.length === 0) return null;

        const sortedTiers = [...tiers].sort((a: any, b: any) => {
            const minA = a.minQuantity ?? a.MinQuantity ?? 0;
            const minB = b.minQuantity ?? b.MinQuantity ?? 0;
            return minB - minA;
        });

        return sortedTiers.find((t: any) => {
            const minQ = t.minQuantity ?? t.MinQuantity ?? 0;
            return quantity >= minQ;
        }) || null;
    }, [product, quantity]);

    const currentPrice = useMemo(() => {
        if (!product) return 0;
        const basePrice = product.price ?? (product as any).Price ?? 0;
        if (!activeTier) return basePrice;

        const tierPrice = activeTier.amount ?? activeTier.Amount ?? activeTier.price?.amount ?? activeTier.Price?.Amount ?? 0;
        return tierPrice > 0 ? tierPrice : basePrice;
    }, [product, activeTier]);

    const currentStep = useMemo(() => {
        if (!product) return 1;
        return isPackageMode && product.salesStep > 0 ? product.salesStep : 1;
    }, [isPackageMode, product]);

    if (isLoading) return <div className="p-20 text-center animate-pulse font-medium text-lg text-text-muted">Загрузка карточки товара...</div>;

    if (!product) {
        return (
            <div className="p-20 text-center">
                <h2 className="text-2xl font-bold text-text mb-4">Товар не найден</h2>
                <button onClick={() => navigate(-1)} className="text-accent hover:underline font-semibold">Вернуться в каталог</button>
            </div>
        );
    }

    const getUnitText = (unit: number) => {
        switch (unit) { case 1: return 'шт.'; case 2: return 'упак.'; case 3: return 'кг'; default: return 'шт.'; }
    };

    const handleModeSwitch = (toPackage: boolean) => {
        setIsPackageMode(toPackage);
    };

    const handleQuantityButtons = (delta: number) => {
        const startVal = quantity === 0 ? 0 : quantity;
        let next = Math.max(0, startVal + delta * currentStep);

        if (availableStock !== null && next > availableStock) {
            next = availableStock;
        }

        setInputQty(String(next));
    };

    const handleInputBlur = () => {
        let val = parseInt(inputQty, 10);
        if (isNaN(val) || val < 0) val = 0;

        if (availableStock !== null && val > availableStock) {
            val = availableStock;
        }

        setInputQty(String(val));
    };

    const handleAddToCart = () => {
        void addItemToBasket({
            productId: product.id,
            sku: product.sku,
            name: product.name,
            brand: product.brand || 'Без бренда',
            price: currentPrice,
            imageUrl: product.imageUrls?.[0],
            quantity: quantity
        });
        alert(`Добавлено в корзину: ${quantity} ${getUnitText(product.salesUnit)} по ${currentPrice} ₽`);
    };

    const [labelSingle, labelPackage] = getToggleLabels(product.salesUnit);

    return (
        <div className="max-w-7xl mx-auto p-4 sm:p-6 space-y-6">
            <nav className="text-sm font-medium text-text-muted mb-4 flex items-center gap-2">
                <button onClick={() => navigate(-1)} className="hover:text-accent transition-colors">← Назад</button>
                <span>/</span>
                <span className="text-text font-semibold">{product.sku}</span>
            </nav>

            <div className="bg-surface rounded-3xl border border-border shadow-sm overflow-hidden">
                <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 p-6 md:p-10">

                    <div className="lg:col-span-5 space-y-4">
                        <div className="aspect-square bg-bg rounded-2xl border border-border flex items-center justify-center overflow-hidden relative">
                            {mainImage ? (
                                <img src={getImageUrl(mainImage)} alt={product.name} className="w-full h-full object-contain p-4" />
                            ) : (
                                <span className="text-text-muted font-medium">Нет фото</span>
                            )}
                        </div>

                        {product.imageUrls && product.imageUrls.length > 1 && (
                            <div className="flex gap-3 overflow-x-auto pb-2 scrollbar-thin">
                                {product.imageUrls.map((url, idx) => (
                                    <button
                                        key={idx}
                                        onClick={() => setMainImage(url)}
                                        className={`w-20 h-20 flex-shrink-0 rounded-xl border-2 overflow-hidden transition-all ${mainImage === url ? 'border-accent' : 'border-border opacity-70 hover:opacity-100'}`}
                                    >
                                        <img src={getImageUrl(url)} alt={`thumb-${idx}`} className="w-full h-full object-cover" />
                                    </button>
                                ))}
                            </div>
                        )}
                    </div>

                    <div className="lg:col-span-7 flex flex-col">
                        <div className="mb-4 flex flex-wrap items-center gap-3">
                            <span className="bg-bg text-text-muted px-3 py-1 rounded-lg text-sm font-mono font-bold border border-border">Арт: {product.sku}</span>
                            {product.standard && <span className="bg-sand/30 text-text-muted px-3 py-1 rounded-lg text-sm font-bold border border-border">{product.standard}</span>}
                            {product.brand && <span className="text-accent text-sm font-bold">{product.brand}</span>}
                            {availableStock !== null && (
                                <span className={`px-3 py-1 rounded-lg text-sm font-bold border ${availableStock > 0 ? 'bg-success/10 text-success border-success/20' : 'bg-error/10 text-error border-error/20'}`}>
                                    {availableStock > 0 ? `В наличии: ${availableStock.toLocaleString('ru-RU')} ${getUnitText(product.salesUnit)}` : 'Нет в наличии'}
                                </span>
                            )}
                        </div>

                        <h1 className="text-3xl sm:text-4xl font-extrabold text-text mb-6 leading-tight">{product.name}</h1>

                        <div className="bg-bg/50 p-6 rounded-2xl border border-border/50 mb-8 flex flex-col sm:flex-row sm:items-start justify-between gap-6">
                            <div>
                                <div className="text-4xl font-black text-text flex items-baseline gap-2">
                                    {currentPrice.toLocaleString('ru-RU')} <span className="text-2xl text-text-muted">₽</span>
                                    <span className="text-sm font-medium text-text-muted font-sans">/ {getUnitText(product.salesUnit)}</span>
                                </div>

                                {activeTier && (
                                    <div className="text-sm text-accent font-bold mt-1 transition-all duration-300">
                                        С учетом скидки (от {activeTier.minQuantity ?? (activeTier as any).MinQuantity} {getUnitText(product.salesUnit)})
                                    </div>
                                )}

                                {currentPrice < product.price && (
                                    <div className="text-sm text-text-muted line-through mt-1">
                                        Базовая цена: {product.price.toLocaleString('ru-RU')} ₽
                                    </div>
                                )}
                            </div>

                            <div className="flex flex-col gap-4 min-h-[110px] justify-end">

                                <div className="h-10 flex items-center">
                                    {product.salesStep > 1 && (
                                        <div className="flex bg-surface p-1.5 rounded-xl border border-border w-fit shadow-sm gap-2">
                                            <button
                                                onClick={() => handleModeSwitch(false)}
                                                className={`px-5 py-2 text-sm font-bold rounded-lg transition-all duration-300 ease-out border ${!isPackageMode ? 'bg-bg text-text shadow-sm border-border/50' : 'text-text-muted hover:text-text hover:bg-bg/40 border-transparent'}`}
                                            >
                                                {labelSingle}
                                            </button>
                                            <button
                                                onClick={() => handleModeSwitch(true)}
                                                className={`px-5 py-2 text-sm font-bold rounded-lg transition-all duration-300 ease-out flex items-center gap-2 border ${isPackageMode ? 'bg-bg text-text shadow-sm border-border/50' : 'text-text-muted hover:text-text hover:bg-bg/40 border-transparent'}`}
                                            >
                                                {labelPackage}
                                                <span className={`text-[11px] px-2 py-0.5 rounded-full transition-colors duration-300 ${isPackageMode ? 'bg-accent/15 text-accent' : 'bg-border/50 text-text-muted'}`}>
                                                    х{product.salesStep}
                                                </span>
                                            </button>
                                        </div>
                                    )}
                                </div>

                                <div className="flex items-center gap-3">
                                    <div className="flex items-center bg-surface border border-border rounded-xl p-1 w-full sm:w-auto focus-within:border-accent transition-colors">
                                        <button
                                            onClick={() => handleQuantityButtons(-1)}
                                            disabled={quantity <= 0}
                                            className="w-11 h-11 flex items-center justify-center text-text-muted hover:text-text font-bold text-xl disabled:opacity-30 transition-colors"
                                        >
                                            -
                                        </button>
                                        <input
                                            type="number"
                                            value={inputQty}
                                            onChange={(e) => setInputQty(e.target.value)}
                                            onBlur={handleInputBlur}
                                            className="flex-1 w-20 text-center font-bold text-text bg-transparent outline-none [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
                                        />
                                        <button
                                            onClick={() => handleQuantityButtons(1)}
                                            className="w-11 h-11 flex items-center justify-center text-text-muted hover:text-text font-bold text-xl transition-colors"
                                        >
                                            +
                                        </button>
                                    </div>

                                    <button
                                        onClick={handleAddToCart}
                                        disabled={quantity < 1 || availableStock === 0}
                                        className={`w-full sm:w-auto px-8 py-3.5 font-bold text-lg rounded-xl shadow-md transition-all 
                                                ${quantity < 1 || availableStock === 0
                                                ? 'bg-border text-text-muted cursor-not-allowed opacity-60'
                                                : 'bg-accent text-surface hover:shadow-lg hover:bg-accent/90 hover:-translate-y-0.5'}`}
                                        >
                                        {availableStock === 0 ? 'Нет в наличии' : 'В корзину'}
                                    </button>
                                </div>
                            </div>
                        </div>

                        {product.priceTiers && Array.isArray(product.priceTiers) && product.priceTiers.length > 0 && (
                            <div className="mb-8">
                                <h3 className="text-sm font-bold text-text-muted mb-3 uppercase tracking-wider">Оптовые скидки</h3>
                                <div className="flex flex-wrap gap-3">
                                    {product.priceTiers.map((tier: any, idx: number) => {
                                        const minQty = tier.minQuantity ?? tier.MinQuantity;
                                        const tierPrice = tier.amount ?? tier.Amount ?? tier.price?.amount ?? tier.Price?.Amount ?? 0;
                                        const isActive = activeTier?.minQuantity === minQty || (activeTier as any)?.MinQuantity === minQty;

                                        return (
                                            <div key={idx} className={`bg-surface border px-4 py-2 rounded-xl flex items-center gap-3 transition-colors duration-300 ${isActive ? 'border-accent bg-accent/5' : 'border-border'}`}>
                                                <span className="text-sm text-text-muted font-medium">от {minQty} {getUnitText(product.salesUnit)}</span>
                                                <span className="text-sm font-bold text-text">{Number(tierPrice).toLocaleString('ru-RU')} ₽</span>
                                            </div>
                                        );
                                    })}
                                </div>
                            </div>
                        )}

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                            {product.attributes && Object.keys(product.attributes).length > 0 && (
                                <div>
                                    <h3 className="text-lg font-bold text-text mb-4 border-b border-border pb-2">Характеристики</h3>
                                    <div className="space-y-2">
                                        {Object.entries(product.attributes).map(([key, value]) => (
                                            <div key={key} className="flex justify-between items-end border-b border-border/50 pb-2 border-dotted">
                                                <span className="text-sm text-text-muted">{key}</span>
                                                <span className="text-sm font-semibold text-text text-right max-w-[60%]">{String(value)}</span>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            )}

                            <div>
                                <h3 className="text-lg font-bold text-text mb-4 border-b border-border pb-2">Описание</h3>
                                <p className="text-sm text-text-muted leading-relaxed whitespace-pre-wrap">
                                    {product.description || 'Описание товара пока не добавлено.'}
                                </p>
                            </div>
                        </div>

                    </div>
                </div>
            </div>
        </div>
    );
};