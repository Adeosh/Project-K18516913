import { useEffect, useState } from 'react';
import type { FC } from 'react';
import { useBasketStore } from '../store/basketStore';
import { catalogApi } from '../../catalog/api/catalogApi';
import type { BasketItem } from '../types/basket';

const calculatePrice = (product: any, qty: number): number => {
    const tiers = typeof product.priceTiers === 'string' ? JSON.parse(product.priceTiers) : (product.priceTiers || []);
    const basePrice = product.price ?? 0;

    if (!Array.isArray(tiers) || tiers.length === 0) return basePrice;

    const sorted = [...tiers].sort((a: any, b: any) => (b.minQuantity ?? b.MinQuantity ?? 0) - (a.minQuantity ?? a.MinQuantity ?? 0));
    const active = sorted.find((t: any) => qty >= (t.minQuantity ?? t.MinQuantity ?? 0));

    if (!active) return basePrice;

    const tierPrice = active.amount ?? active.Amount ?? active.price?.amount ?? active.Price?.Amount ?? 0;
    return tierPrice > 0 ? tierPrice : basePrice;
};

const BasketItemCard: FC<{
    item: BasketItem;
    updateQuantity: (id: string, qty: number, price: number) => Promise<void>;
    removeItem: (id: string) => Promise<void>;
}> = ({ item, updateQuantity, removeItem }) => {
    const [localQty, setLocalQty] = useState<string>(String(item.quantity));
    const [isUpdating, setIsUpdating] = useState(false);

    useEffect(() => {
        setLocalQty(String(item.quantity));
    }, [item.quantity]);

    const handleSmartQuantityChange = async (delta: number, manualValue?: number) => {
        setIsUpdating(true);
        try {
            const productInfo = await catalogApi.getById(item.productId);

            let newQty = item.quantity;
            if (manualValue !== undefined) {
                newQty = isNaN(manualValue) || manualValue < 1 ? 1 : manualValue;
            } else {
                newQty = Math.max(1, item.quantity + delta);
            }

            setLocalQty(String(newQty));

            const newPrice = calculatePrice(productInfo, newQty);
            await updateQuantity(item.productId, newQty, newPrice);

        } catch (error) {
            console.error("Ошибка при умном пересчете:", error);
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

    return (
        <div className={`bg-surface rounded-2xl p-4 sm:p-5 border shadow-sm flex flex-col md:flex-row justify-between items-start md:items-center gap-6 transition-all duration-300 ${isUpdating ? 'opacity-50 pointer-events-none' : 'border-border hover:border-border-focus'}`}>

            <div className="flex items-center gap-4 flex-1 w-full md:w-auto">

                <div className="w-20 h-20 sm:w-24 sm:h-24 flex-shrink-0 bg-bg rounded-xl border border-border flex items-center justify-center overflow-hidden relative">
                    {item.imageUrl ? (
                        <img src={item.imageUrl} alt={item.name} className="w-full h-full object-contain p-2" />
                    ) : (
                        <span className="text-[10px] text-text-muted font-medium">Нет фото</span>
                    )}
                </div>

                <div>
                    <div className="text-xs text-text-muted mb-1.5 font-medium flex flex-wrap gap-x-2">
                        <span>Арт: {item.sku}</span>
                        <span className="text-border-focus hidden sm:inline">•</span>
                        <span className="text-accent">{item.brand}</span>
                    </div>
                    <div className="font-bold text-text text-base sm:text-lg leading-tight line-clamp-2">{item.name}</div>
                </div>
            </div>

            <div className="flex items-center gap-1 bg-bg border border-border/50 rounded-xl p-1 focus-within:border-accent transition-colors self-start md:self-auto">
                <button
                    onClick={() => void handleSmartQuantityChange(-1)}
                    disabled={item.quantity <= 1}
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
                    <div className="text-xl font-bold text-text">{(item.price * item.quantity).toLocaleString('ru-RU')} ₽</div>
                    <div className="text-sm text-text-muted mt-0.5 whitespace-nowrap">{item.price.toLocaleString('ru-RU')} ₽ / шт.</div>
                </div>

                <button onClick={() => void removeItem(item.productId)} className="text-error bg-error/5 hover:bg-error/10 px-4 py-2 rounded-xl text-sm font-semibold transition-colors mt-0 md:mt-3">
                    Удалить
                </button>
            </div>
        </div>
    );
};

export const BasketView: FC = () => {
    const { basket, fetchBasket, updateQuantity, removeItem, isLoading } = useBasketStore();

    useEffect(() => {
        void fetchBasket();
    }, []);

    const totalSum = basket?.items.reduce((sum, item) => sum + item.price * item.quantity, 0) || 0;

    if (isLoading) return <div className="text-center py-20 text-lg font-medium text-text-muted animate-pulse">Синхронизация корзины...</div>;

    if (!basket || basket.items.length === 0) {
        return <div className="max-w-4xl mx-auto p-6 text-center py-20 bg-surface rounded-2xl border border-dashed border-border text-text-muted mt-8">Ваша корзина пуста. Перейдите в каталог, чтобы добавить товары.</div>;
    }

    return (
        <div className="max-w-5xl mx-auto p-4 sm:p-6">
            <h2 className="text-3xl font-bold text-text mb-8">Корзина</h2>

            <div className="space-y-4">
                {basket.items.map((item) => (
                    <BasketItemCard
                        key={item.productId}
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
                <button className="w-full sm:w-auto px-10 py-4 bg-accent text-surface font-bold text-lg rounded-xl shadow-lg hover:shadow-xl transition-all hover:-translate-y-0.5">
                    Оформить заказ
                </button>
            </div>
        </div>
    );
};