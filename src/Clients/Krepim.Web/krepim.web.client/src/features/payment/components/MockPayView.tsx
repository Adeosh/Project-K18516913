import { FC, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { apiClient } from '@/api/apiClient';

type PaymentStatus = 'Pending' | 'Succeeded' | 'Failed' | 'Refunded';

export const MockPayView: FC = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();

    const txId = searchParams.get('tx');
    const orderId = searchParams.get('orderId');
    const amount = searchParams.get('amount');

    const [isLoading, setIsLoading] = useState(false);
    const [selectedStatus, setSelectedStatus] = useState<PaymentStatus | null>(null);

    if (!txId || !orderId) {
        return <div className="p-10 text-center text-error font-bold">Неверная ссылка на оплату.</div>;
    }

    const handlePay = async (status: PaymentStatus) => {
        setIsLoading(true);
        setSelectedStatus(status);

        try {
            await apiClient.post('/api/payments/webhook/mock', {
                transactionId: txId,
                status
            });

            setTimeout(() => {
                navigate(`/order/${orderId}`);
            }, 1200);
        } catch (error) {
            console.error('Ошибка при симуляции платежа:', error);
            alert('Произошла ошибка связи с тестовым шлюзом.');
            setIsLoading(false);
            setSelectedStatus(null);
        }
    };

    const buttonBase =
        'w-full py-4 font-bold text-lg rounded-xl shadow-md transition-all disabled:opacity-50 disabled:cursor-not-allowed';

    return (
        <div className="min-h-screen bg-bg flex items-center justify-center p-4">
            <div className="bg-surface max-w-md w-full rounded-3xl shadow-xl border border-border p-8 text-center animate-fadeIn">
                <h2 className="text-2xl font-extrabold text-text mb-2">Оплата заказа</h2>
                <p className="text-text-muted mb-8">Магазин «Крепим.ПРО»</p>

                <div className="bg-bg rounded-2xl p-6 mb-8 border border-border/50">
                    <div className="text-sm text-text-muted mb-1">Сумма к списанию</div>
                    <div className="text-3xl font-black text-text">
                        {Number(amount).toLocaleString('ru-RU')} ₽
                    </div>
                </div>

                {selectedStatus && (
                    <div className="mb-6 rounded-xl border border-border/50 bg-bg p-3 text-sm text-text-muted">
                        Выбран статус: <span className="text-text font-semibold">{selectedStatus}</span>
                    </div>
                )}

                <div className="space-y-4">
                    <button
                        onClick={() => handlePay('Pending')}
                        disabled={isLoading}
                        className={`${buttonBase} bg-yellow-500 hover:bg-yellow-600 text-white`}
                    >
                        Имитировать "Создан"
                    </button>

                    <button
                        onClick={() => handlePay('Succeeded')}
                        disabled={isLoading}
                        className={`${buttonBase} bg-green-500 hover:bg-green-600 text-white`}
                    >
                        {isLoading && selectedStatus === 'Succeeded' ? 'Обработка...' : 'Имитировать "Успешно оплачен"'}
                    </button>

                    <button
                        onClick={() => handlePay('Failed')}
                        disabled={isLoading}
                        className={`${buttonBase} bg-transparent border-2 border-error text-error hover:bg-error/5`}
                    >
                        Имитировать "Ошибка оплаты"
                    </button>

                    <button
                        onClick={() => handlePay('Refunded')}
                        disabled={isLoading}
                        className={`${buttonBase} bg-blue-500 hover:bg-blue-600 text-white`}
                    >
                        Имитировать "Оформлен возврат"
                    </button>

                    <button
                        onClick={() => navigate(`/order/${orderId}`)}
                        disabled={isLoading}
                        className="w-full py-2 text-text-muted hover:text-text text-sm font-medium transition-colors"
                    >
                        Вернуться в заказ
                    </button>
                </div>

                <div className="mt-8 pt-6 border-t border-border/50 text-xs text-text-muted">
                    Это тестовая среда разработчика. Реальные деньги не списываются.
                </div>
            </div>
        </div>
    );
};