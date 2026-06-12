import { useEffect, useState } from 'react';
import type { FC, FormEvent } from 'react';
import { useProfileStore } from '../store/profileStore';
import { AddressMapPicker } from '../../../components/ui/AddressMapPicker';
import { profileApi } from '../api/profileApi';

type Tab = 'profile' | 'orders';

export const ProfileView: FC = () => {
    const { profile, fetchProfile, updateProfile, isLoading } = useProfileStore();
    const [activeTab, setActiveTab] = useState<Tab>('profile');

    const [email, setEmail] = useState('');
    const [phone, setPhone] = useState('');
    const [fullAddress, setFullAddress] = useState('');
    const [lat, setLat] = useState<number | null>(null);
    const [lng, setLng] = useState<number | null>(null);
    const [flat, setFlat] = useState('');

    const [isProfileSuccess, setIsProfileSuccess] = useState(false);
    const [profileError, setProfileError] = useState<string | null>(null);

    const [isPasswordModalOpen, setIsPasswordModalOpen] = useState(false);
    const [oldPassword, setOldPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');

    const [passwordError, setPasswordError] = useState<string | null>(null);
    const [isPasswordSuccess, setIsPasswordSuccess] = useState(false);
    const [isPasswordLoading, setIsPasswordLoading] = useState(false);

    useEffect(() => {
        void fetchProfile();
    }, []);

    useEffect(() => {
        if (profile) {
            setEmail(profile.email || '');
            if (profile.phoneNumber) {
                handlePhoneChange(profile.phoneNumber);
            } else {
                setPhone('');
            }
            setFullAddress(profile.defaultAddress?.fullAddress || '');
            setLat(profile.defaultAddress?.latitude || null);
            setLng(profile.defaultAddress?.longitude || null);
            setFlat(profile.defaultAddress?.flat || '');
        }
    }, [profile]);

    const handlePhoneChange = (value: string) => {
        const digits = value.replace(/\D/g, '');
        let cleaned = digits;
        if (cleaned.startsWith('7') || cleaned.startsWith('8')) {
            cleaned = cleaned.slice(1);
        }

        let formatted = '+7';
        if (cleaned.length > 0) formatted += ` (${cleaned.slice(0, 3)}`;
        if (cleaned.length > 3) formatted += `) ${cleaned.slice(3, 6)}`;
        if (cleaned.length > 6) formatted += `-${cleaned.slice(6, 8)}`;
        if (cleaned.length > 8) formatted += `-${cleaned.slice(8, 10)}`;

        setPhone(digits.length === 0 ? '' : formatted);
    };

    const handleSaveProfile = async (e: FormEvent) => {
        e.preventDefault();
        setProfileError(null);
        setIsProfileSuccess(false);

        try {
            await updateProfile({
                email: email,
                phoneNumber: phone || null,
                defaultAddress: fullAddress && lat && lng ? {
                    fullAddress: fullAddress,
                    latitude: lat,
                    longitude: lng,
                    flat: flat || null
                } : null
            });
            setIsProfileSuccess(true);
            setTimeout(() => setIsProfileSuccess(false), 3000);
        } catch (err: any) {
            setProfileError(err.message || 'Ошибка сохранения профиля');
        }
    };

    const isNewPasswordValidLength = newPassword.length >= 6;
    const isPasswordsMatch = newPassword === confirmPassword;
    const canSubmitPassword = oldPassword && isNewPasswordValidLength && isPasswordsMatch;

    const handleChangePassword = async (e: FormEvent) => {
        e.preventDefault();
        if (!canSubmitPassword) return;

        setPasswordError(null);
        setIsPasswordSuccess(false);
        setIsPasswordLoading(true);

        try {
            await profileApi.changePassword(oldPassword, newPassword);
            setIsPasswordSuccess(true);
            setOldPassword('');
            setNewPassword('');
            setConfirmPassword('');

            setTimeout(() => {
                setIsPasswordSuccess(false);
                setIsPasswordModalOpen(false);
            }, 2000);
        } catch (err: any) {
            setPasswordError(err.message);
        } finally {
            setIsPasswordLoading(false);
        }
    };

    if (isLoading && !profile) {
        return <div className="p-10 text-center text-text-muted animate-pulse font-medium">Загрузка профиля...</div>;
    }

    return (
        <div className="max-w-7xl mx-auto p-4 sm:p-6 mt-4 sm:mt-8 relative">
            <h2 className="text-2xl sm:text-3xl font-extrabold text-text mb-6 sm:mb-8">Личный кабинет</h2>

            <div className="flex flex-col md:flex-row gap-6 sm:gap-8">
                <aside className="w-full md:w-64 flex-shrink-0">
                    <nav className="flex flex-col gap-2">
                        <button
                            onClick={() => setActiveTab('profile')}
                            className={`w-full text-left px-5 py-3 rounded-xl font-bold transition-all ${activeTab === 'profile' ? 'bg-accent text-surface shadow-md' : 'bg-surface text-text-muted hover:bg-bg hover:text-text'}`}
                        >
                            Настройки профиля
                        </button>
                        <button
                            onClick={() => setActiveTab('orders')}
                            className={`w-full text-left px-5 py-3 rounded-xl font-bold transition-all ${activeTab === 'orders' ? 'bg-accent text-surface shadow-md' : 'bg-surface text-text-muted hover:bg-bg hover:text-text'}`}
                        >
                            История заказов
                        </button>
                    </nav>
                </aside>

                <main className="flex-1 bg-surface rounded-3xl p-6 sm:p-8 md:p-10 shadow-sm border border-border">
                    {activeTab === 'profile' && (
                        <div className="grid grid-cols-1 xl:grid-cols-2 gap-12">
                            <div className="space-y-10">
                                <section>
                                    <h3 className="text-xl font-bold text-text mb-6 pb-2 border-b border-border/50">Учетная запись</h3>
                                    <div className="space-y-4">
                                        <div>
                                            <label className="block text-sm font-semibold text-text mb-1.5">Электронная почта</label>
                                            <input
                                                type="email"
                                                value={email}
                                                onChange={(e) => setEmail(e.target.value)}
                                                placeholder="mail@example.com"
                                                className="w-full px-4 py-3 bg-bg border-2 border-transparent rounded-xl text-text placeholder-text-placeholder focus:border-border-focus focus:ring-4 focus:ring-accent/10 focus:outline-none transition-all"
                                            />
                                        </div>

                                        <div>
                                            <label className="block text-sm font-semibold text-text mb-1.5">Номер телефона</label>
                                            <input
                                                type="tel"
                                                value={phone}
                                                onChange={(e) => handlePhoneChange(e.target.value)}
                                                placeholder="+7 (999) 000-00-00"
                                                className="w-full px-4 py-3 bg-bg border-2 border-transparent rounded-xl text-text placeholder-text-placeholder focus:border-border-focus focus:ring-4 focus:ring-accent/10 focus:outline-none transition-all"
                                            />
                                        </div>

                                        <div className="pt-4">
                                            <button
                                                onClick={() => setIsPasswordModalOpen(true)}
                                                className="text-sm font-bold text-text border border-border px-4 py-2.5 rounded-xl hover:border-accent hover:text-accent transition-all"
                                            >
                                                Изменить пароль
                                            </button>
                                        </div>
                                    </div>
                                </section>
                            </div>

                            <div className="space-y-6">
                                <h3 className="text-xl font-bold text-text mb-6 pb-2 border-b border-border/50">Адрес доставки</h3>
                                <p className="text-sm text-text-muted">Отметьте ваш дом на карте. Этот адрес будет использоваться при оформлении заказов.</p>

                                <AddressMapPicker
                                    initialLocation={profile?.defaultAddress?.fullAddress ? {
                                        fullAddress: profile.defaultAddress.fullAddress,
                                        latitude: profile.defaultAddress.latitude!,
                                        longitude: profile.defaultAddress.longitude!
                                    } : null}
                                    onChange={(loc) => {
                                        setFullAddress(loc.fullAddress);
                                        setLat(loc.latitude);
                                        setLng(loc.longitude);
                                    }}
                                />

                                <div>
                                    <label className="block text-sm font-semibold text-text mb-1.5">Квартира / Офис <span className="text-text-muted font-normal text-xs">(необязательно)</span></label>
                                    <input
                                        type="text"
                                        value={flat}
                                        onChange={(e) => setFlat(e.target.value)}
                                        placeholder="Например: 42"
                                        className="w-full sm:w-1/2 px-4 py-3 bg-bg border-2 border-transparent rounded-xl text-text placeholder-text-placeholder focus:border-border-focus focus:ring-4 focus:ring-accent/10 focus:outline-none transition-all"
                                    />
                                </div>

                                <div className="pt-6">
                                    {profileError && <p className="text-error font-medium mb-3 text-sm">{profileError}</p>}
                                    <button
                                        onClick={handleSaveProfile}
                                        disabled={isLoading}
                                        className="w-full py-4 bg-accent text-surface font-bold text-lg rounded-xl shadow-md hover:shadow-lg hover:scale-[1.02] transition-all disabled:opacity-70 disabled:hover:scale-100"
                                    >
                                        {isLoading ? 'Сохранение...' : 'Сохранить данные'}
                                    </button>
                                    {isProfileSuccess && (
                                        <p className="text-green-600 font-bold text-center mt-3">Изменения сохранены!</p>
                                    )}
                                </div>
                            </div>
                        </div>
                    )}

                    {activeTab === 'orders' && (
                        <div>
                            <h3 className="text-2xl font-bold text-text mb-6">История заказов</h3>
                            <div className="text-center py-20 border-2 border-dashed border-border rounded-2xl text-text-muted font-medium">У вас пока нет заказов</div>
                        </div>
                    )}

                </main>
            </div>

            {isPasswordModalOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-text/20 backdrop-blur-sm transition-opacity">
                    <div className="bg-surface w-full max-w-md rounded-3xl shadow-2xl border border-border p-6 sm:p-8 relative">
                        <button
                            onClick={() => setIsPasswordModalOpen(false)}
                            className="absolute top-5 right-5 text-text-muted hover:text-error transition-colors"
                        >
                            ✕ Закрыть
                        </button>

                        <h3 className="text-2xl font-bold text-text mb-6">Смена пароля</h3>

                        {passwordError && (
                            <div className="bg-error/10 text-error p-3 rounded-xl text-sm font-medium border border-error/20 mb-4">
                                {passwordError}
                            </div>
                        )}
                        {isPasswordSuccess && (
                            <div className="bg-green-50 text-green-600 p-3 rounded-xl text-sm font-medium border border-green-200 mb-4">
                                Пароль успешно изменен!
                            </div>
                        )}

                        <form onSubmit={handleChangePassword} className="space-y-4">
                            <div>
                                <label className="block text-sm font-semibold text-text mb-1.5">Текущий пароль</label>
                                <input
                                    type="password"
                                    value={oldPassword}
                                    onChange={(e) => setOldPassword(e.target.value)}
                                    placeholder="••••••••"
                                    className="w-full px-4 py-3 bg-bg border-2 border-transparent rounded-xl text-text focus:border-border-focus focus:outline-none"
                                />
                            </div>

                            <div>
                                <label className="block text-sm font-semibold text-text mb-1.5">Новый пароль</label>
                                <input
                                    type="password"
                                    value={newPassword}
                                    onChange={(e) => setNewPassword(e.target.value)}
                                    placeholder="••••••••"
                                    className={`w-full px-4 py-3 bg-bg border-2 rounded-xl text-text focus:outline-none ${newPassword && !isNewPasswordValidLength ? 'border-error/50' : 'border-transparent focus:border-border-focus'}`}
                                />
                                {newPassword && !isNewPasswordValidLength && (
                                    <p className="text-xs text-error mt-1 font-medium">Минимум 6 символов</p>
                                )}
                            </div>

                            <div>
                                <label className="block text-sm font-semibold text-text mb-1.5">Повторите новый пароль</label>
                                <input
                                    type="password"
                                    value={confirmPassword}
                                    onChange={(e) => setConfirmPassword(e.target.value)}
                                    placeholder="••••••••"
                                    className={`w-full px-4 py-3 bg-bg border-2 rounded-xl text-text focus:outline-none ${confirmPassword && !isPasswordsMatch ? 'border-error/50' : 'border-transparent focus:border-border-focus'}`}
                                />
                                {confirmPassword && !isPasswordsMatch && (
                                    <p className="text-xs text-error mt-1 font-medium">Пароли не совпадают</p>
                                )}
                            </div>

                            <button
                                type="submit"
                                disabled={!canSubmitPassword || isPasswordLoading}
                                className="w-full mt-4 py-3.5 bg-text text-surface font-bold rounded-xl hover:bg-accent transition-all disabled:opacity-50 disabled:hover:bg-text"
                            >
                                {isPasswordLoading ? 'Проверка...' : 'Обновить пароль'}
                            </button>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
};