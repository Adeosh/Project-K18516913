import type { FC, SubmitEvent } from 'react';
import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';

export const RegisterForm: FC = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [phoneNumber, setPhoneNumber] = useState('');
    const [validationError, setValidationError] = useState('');

    const { register, isLoading, error } = useAuthStore();
    const navigate = useNavigate();

    const handlePhoneChange = (value: string) => {
        const digits = value.replace(/\D/g, '');

        let cleaned = digits;
        if (cleaned.startsWith('7') || cleaned.startsWith('8')) {
            cleaned = cleaned.slice(1);
        }

        let formatted = '+7';
        if (cleaned.length > 0) {
            formatted += ` (${cleaned.slice(0, 3)}`;
        }
        if (cleaned.length > 3) {
            formatted += `) ${cleaned.slice(3, 6)}`;
        }
        if (cleaned.length > 6) {
            formatted += `-${cleaned.slice(6, 8)}`;
        }
        if (cleaned.length > 8) {
            formatted += `-${cleaned.slice(8, 10)}`;
        }

        setPhoneNumber(digits.length === 0 ? '' : formatted);
    };

    const handleSubmit = async (e: SubmitEvent) => {
        e.preventDefault();
        setValidationError('');

        if (password !== confirmPassword) {
            setValidationError('Пароли не совпадают');
            return;
        }

        try {
            await register(email, password, phoneNumber || undefined);
            navigate('/');
        }
        catch { // обрабатывается в Zustand
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-bg p-4 relative overflow-hidden">
            <div className="absolute top-0 right-0 w-96 h-96 bg-sand/40 rounded-full blur-3xl animate-pulse"></div>
            <div className="absolute bottom-0 left-0 w-96 h-96 bg-rust/20 rounded-full blur-3xl"></div>

            <div className="w-full max-w-md bg-surface rounded-3xl p-8 shadow-2xl relative z-10 border border-border">
                <div className="text-center mb-8">
                    <h2 className="text-3xl font-bold text-text mb-2">
                        РЕГИСТРАЦИЯ
                    </h2>
                    <p className="text-sm text-text-muted">Создание нового профиля</p>
                </div>

                {(error || validationError) && (
                    <div className="mb-6 bg-error/10 text-error p-4 rounded-xl text-sm font-medium border border-error/20">
                        {validationError || error}
                    </div>
                )}

                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                        <label className="block text-sm font-semibold text-text mb-1.5">
                            Электронная почта
                        </label>
                        <input
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                            placeholder="mail@mail.com"
                            className="w-full px-4 py-3 bg-bg border-2 border-transparent rounded-xl text-text placeholder-text-placeholder focus:border-border-focus focus:ring-4 focus:ring-accent/10 focus:outline-none transition-all"
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-semibold text-text mb-1.5">
                            Номер телефона <span className="text-text-muted font-normal text-xs">(необязательно)</span>
                        </label>
                        <input
                            type="tel"
                            value={phoneNumber}
                            onChange={(e) => handlePhoneChange(e.target.value)}
                            placeholder="+7 (999) 000-00-00"
                            className="w-full px-4 py-3 bg-bg border-2 border-transparent rounded-xl text-text placeholder-text-placeholder focus:border-border-focus focus:ring-4 focus:ring-accent/10 focus:outline-none transition-all"
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
                            required
                            placeholder="••••••••"
                            className="w-full px-4 py-3 bg-bg border-2 border-transparent rounded-xl text-text placeholder-text-placeholder focus:border-border-focus focus:ring-4 focus:ring-accent/10 focus:outline-none transition-all"
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-semibold text-text mb-1.5">
                            Подтверждение пароля
                        </label>
                        <input
                            type="password"
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                            required
                            placeholder="••••••••"
                            className="w-full px-4 py-3 bg-bg border-2 border-transparent rounded-xl text-text placeholder-text-placeholder focus:border-border-focus focus:ring-4 focus:ring-accent/10 focus:outline-none transition-all"
                        />
                    </div>

                    <button
                        type="submit"
                        disabled={isLoading}
                        className="w-full py-3.5 mt-4 bg-accent text-surface font-bold rounded-xl shadow-lg hover:shadow-xl hover:scale-[1.02] transition-all duration-200 disabled:opacity-70 disabled:hover:scale-100"
                    >
                        {isLoading ? 'Регистрация...' : 'Создать аккаунт'}
                    </button>
                </form>

                <div className="mt-6 text-center text-sm text-text-muted">
                    Уже есть доступ?{' '}
                    <Link to="/login" className="font-semibold text-accent hover:text-text transition-colors">
                        Войти в систему
                    </Link>
                </div>
            </div>
        </div>
    );
};