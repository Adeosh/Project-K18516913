import type { FC } from 'react';
import { Link } from 'react-router-dom';
import { useAuthStore } from '../../features/auth/store/authStore';
import { useBasketStore } from '../../features/basket/store/basketStore';

export const Header: FC = () => {
    const { user, logout } = useAuthStore();
    const basket = useBasketStore((state) => state.basket);
    const totalItems = basket?.items.length || 0;

    return (
        <header className="bg-surface border-b border-border shadow-sm px-6 py-4 flex flex-col sm:flex-row justify-between items-center gap-4 sticky top-0 z-50">
            <div className="flex-1 flex justify-start items-center">
                <Link to="/" className="text-2xl font-extrabold text-text hover:text-accent transition-colors">
                    КРЕПИМ<span className="text-accent">.ПРО</span>
                </Link>
            </div>

            <nav className="flex justify-center items-center gap-8 font-semibold text-sm">
                <Link to="/" className="text-text hover:text-accent transition-colors">Главная</Link>
                <Link to="/catalog" className="text-text hover:text-accent transition-colors">Каталог</Link>
                {user && (
                    <Link to="/basket" className="text-text hover:text-accent transition-colors flex items-center gap-2">
                        Корзина
                        {totalItems > 0 && (
                            <span className="bg-accent text-surface px-2.5 py-0.5 rounded-full text-xs font-bold shadow-sm">
                                {totalItems}
                            </span>
                        )}
                    </Link>
                )}
            </nav>

            <div className="flex-1 flex justify-end items-center gap-4 sm:gap-6">
                <Link to={user ? "/profile" : "/login"} className="text-right hidden sm:block group">
                    <div className="text-text font-bold text-sm group-hover:text-accent transition-colors">
                        {user?.email?.split('@')[0] || 'Гость'}
                    </div>
                </Link>

                <div className="flex items-center gap-3">
                    {user?.role === 'Manager' && (
                        <Link to="/manager" className="bg-accent text-surface hover:bg-accent/90 px-4 py-2 rounded-xl text-sm font-semibold transition-all shadow-md hover:-translate-y-0.5">
                            Панель управления
                        </Link>
                    )}
                    {user ? (
                        <>
                            <Link to="/profile" className="bg-bg border border-border hover:border-border-focus hover:text-accent text-text px-4 py-2 rounded-xl text-sm font-semibold transition-all">Профиль</Link>
                            <button onClick={() => logout()} className="bg-error/5 text-error hover:bg-error/10 border border-transparent hover:border-error/20 px-4 py-2 rounded-xl text-sm font-semibold transition-all">Выйти</button>
                        </>
                    ) : (
                        <Link to="/login" className="bg-accent/10 text-accent hover:bg-accent/20 border border-transparent px-4 py-2 rounded-xl text-sm font-semibold transition-all">Войти</Link>
                    )}
                </div>
            </div>
        </header>
    );
};