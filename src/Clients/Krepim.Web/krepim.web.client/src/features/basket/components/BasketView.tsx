import { useEffect, useState, useRef } from 'react';
import type { FC } from 'react';
import { useNavigate } from 'react-router-dom';
import { useBasketStore } from '../store/basketStore';
import { catalogApi } from '../../catalog/api/catalogApi';
import { useProfileStore } from '../../profile/store/profileStore';
import { getImageUrl } from '@/utils/imageUtils';
import type { BasketItem } from '../types/basket';
import type { Product } from '../../catalog/types/product';

interface PriceTier {
    minQuantity?: number;
    MinQuantity?: number;
    amount?: number;
    Amount?: number;
    price?: { amount?: number };
    Price?: { Amount?: number };
}

type ExtendedProduct = Partial<Product> & {
    priceTiers?: string | PriceTier[];
    PriceTiers?: string | PriceTier[];
    price?: number;
    Price?: number;
};

const calculatePrice = (product: ExtendedProduct | null | undefined, qty: number): number => {
    try {
        if (!product) return 0;

        const rawTiers = product.priceTiers || product.PriceTiers;
        const tiers: PriceTier[] = typeof rawTiers === 'string' ? JSON.parse(rawTiers) : (rawTiers || []);

        const basePrice = product.price ?? product.Price ?? 0;

        if (!Array.isArray(tiers) || tiers.length === 0) return basePrice;

        const sorted = [...tiers].sort((a, b) =>
            (b.minQuantity ?? b.MinQuantity ?? 0) - (a.minQuantity ?? a.MinQuantity ?? 0)
        );
        const active = sorted.find((t) => qty >= (t.minQuantity ?? t.MinQuantity ?? 0));

        if (!active) return basePrice;

        const tierPrice = active.amount ?? active.Amount ?? active.price?.amount ?? active.Price?.Amount ?? 0;
        return tierPrice > 0 ? tierPrice : basePrice;
    } catch (e: unknown) {
        if (e instanceof Error) console.error('Ошибка расчета цены:', e.message);
        return product?.price ?? product?.Price ?? 0;
    }
};

const BasketItemCard: FC<{
    item: BasketItem;
    updateQuantity: (id: string, qty: number, price: number) => Promise<void>;
    removeItem: (id: string) => Promise<void>;
}> = ({ item, updateQuantity, removeItem }) => {
    const [localQty, setLocalQty] = useState<string>(String(item?.quantity ?? 1));
    const [isUpdating, setIsUpdating] = useState(false);
    const [enrichedData, setEnrichedData] = useState<Product | null>(null);
    const [prevItemQuantity, setPrevItemQuantity] = useState(item?.quantity);

    const updateQuantityRef = useRef(updateQuantity);

    useEffect(() => {
        updateQuantityRef.current = updateQuantity;
    }, [updateQuantity]);

    if (item?.quantity !== prevItemQuantity) {
        setPrevItemQuantity(item?.quantity);
        setLocalQty(String(item?.quantity ?? 1));
    }

    useEffect(() => {
        let isMounted = true;
        const isNameInvalid = !item.name || item.name === 'Неизвестный товар' || item.name === 'Без названия';
        const needsEnrichment = isNameInvalid || !item.imageUrl || !item.price || item.price === 0;

        if (needsEnrichment) {
            catalogApi.getById(item.productId)
                .then(async (productInfo) => {
                    if (!isMounted) return;
                    setEnrichedData(productInfo);

                    if (!item.price || item.price === 0) {
                        const correctPrice = calculatePrice(productInfo as ExtendedProduct, item.quantity);
                        await updateQuantityRef.current(item.productId, item.quantity, correctPrice);
                    }
                })
                .catch(err => console.error("Ошибка загрузки данных из каталога:", err));
        }

        return () => { isMounted = false; };
    }, [item.productId, item.name, item.imageUrl, item.price, item.quantity]);

    const handleSmartQuantityChange = async (delta: number, manualValue?: number) => {
        setIsUpdating(true);
        try {
            const productInfo = enrichedData || await catalogApi.getById(item.productId);
            if (!enrichedData) setEnrichedData(productInfo);

            let newQty = item.quantity;
            if (manualValue !== undefined) {
                newQty = isNaN(manualValue) || manualValue < 1 ? 1 : manualValue;
            } else {
                newQty = Math.max(1, item.quantity + delta);
            }

            setLocalQty(String(newQty));

            const newPrice = calculatePrice(productInfo as ExtendedProduct, newQty);
            await updateQuantity(item.productId, newQty, newPrice);

        } catch (error: unknown) {
            if (error instanceof Error) console.error("Ошибка при умном пересчете:", error.message);
            setLocalQty(String(item.quantity));
        } finally {
            setIsUpdating(false);
        }
    };

    const handleBlur = () => {
        const val = parseInt(localQty, 10);
        if (isNaN(val) || val < 1) {
            setLocalQty(String(item.quantity));
            return;
        }
        if (val !== item.quantity) {
            void handleSmartQuantityChange(0, val);
        } else {
            setLocalQty(String(val));
        }
    };

    const isItemNameInvalid = !item?.name || item?.name === 'Неизвестный товар' || item?.name === 'Без названия';
    const displayName = enrichedData?.name || (isItemNameInvalid ? 'Загрузка...' : item?.name);
    const displayBrand = enrichedData?.brand || item?.brand || 'Без бренда';
    const displaySku = enrichedData?.sku || item?.sku || 'Н/Д';
    const displayImage = enrichedData?.imageUrls?.[0] || item?.imageUrl || null;
    const safePrice = (item?.price && item.price > 0) ? item.price : (enrichedData ? calculatePrice(enrichedData as ExtendedProduct, item.quantity) : 0);
    const safeQuantity = item?.quantity ?? 1;
    const itemTotal = safePrice * safeQuantity;

    return (
        <div className={`bg-surface rounded-2xl p-4 sm:p-5 border shadow-sm flex flex-col md:flex-row justify-between items-start md:items-center gap-6 transition-all duration-300 ${isUpdating ? 'opacity-50 pointer-events-none' : 'border-border hover:border-border-focus'}`}>

            <div className="flex items-center gap-4 flex-1 w-full md:w-auto">

                <div className="w-20 h-20 sm:w-24 sm:h-24 flex-shrink-0 bg-bg rounded-xl border border-border flex items-center justify-center overflow-hidden relative">
                    {displayImage ? (
                        <img src={getImageUrl(displayImage)} alt={displayName} className="w-full h-full object-contain p-2" />
                    ) : (
                        <span className="text-[10px] text-text-muted font-medium">Нет фото</span>
                    )}
                </div>

                <div>
                    <div className="text-xs text-text-muted mb-1.5 font-medium flex flex-wrap gap-x-2">
                        <span>Арт: {displaySku}</span>
                        <span className="text-border-focus hidden sm:inline">•</span>
                        <span className="text-accent">{displayBrand}</span>
                    </div>
                    <div className="font-bold text-text text-base sm:text-lg leading-tight line-clamp-2">{displayName}</div>
                </div>
            </div>

            <div className="flex items-center gap-1 bg-bg border border-border/50 rounded-xl p-1 focus-within:border-accent transition-colors self-start md:self-auto">
                <button
                    onClick={() => void handleSmartQuantityChange(-1)}
                    disabled={safeQuantity <= 1}
                    className="w-10 h-10 flex items-center justify-center text-text-muted hover:text-text font-bold text-xl disabled:opacity-30 transition-colors"
                >
                    −
                </button>
                <input
                    type="number"
                    value={localQty}
                    onChange={(e) => setLocalQty(e.target.value)}
                    onBlur={handleBlur}
                    className="w-14 sm:w-16 text-center text-text font-bold bg-transparent outline-none [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
                />
                <button
                    onClick={() => void handleSmartQuantityChange(1)}
                    className="w-10 h-10 flex items-center justify-center text-text-muted hover:text-text font-bold text-xl transition-colors"
                >
                    +
                </button>
            </div>

            <div className="flex flex-row md:flex-col justify-between md:justify-center items-center md:items-end w-full md:w-auto gap-4 md:gap-0">
                <div className="text-right min-w-[120px]">
                    <div className="text-xl font-bold text-text">
                        {itemTotal.toLocaleString('ru-RU')} ₽
                    </div>
                    <div className="text-sm text-text-muted mt-0.5 whitespace-nowrap">
                        {safePrice.toLocaleString('ru-RU')} ₽ / шт.
                    </div>
                </div>

                <button onClick={() => void removeItem(item.productId)} className="text-error bg-error/5 hover:bg-error/10 px-4 py-2 rounded-xl text-sm font-semibold transition-colors mt-0 md:mt-3">
                    Удалить
                </button>
            </div>
        </div>
    );
};

export const BasketView: FC = () => {
    const { basket, fetchBasket, updateQuantity, removeItem, isLoading, checkout } = useBasketStore();
    const profile = useProfileStore(state => state.profile);
    const fetchProfile = useProfileStore(state => state.fetchProfile);
    const navigate = useNavigate();
    const [isCheckingOut, setIsCheckingOut] = useState(false);

    useEffect(() => {
        void fetchBasket();
    }, [fetchBasket]);

    useEffect(() => {
        if (!profile) {
            void fetchProfile();
        }
    }, [profile, fetchProfile]);

    const handleCheckout = async () => {
        if (!profile?.defaultAddress?.fullAddress) {
            alert('Пожалуйста, заполните адрес доставки в профиле перед оформлением заказа.');
            navigate('/profile');
            return;
        }

        if (!profile.email) {
            alert('Укажите email в профиле.');
            navigate('/profile');
            return;
        }

        setIsCheckingOut(true);
        try {
            const newOrderId = await checkout({
                customerEmail: profile.email,
                customerPhone: profile.phoneNumber || null,
                fullAddress: profile.defaultAddress.fullAddress,
                latitude: profile.defaultAddress.latitude!,
                longitude: profile.defaultAddress.longitude!,
                flat: profile.defaultAddress.flat || null
            });

            if (newOrderId) {
                navigate(`/order/${newOrderId}`);
            } else {
                navigate('/profile');
            }
        } catch {
            alert('Не удалось оформить заказ. Попробуйте позже.');
        } finally {
            setIsCheckingOut(false);
        }
    };

    const totalSum = basket?.items?.reduce((sum, item) => {
        const p = item?.price ?? 0;
        const q = item?.quantity ?? 0;
        return sum + (p * q);
    }, 0) || 0;

    if (isLoading && !isCheckingOut) return <div className="text-center py-20 text-lg font-medium text-text-muted animate-pulse">Синхронизация корзины...</div>;

    if (!basket || !Array.isArray(basket.items) || basket.items.length === 0) {
        return <div className="max-w-4xl mx-auto p-6 text-center py-20 bg-surface rounded-2xl border border-dashed border-border text-text-muted mt-8">Ваша корзина пуста. Перейдите в каталог, чтобы добавить товары.</div>;
    }

    return (
        <div className="max-w-5xl mx-auto p-4 sm:p-6">
            <h2 className="text-3xl font-bold text-text mb-8">Корзина</h2>

            <div className="space-y-4">
                {basket.items.map((item, index) => (
                    <BasketItemCard
                        key={item?.productId || `basket-item-${index}`}
                        item={item}
                        updateQuantity={updateQuantity}
                        removeItem={removeItem}
                    />
                ))}
            </div>

            <div className="mt-8 bg-surface rounded-3xl border border-border shadow-md p-6 sm:p-8 flex flex-col sm:flex-row justify-between items-center gap-6">
                <div>
                    <div className="text-sm text-text-muted font-medium mb-1">Итого к оплате</div>
                    <div className="text-4xl font-extrabold text-accent">{totalSum.toLocaleString('ru-RU')} ₽</div>
                </div>
                <button
                    onClick={() => void handleCheckout()}
                    disabled={isCheckingOut || totalSum === 0}
                    className="w-full sm:w-auto px-10 py-4 bg-accent text-surface font-bold text-lg rounded-xl shadow-lg hover:shadow-xl transition-all hover:-translate-y-0.5 disabled:opacity-50"
                >
                    {isCheckingOut ? 'Оформление...' : 'Оформить заказ'}
                </button>
            </div>
        </div>
    );
};