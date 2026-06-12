import type { FC } from 'react';
import { Link, Outlet } from 'react-router-dom';
import { useAuthStore } from '../../features/auth/store/authStore';
import { useBasketStore } from '../../features/basket/store/basketStore';

export const MainLayout: FC = () => {
    const { user, logout } = useAuthStore();
    const basket = useBasketStore((state) => state.basket);

    const totalItems = basket?.items.length || 0;

    return (
        <div className="min-h-screen bg-bg text-text flex flex-col">
            <header className="bg-surface border-b border-border shadow-sm px-6 py-4 flex flex-col sm:flex-row justify-between items-center gap-4 sticky top-0 z-50">
                <div className="flex items-center gap-8">
                    <Link to="/" className="text-2xl font-extrabold text-text hover:text-accent transition-colors">
                        КРЕПИМ<span className="text-accent">.PRO</span>
                    </Link>
                </div>

                <nav className="flex items-center gap-8 font-semibold text-sm">
                    <Link to="/" className="text-text hover:text-accent transition-colors">Каталог</Link>
                    <Link to="/basket" className="text-text hover:text-accent transition-colors flex items-center gap-2">
                        Корзина
                        {totalItems > 0 && (
                            <span className="bg-accent text-surface px-2.5 py-0.5 rounded-full text-xs font-bold shadow-sm">
                                {totalItems}
                            </span>
                        )}
                    </Link>
                </nav>

                <div className="flex items-center gap-4 sm:gap-6">
                    <Link to="/profile" className="text-right hidden sm:block group">
                        <div className="text-text font-bold text-sm group-hover:text-accent transition-colors">
                            {user?.email || 'Гость'}
                        </div>
                        <div className="text-text-muted text-xs font-medium">
                            Роль: {user?.role || 'Не авторизован'}
                        </div>
                    </Link>

                    <div className="flex items-center gap-3">
                        {user?.role === 'Manager' && (
                            <Link
                                to="/manager"
                                className="bg-accent text-surface hover:bg-accent/90 px-4 py-2 rounded-xl text-sm font-semibold transition-all shadow-md hover:-translate-y-0.5"
                            >
                                Панель управления
                            </Link>
                        )}
                        <Link
                            to="/profile"
                            className="bg-bg border border-border hover:border-border-focus hover:text-accent text-text px-4 py-2 rounded-xl text-sm font-semibold transition-all"
                        >
                            Профиль
                        </Link>
                        <button
                            onClick={() => logout()}
                            className="bg-error/5 text-error hover:bg-error/10 border border-transparent hover:border-error/20 px-4 py-2 rounded-xl text-sm font-semibold transition-all"
                        >
                            Выйти
                        </button>
                    </div>
                </div>
            </header>

            <main className="flex-1">
                <Outlet />
            </main>

            <footer className="bg-surface border-t border-border px-6 py-6 mt-12 text-center text-sm text-text-muted font-medium">
                © {new Date().getFullYear()} Крепим.PRO. Все права защищены.
            </footer>
        </div>
    );
};