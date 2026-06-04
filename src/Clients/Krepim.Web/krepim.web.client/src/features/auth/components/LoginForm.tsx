import { useState } from 'react';
import type { FC, FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';

export const LoginForm: FC = () => {
    const { login, isLoading, error: authError } = useAuthStore();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [validationError, setValidationError] = useState<string | null>(null);

    const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setValidationError(null);

        if (!email.trim() || !password.trim()) {
            setValidationError('Все поля обязательны для заполнения');
            return;
        }

        try {
            await login(email, password);
            window.location.href = '/';
        }
        catch { // обрабатывается в Zustand 
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-bg p-4 relative overflow-hidden">
            <div className="absolute top-0 left-0 w-96 h-96 bg-sand/40 rounded-full blur-3xl animate-pulse"></div>
            <div className="absolute bottom-0 right-0 w-96 h-96 bg-rust/20 rounded-full blur-3xl"></div>

            <div className="w-full max-w-md bg-surface rounded-3xl p-8 shadow-2xl relative z-10 border border-border">
                <div className="mb-8 text-center">
                    <h2 className="text-3xl font-bold text-text mb-2">
                        КРЕПИМ<span className="text-accent">.PRO</span>
                    </h2>
                    <p className="text-sm text-text-muted">Добро пожаловать в систему</p>
                </div>

                <form onSubmit={handleSubmit} className="space-y-5">
                    <div>
                        <label className="block text-sm font-semibold text-text mb-1.5">
                            Электронная почта
                        </label>
                        <input
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            placeholder="mail@mail.com"
                            disabled={isLoading}
                            className="w-full px-4 py-3 bg-bg border-2 border-border rounded-xl text-text placeholder-text-placeholder focus:border-border-focus focus:ring-4 focus:ring-accent/10 focus:outline-none transition-all"
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-semibold text-text mb-1.5">
                            Пароль
                        </label>
                        <input
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            placeholder="••••••••"
                            disabled={isLoading}
                            className="w-full px-4 py-3 bg-bg border-2 border-border rounded-xl text-text placeholder-text-placeholder focus:border-border-focus focus:ring-4 focus:ring-accent/10 focus:outline-none transition-all"
                        />
                    </div>

                    {(validationError || authError) && (
                        <div className="bg-error/10 text-error p-4 rounded-xl text-sm font-medium border border-error/20">
                            {validationError || authError}
                        </div>
                    )}

                    <button
                        type="submit"
                        disabled={isLoading}
                        className="w-full py-3.5 mt-2 bg-accent text-surface font-bold rounded-xl shadow-lg hover:shadow-xl hover:scale-[1.02] transition-all duration-200 disabled:opacity-70 disabled:hover:scale-100"
                    >
                        {isLoading ? 'Выполняется вход...' : 'Войти в аккаунт'}
                    </button>

                    <div className="mt-6 text-center text-sm text-text-muted">
                        Нет аккаунта?{' '}
                        <Link to="/register" className="font-semibold text-accent hover:text-text transition-colors">
                            Создать аккаунт
                        </Link>
                    </div>
                </form>
            </div>
        </div>
    );
};