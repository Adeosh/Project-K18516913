import { useEffect, useState } from 'react';
import type { FC } from 'react';
import { useParams, Link } from 'react-router-dom';
import { orderApi, type OrderDto, type OrderItemDto } from '../api/orderApi';
import { catalogApi } from '../../catalog/api/catalogApi';
import { paymentApi } from '../../payment/api/paymentApi';
import type { Product } from '../../catalog/types/product';
import { getImageUrl } from '@/utils/imageUtils';

const OrderItemCard: FC<{ item: OrderItemDto }> = ({ item }) => {
    const [product, setProduct] = useState<Product | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        catalogApi.getById(item.productId)
            .then(setProduct)
            .catch(err => {
                console.error("Ошибка подгрузки данных товара для заказа:", err);
                setProduct(null);
            })
            .finally(() => setIsLoading(false));
    }, [item.productId]);

    const isDeleted = !isLoading && !product;
    const displayName = isLoading
        ? 'Загрузка товара...'
        : (product?.name || 'Товар больше недоступен (удален)');

    const rawImage = product?.imageUrls?.[0] || null;
    const displayImage = rawImage ? getImageUrl(rawImage) : null;
    const imageContent = displayImage ? (
        <img src={displayImage} alt={displayName} className={`w-full h-full object-contain p-1 ${isDeleted ? 'grayscale opacity-50' : ''}`} />
    ) : (
        <span className="text-[10px] text-text-muted font-medium text-center px-1">
            {isLoading ? '...' : (isDeleted ? 'Удален' : 'Нет фото')}
        </span>
    );

    const textContent = (
        <div>
            <div className={`font-bold text-sm sm:text-base line-clamp-2 transition-colors ${isDeleted ? 'text-text-muted line-through' : 'text-text hover:text-accent'}`}>
                {displayName}
            </div>
            <div className="text-xs text-text-muted mt-1 font-medium">
                {item.quantity} шт. × {item.unitPrice.toLocaleString('ru-RU')} ₽
            </div>
        </div>
    );

    return (
        <div className={`flex items-center justify-between gap-4 p-4 border border-border/50 rounded-2xl transition-all ${isDeleted ? 'bg-bg/50 opacity-70' : 'bg-bg hover:shadow-sm'}`}>
            <div className="flex items-center gap-4 flex-1">

                {isDeleted ? (
                    <div className="w-16 h-16 sm:w-20 sm:h-20 flex-shrink-0 bg-surface rounded-xl border border-border/30 flex items-center justify-center overflow-hidden">
                        {imageContent}
                    </div>
                ) : (
                    <Link to={`/product/${item.productId}`} className="w-16 h-16 sm:w-20 sm:h-20 flex-shrink-0 bg-surface rounded-xl border border-border/50 flex items-center justify-center overflow-hidden hover:border-accent transition-colors">
                        {imageContent}
                    </Link>
                )}

                {isDeleted ? (
                    <div className="flex-1 cursor-default">
                        {textContent}
                    </div>
                ) : (
                    <Link to={`/product/${item.productId}`} className="flex-1">
                        {textContent}
                    </Link>
                )}
            </div>

            <div className="text-right flex-shrink-0">
                <div className={`font-black text-base sm:text-lg ${isDeleted ? 'text-text-muted' : 'text-text'}`}>
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

    const [isPaymentLoading, setIsPaymentLoading] = useState(false);
    const [paymentError, setPaymentError] = useState<string | null>(null);

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
                    setTimeout(fetchOrder, retryDelay);
                } else {
                    setIsLoading(false);
                }
            }
        };
        void fetchOrder();
    }, [id]);

    const handlePayment = async () => {
        if (!order) return;
        setIsPaymentLoading(true);
        setPaymentError(null);

        let attempts = 0;
        const maxAttempts = 6;
        const intervalDelay = 1200;

        const tryGetUrl = async () => {
            try {
                const paymentUrl = await paymentApi.getPaymentUrl(order.id);

                if (paymentUrl) {
                    if (paymentUrl.includes('/mock-pay')) {
                        const urlObj = new URL(paymentUrl, window.location.origin);
                        window.location.href = `${window.location.origin}/mock-pay${urlObj.search}`;
                    } else {
                        window.location.href = paymentUrl;
                    }
                } else {
                    throw new Error("Ссылка не получена");
                }
            } catch (error: any) {
                if (attempts < maxAttempts) {
                    attempts++;
                    setTimeout(tryGetUrl, intervalDelay);
                } else {
                    console.error("Ошибка при получении ссылки на оплату:", error);
                    setPaymentError("Платежная система пока недоступна. Пожалуйста, попробуйте нажать кнопку еще раз чуть позже.");
                    setIsPaymentLoading(false);
                }
            }
        };

        void tryGetUrl();
    };

    if (isLoading) return <div className="text-center py-20 animate-pulse text-text-muted font-medium">Загрузка данных заказа...</div>;
    if (!order) return <div className="text-center py-20 text-error font-bold text-lg">Заказ не найден</div>;

    const statusMap: Record<string, string> = {
        'Pending': 'Ожидает оплаты',
        'AwaitingValidation': 'Проверка остатков',
        'Paid': 'Оплачен',
        'Shipped': 'Отправлен',
        'Cancelled': 'Отменен'
    };

    const canBePaid = order.status === 'Pending' || order.status === 'AwaitingValidation';

    return (
        <div className="max-w-5xl mx-auto p-4 sm:p-6 mt-4 sm:mt-8 animate-fadeIn">
            <Link to="/profile" className="text-sm font-bold text-text-muted hover:text-accent mb-6 inline-block transition-colors">
                ← Назад в профиль
            </Link>

            <div className="bg-surface rounded-3xl p-6 sm:p-10 border border-border shadow-sm">

                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-8">
                    <div>
                        <h1 className="text-2xl sm:text-3xl font-extrabold text-text">Заказ оформлен</h1>
                        <p className="text-text-muted font-medium mt-1">№ {order.id.split('-')[0].toUpperCase()}</p>
                    </div>
                    <div className={`font-bold px-4 py-2 rounded-xl text-sm ${order.status === 'Paid' ? 'bg-success/10 text-success' : 'bg-orange-100 text-orange-700'}`}>
                        {statusMap[order.status] || order.status}
                    </div>
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-10">

                    <div className="lg:col-span-2 bg-bg rounded-2xl p-6 sm:p-8 border border-border/50 flex flex-col justify-between">
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
                            <div>
                                <p className="text-sm text-text-muted font-medium mb-1">Дата заказа</p>
                                <p className="font-bold text-text text-lg">{new Date(order.createdAt).toLocaleDateString('ru-RU')}</p>
                            </div>
                            <div>
                                <p className="text-sm text-text-muted font-medium mb-1">Итого к оплате</p>
                                <p className="text-2xl font-black text-text">{order.totalPrice.toLocaleString('ru-RU')} ₽</p>
                            </div>
                            <div className="sm:col-span-2 pt-4 border-t border-border/50">
                                <p className="text-sm text-text-muted font-medium mb-1">Адрес доставки</p>
                                <p className="font-bold text-text leading-relaxed">
                                    {order.fullAddress} {order.flat && `, Кв/Офис: ${order.flat}`}
                                </p>
                            </div>
                        </div>

                        {canBePaid && (
                            <div className="mt-8 pt-6 border-t border-border/50 flex flex-col items-center justify-center">
                                <button
                                    onClick={handlePayment}
                                    disabled={isPaymentLoading}
                                    className="w-full sm:w-auto px-10 py-3.5 bg-[#9E2F1F] hover:bg-[#85281a] text-white font-bold text-lg rounded-xl shadow-md active:scale-95 transition-all disabled:opacity-70 disabled:active:scale-100"
                                >
                                    {isPaymentLoading ? 'Переход к оплате...' : 'Перейти к оплате'}
                                </button>

                                {paymentError && (
                                    <p className="text-error text-sm font-medium mt-3 text-center animate-fadeIn">
                                        {paymentError}
                                    </p>
                                )}
                            </div>
                        )}
                    </div>

                    <div className="bg-bg rounded-2xl p-6 sm:p-8 border border-border/50 flex flex-col items-center justify-center text-center">
                        <div className="text-sm font-bold text-text-muted uppercase tracking-wider mb-6">QR-код получения</div>
                        {order.qrCodeUrl ? (
                            <div className="bg-white p-3 rounded-2xl shadow-sm hover:scale-105 transition-transform duration-300">
                                <img src={order.qrCodeUrl} alt="QR Code" className="w-40 h-40 sm:w-48 sm:h-48 object-contain" />
                            </div>
                        ) : (
                            <div className="w-40 h-40 sm:w-48 sm:h-48 bg-surface rounded-2xl flex items-center justify-center border border-border">
                                <span className="text-xs text-text-muted font-medium">Генерация...</span>
                            </div>
                        )}
                        <p className="text-xs text-text-muted mt-6 font-medium max-w-[220px] leading-relaxed">
                            Покажите этот код сотруднику пункта выдачи при получении товаров
                        </p>
                    </div>

                </div>

                <div>
                    <h3 className="text-xl font-bold text-text mb-4">Состав заказа</h3>
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