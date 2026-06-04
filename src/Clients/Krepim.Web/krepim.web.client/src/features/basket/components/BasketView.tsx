import { useEffect } from 'react';
import type { FC } from 'react';
import { useBasketStore } from '../store/basketStore';

export const BasketView: FC = () => {
    const { basket, fetchBasket, updateQuantity, removeItem, isLoading } = useBasketStore();

    useEffect(() => {
        void fetchBasket();
    }, []);

    const totalSum = basket?.items.reduce((sum, item) => sum + item.price * item.quantity, 0) || 0;

    if (isLoading) {
        return (
            <div className="text-center py-20 text-lg font-medium text-text-muted animate-pulse">
                Синхронизация корзины...
            </div>
        );
    }

    if (!basket || basket.items.length === 0) {
        return (
            <div className="max-w-4xl mx-auto p-6 text-center py-20 bg-surface rounded-2xl border border-dashed border-border text-text-muted mt-8">
                Ваша корзина пуста. Перейдите в каталог, чтобы добавить товары.
            </div>
        );
    }

    return (
        <div className="max-w-5xl mx-auto p-6">
            <h2 className="text-3xl font-bold text-text mb-8">
                Корзина
            </h2>

            <div className="space-y-4">
                {basket.items.map((item) => (
                    <div
                        key={item.productId}
                        className="bg-surface rounded-2xl p-5 border border-border shadow-sm flex flex-col md:flex-row justify-between items-start md:items-center gap-6 transition-colors hover:border-border-focus"
                    >
                        <div className="flex-1">
                            <div className="text-xs text-text-muted mb-1.5 font-medium">
                                Арт: {item.sku} <span className="mx-2 text-border-focus">•</span> Бренд: {item.brand}
                            </div>
                            <div className="font-bold text-text text-lg leading-tight">{item.name}</div>
                        </div>

                        <div className="flex items-center gap-1 bg-bg border border-border/50 rounded-xl p-1">
                            <button
                                onClick={() => void updateQuantity(item.productId, item.quantity - 1)}
                                className="w-8 h-8 flex items-center justify-center text-text-muted hover:text-text hover:bg-surface rounded-lg transition-colors font-bold text-xl"
                            >
                                −
                            </button>
                            <span className="w-10 text-center text-text font-bold">{item.quantity}</span>
                            <button
                                onClick={() => void updateQuantity(item.productId, item.quantity + 1)}
                                className="w-8 h-8 flex items-center justify-center text-text-muted hover:text-text hover:bg-surface rounded-lg transition-colors font-bold text-xl"
                            >
                                +
                            </button>
                        </div>

                        <div className="text-right min-w-[140px]">
                            <div className="text-xl font-bold text-text">
                                {(item.price * item.quantity).toLocaleString('ru-RU')} ₽
                            </div>
                            <div className="text-sm text-text-muted mt-0.5">{item.price.toLocaleString('ru-RU')} ₽ / шт.</div>
                        </div>

                        <button
                            onClick={() => void removeItem(item.productId)}
                            className="text-error bg-error/5 hover:bg-error/10 px-4 py-2 rounded-xl text-sm font-semibold transition-colors"
                        >
                            Удалить
                        </button>
                    </div>
                ))}
            </div>

            <div className="mt-8 bg-surface rounded-3xl border border-border shadow-md p-6 sm:p-8 flex flex-col sm:flex-row justify-between items-center gap-6">
                <div>
                    <div className="text-sm text-text-muted font-medium mb-1">Итого к оплате</div>
                    <div className="text-4xl font-extrabold text-accent">
                        {totalSum.toLocaleString('ru-RU')} ₽
                    </div>
                </div>
                <button
                    onClick={() => console.log('Инициализация Checkout через MassTransit')}
                    className="w-full sm:w-auto px-10 py-4 bg-accent text-surface font-bold text-lg rounded-xl shadow-lg hover:shadow-xl hover:scale-[1.02] transition-all"
                >
                    Оформить заказ
                </button>
            </div>
        </div>
    );
};