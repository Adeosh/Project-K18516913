import type { FC } from 'react';
import { NavLink, Outlet } from 'react-router-dom';

import iconCatalog from '../assets/images/icons/other/catalog.png';
import iconInventory from '../assets/images/icons/other/warehouse.png';
import iconOrders from '../assets/images/icons/other/order.png';

export const ManagerLayout: FC = () => {
    return (
        <div className="min-h-screen bg-bg flex flex-col md:flex-row">

            <aside className="w-full md:w-64 bg-[#ECD7A1] border-r border-[#392012]/10 flex flex-col shadow-sm flex-shrink-0">
                <div className="p-6 border-b border-[#392012]/10">
                    <h1 className="text-xl font-black text-[#9E2F1F] tracking-wide">КРЕПИМ<span className="text-[#392012]">.ERP</span></h1>
                    <p className="text-xs text-[#602917] mt-1 font-medium uppercase">Панель управления</p>
                </div>

                <nav className="flex-1 p-4 space-y-2 flex flex-row md:flex-col overflow-x-auto md:overflow-visible">
                    <NavLink
                        to="/manager/catalog"
                        className={({ isActive }) => `flex items-center gap-3 px-4 py-3 rounded-xl font-bold transition-all whitespace-nowrap ${isActive ? 'bg-[#392012] text-white shadow-md' : 'text-[#392012] hover:bg-white/40'}`}
                    >
                        <img src={iconCatalog} alt="Каталог" className="w-6 h-6 object-contain" />
                        Каталог товаров
                    </NavLink>

                    <NavLink
                        to="/manager/inventory"
                        className={({ isActive }) => `flex items-center gap-3 px-4 py-3 rounded-xl font-bold transition-all whitespace-nowrap ${isActive ? 'bg-[#392012] text-white shadow-md' : 'text-[#392012] hover:bg-white/40'}`}
                    >
                        <img src={iconInventory} alt="Склад" className="w-6 h-6 object-contain" />
                        Склад и приемка
                    </NavLink>

                    <NavLink
                        to="/manager/orders"
                        className={({ isActive }) => `flex items-center gap-3 px-4 py-3 rounded-xl font-bold transition-all whitespace-nowrap ${isActive ? 'bg-[#392012] text-white shadow-md' : 'text-[#392012] hover:bg-white/40'}`}
                    >
                        <img src={iconOrders} alt="Заказы" className="w-6 h-6 object-contain" />
                        Заказы
                    </NavLink>
                </nav>
            </aside>

            <main className="flex-1 overflow-y-auto">
                <Outlet />
            </main>
        </div>
    );
};