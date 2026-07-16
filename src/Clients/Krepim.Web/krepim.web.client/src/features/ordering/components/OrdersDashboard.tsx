import { useState, useEffect } from 'react';
import type { FC } from 'react';
import { Link } from 'react-router-dom';
import { orderApi, type OrderDto } from '../../ordering/api/orderApi';

const orderStatusMap: Record<string, { label: string, color: string }> = {
    'Pending': { label: 'Ожидает обработки', color: 'bg-orange-100 text-orange-700 border-orange-200' },
    'AwaitingValidation': { label: 'Проверка остатков', color: 'bg-blue-100 text-blue-700 border-blue-200' },
    'Paid': { label: 'Оплачен', color: 'bg-green-100 text-green-700 border-green-200' },
    'Shipped': { label: 'Отправлен', color: 'bg-purple-100 text-purple-700 border-purple-200' },
    'Cancelled': { label: 'Отменен', color: 'bg-error/10 text-error border-error/20' }
};

export const OrdersDashboard: FC = () => {
    const [orders, setOrders] = useState<OrderDto[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    const [searchQuery, setSearchQuery] = useState('');
    const [statusFilter, setStatusFilter] = useState('');

    useEffect(() => {
        const fetchManagerOrders = async () => {
            setIsLoading(true);
            try {
                const data = await orderApi.getAllOrders();

                if (Array.isArray(data)) {
                    const sorted = data.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
                    setOrders(sorted);
                }
            } catch (error) {
                console.error('Ошибка при загрузке заказов менеджера:', error);
            } finally {
                setIsLoading(false);
            }
        };

        void fetchManagerOrders();
    }, []);

    const filteredOrders = orders.filter(order => {
        const query = searchQuery.toLowerCase();

        const matchesSearch =
            order.id.toLowerCase().includes(query) ||
            order.fullAddress.toLowerCase().includes(query) ||
            order.customerEmail.toLowerCase().includes(query) ||
            (order.customerPhone && order.customerPhone.toLowerCase().includes(query));

        const matchesStatus = statusFilter ? order.status === statusFilter : true;

        return matchesSearch && matchesStatus;
    });

    return (
        <div className="max-w-7xl mx-auto p-4 sm:p-6 space-y-6 animate-fadeIn">
            <div className="bg-surface p-6 rounded-2xl border border-border shadow-sm">
                <h2 className="text-2xl font-bold text-text mb-2">Управление заказами</h2>
                <p className="text-sm text-text-muted mb-6">Обработка входящих заказов и контроль доставки.</p>

                <div className="flex flex-col md:flex-row gap-4 mb-6">
                    <input
                        type="text"
                        placeholder="Поиск по номеру, адресу, почте или телефону..."
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        className="flex-1 px-4 py-3 bg-bg border-2 border-transparent focus:border-border-focus rounded-xl text-sm text-text outline-none transition-all"
                    />
                    <select
                        value={statusFilter}
                        onChange={(e) => setStatusFilter(e.target.value)}
                        className="px-4 py-3 bg-bg border-2 border-transparent focus:border-border-focus rounded-xl text-sm text-text outline-none cursor-pointer md:w-64 transition-all"
                    >
                        <option value="">Все статусы</option>
                        {Object.entries(orderStatusMap).map(([key, config]) => (
                            <option key={key} value={key}>{config.label}</option>
                        ))}
                    </select>
                </div>

                <div className="overflow-x-auto border border-border/50 rounded-xl">
                    <table className="w-full text-left border-collapse">
                        <thead className="bg-bg">
                            <tr className="border-b border-border/50 text-sm text-text-muted">
                                <th className="py-4 px-4 font-semibold whitespace-nowrap">Номер и Дата</th>
                                <th className="py-4 px-4 font-semibold">Клиент</th>
                                <th className="py-4 px-4 font-semibold">Адрес доставки</th>
                                <th className="py-4 px-4 font-semibold">Сумма</th>
                                <th className="py-4 px-4 font-semibold">Статус</th>
                                <th className="py-4 px-4 font-semibold text-right">Действие</th>
                            </tr>
                        </thead>
                        <tbody className="text-sm bg-surface">
                            {isLoading ? (
                                <tr>
                                    <td colSpan={6} className="text-center py-10 animate-pulse text-text-muted font-medium">Загрузка заказов...</td>
                                </tr>
                            ) : filteredOrders.length === 0 ? (
                                <tr>
                                    <td colSpan={6} className="text-center py-10 text-text-muted font-medium border-2 border-dashed border-border/50 m-4 rounded-xl">
                                        Заказы не найдены
                                    </td>
                                </tr>
                            ) : (
                                filteredOrders.map(order => {
                                    const statusConfig = orderStatusMap[order.status] || { label: order.status, color: 'bg-bg text-text-muted border-border' };

                                    return (
                                        <tr key={order.id} className="border-b border-border/50 hover:bg-bg/50 transition-colors">
                                            <td className="py-4 px-4">
                                                <div className="font-mono font-bold text-accent">
                                                    #{order.id.split('-')[0].toUpperCase()}
                                                </div>
                                                <div className="text-xs text-text-muted mt-1">
                                                    {new Date(order.createdAt).toLocaleDateString('ru-RU', {
                                                        day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit'
                                                    })}
                                                </div>
                                            </td>

                                            <td className="py-4 px-4">
                                                <div className="font-semibold text-text">{order.customerEmail}</div>
                                                {order.customerPhone && (
                                                    <div className="text-xs text-text-muted mt-0.5">{order.customerPhone}</div>
                                                )}
                                            </td>

                                            <td className="py-4 px-4 max-w-xs">
                                                <div className="font-medium text-text truncate" title={order.fullAddress}>
                                                    {order.fullAddress}
                                                </div>
                                                {order.flat && (
                                                    <div className="text-xs text-text-muted mt-0.5">Кв/Офис: {order.flat}</div>
                                                )}
                                            </td>

                                            <td className="py-4 px-4 font-extrabold text-text whitespace-nowrap">
                                                {order.totalPrice.toLocaleString('ru-RU')} ₽
                                            </td>

                                            <td className="py-4 px-4">
                                                <span className={`px-2 py-1 rounded-md text-xs font-bold border whitespace-nowrap ${statusConfig.color}`}>
                                                    {statusConfig.label}
                                                </span>
                                            </td>

                                            <td className="py-4 px-4 text-right">
                                                <Link
                                                    to={`/order/${order.id}`}
                                                    className="inline-block text-xs bg-surface text-text border border-border px-4 py-2 rounded-lg hover:border-accent hover:text-accent font-bold transition-all shadow-sm hover:shadow"
                                                >
                                                    Подробнее
                                                </Link>
                                            </td>
                                        </tr>
                                    );
                                })
                            )}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    );
};