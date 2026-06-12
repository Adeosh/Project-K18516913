import type { FC } from 'react';
import { NavLink, Outlet } from 'react-router-dom';

export const ManagerLayout: FC = () => {
    return (
        <div className="min-h-screen bg-bg flex flex-col md:flex-row">

            <aside className="w-full md:w-64 bg-surface border-r border-border flex flex-col shadow-sm flex-shrink-0">
                <div className="p-6 border-b border-border">
                    <h1 className="text-xl font-black text-accent tracking-wide">KREPIM<span className="text-text">.ERP</span></h1>
                    <p className="text-xs text-text-muted mt-1 font-medium uppercase">Панель управления</p>
                </div>

                <nav className="flex-1 p-4 space-y-2 flex flex-row md:flex-col overflow-x-auto md:overflow-visible">
                    <NavLink
                        to="/manager/catalog"
                        className={({ isActive }) => `flex items-center gap-3 px-4 py-3 rounded-xl font-bold transition-all whitespace-nowrap ${isActive ? 'bg-accent/10 text-accent border border-accent/20' : 'text-text-muted hover:bg-bg hover:text-text'}`}
                    >
                        📦 Каталог товаров
                    </NavLink>

                    <NavLink
                        to="/manager/inventory"
                        className={({ isActive }) => `flex items-center gap-3 px-4 py-3 rounded-xl font-bold transition-all whitespace-nowrap ${isActive ? 'bg-accent/10 text-accent border border-accent/20' : 'text-text-muted hover:bg-bg hover:text-text'}`}
                    >
                        🏢 Склад и приемка
                    </NavLink>

                    <NavLink
                        to="/manager/orders"
                        className={({ isActive }) => `flex items-center gap-3 px-4 py-3 rounded-xl font-bold transition-all whitespace-nowrap ${isActive ? 'bg-accent/10 text-accent border border-accent/20' : 'text-text-muted hover:bg-bg hover:text-text'}`}
                    >
                        🛒 Заказы
                    </NavLink>
                </nav>
            </aside>

            <main className="flex-1 overflow-y-auto">
                <Outlet />
            </main>
        </div>
    );
};