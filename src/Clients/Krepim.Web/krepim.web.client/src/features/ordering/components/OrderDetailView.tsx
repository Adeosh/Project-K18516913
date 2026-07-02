import { useEffect, useState } from 'react';
import type { FC } from 'react';
import { useParams, Link } from 'react-router-dom';
import { orderApi, type OrderDto, type OrderItemDto } from '../api/orderApi';
import { catalogApi } from '../../catalog/api/catalogApi';
import type { Product } from '../../catalog/types/product';

const OrderItemCard: FC<{ item: OrderItemDto }> = ({ item }) => {
    const [product, setProduct] = useState<Product | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        catalogApi.getById(item.productId)
            .then(setProduct)
            .catch(err => console.error("Ошибка подгрузки данных товара для заказа:", err))
            .finally(() => setIsLoading(false));
    }, [item.productId]);

    const displayName = product?.name || 'Загрузка товара...';
    const displayImage = product?.imageUrls?.[0] || null;

    return (
        <div className="flex items-center justify-between gap-4 p-4 border border-border/50 rounded-2xl bg-bg/50 hover:bg-bg transition-colors">
            <div className="flex items-center gap-4 flex-1">
                <Link to={`/product/${item.productId}`} className="w-16 h-16 sm:w-20 sm:h-20 flex-shrink-0 bg-surface rounded-xl border border-border flex items-center justify-center overflow-hidden hover:border-accent transition-colors">
                    {displayImage ? (
                        <img src={displayImage} alt={displayName} className="w-full h-full object-contain p-2" />
                    ) : (
                        <span className="text-[10px] text-text-muted font-medium">{isLoading ? '...' : 'Нет фото'}</span>
                    )}
                </Link>

                <div>
                    <Link to={`/product/${item.productId}`} className="font-bold text-text text-sm sm:text-base line-clamp-2 hover:text-accent transition-colors">
                        {displayName}
                    </Link>
                    <div className="text-xs text-text-muted mt-1 font-medium">
                        {item.quantity} шт. × {item.unitPrice.toLocaleString('ru-RU')} ₽
                    </div>
                </div>
            </div>

            <div className="text-right flex-shrink-0">
                <div className="font-extrabold text-text text-base sm:text-lg">
                    {(item.quantity * item.unitPrice).toLocaleString('ru-RU')} ₽
                </div>
            </div>
        </div>
    );
};

export const OrderDetailView: FC = () => {
    const { id } = useParams<{ id: string }>();
    const [order, setOrder] = useState<OrderDto | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        if (!id) return;

        let retries = 0;
        const maxRetries = 5;
        const retryDelay = 1500;

        const fetchOrder = async () => {
            try {
                const data = await orderApi.getById(id);
                setOrder(data);
                setIsLoading(false);
            } catch (error) {
                if (retries < maxRetries) {
                    retries++;
                    console.log(`Заказ еще не создался в БД (Eventual Consistency). Попытка ретрая #${retries}...`);
                    setTimeout(fetchOrder, retryDelay);
                } else {
                    console.error("Ошибка загрузки заказа после всех попыток:", error);
                    setIsLoading(false);
                }
            }
        };

        void fetchOrder();
    }, [id]);

    if (isLoading) return <div className="text-center py-20 animate-pulse text-text-muted">Загрузка данных заказа...</div>;

    if (!order) return <div className="text-center py-20 text-error font-bold">Заказ не найден</div>;

    const statusMap: Record<string, string> = {
        'Pending': 'Ожидает обработки',
        'AwaitingValidation': 'Проверка остатков',
        'Paid': 'Оплачен',
        'Shipped': 'Отправлен',
        'Cancelled': 'Отменен'
    };

    return (
        <div className="max-w-4xl mx-auto p-4 sm:p-6 mt-8 animate-fadeIn">
            <Link to="/profile" className="text-sm font-bold text-text-muted hover:text-accent mb-6 inline-block transition-colors">
                ← Назад в профиль
            </Link>

            <div className="bg-surface rounded-3xl p-6 sm:p-10 border border-border shadow-md flex flex-col gap-10">

                <div className="flex flex-col md:flex-row gap-10 items-start">
                    <div className="flex-1 space-y-6 w-full">
                        <div>
                            <h1 className="text-3xl font-extrabold text-text mb-2">Заказ оформлен</h1>
                            <p className="text-text-muted font-medium">№ {order.id.split('-')[0].toUpperCase()}</p>
                        </div>

                        <div className="bg-bg rounded-2xl p-5 border border-border/50 space-y-3">
                            <div className="flex justify-between">
                                <span className="text-text-muted font-medium">Статус:</span>
                                <span className="font-bold text-accent">{statusMap[order.status] || order.status}</span>
                            </div>
                            <div className="flex justify-between">
                                <span className="text-text-muted font-medium">Дата заказа:</span>
                                <span className="font-bold text-text">{new Date(order.createdAt).toLocaleDateString('ru-RU')}</span>
                            </div>
                            <div className="flex justify-between border-t border-border/50 pt-3">
                                <span className="text-text-muted font-medium">Итого к оплате:</span>
                                <span className="text-xl font-extrabold text-text">{order.totalPrice.toLocaleString('ru-RU')} ₽</span>
                            </div>
                        </div>

                        <div>
                            <h3 className="text-lg font-bold text-text mb-3">Адрес доставки</h3>
                            <p className="text-text font-medium bg-bg p-4 rounded-xl border border-border/50">
                                {order.fullAddress} {order.flat && `, Кв/Офис: ${order.flat}`}
                            </p>
                        </div>
                    </div>

                    <div className="flex flex-col items-center justify-center p-6 bg-bg rounded-3xl border-2 border-dashed border-border min-w-[250px] w-full md:w-auto">
                        <div className="text-sm font-bold text-text-muted uppercase tracking-wider mb-4">QR-код заказа</div>
                        {order.qrCodeUrl ? (
                            <div className="bg-white p-2 rounded-xl shadow-sm hover:scale-105 transition-transform duration-300">
                                <img src={order.qrCodeUrl} alt="QR Code" className="w-40 h-40" />
                            </div>
                        ) : (
                            <div className="w-40 h-40 bg-surface rounded-xl flex items-center justify-center border border-border">
                                <span className="text-xs text-text-muted">Генерация...</span>
                            </div>
                        )}
                        <p className="text-xs text-center text-text-muted mt-4 font-medium max-w-[200px]">
                            Покажите этот код при получении товаров
                        </p>
                    </div>
                </div>

                <div className="pt-6 border-t border-border/50">
                    <h3 className="text-xl font-bold text-text mb-6">Состав заказа</h3>
                    <div className="space-y-3">
                        {order.items.map(item => (
                            <OrderItemCard key={item.productId} item={item} />
                        ))}
                    </div>
                </div>

            </div>
        </div>
    );
};