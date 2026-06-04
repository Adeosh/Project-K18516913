import type { FC } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { MainLayout } from './components/Layout/MainLayout';
import { ProtectedRoute } from './components/Router/ProtectedRoute';
import { LoginForm } from './features/auth/components/LoginForm';
import { RegisterForm } from './features/auth/components/RegisterForm';
import { ProfileView } from './features/profile/components/ProfileView';
import { CatalogView } from './features/catalog/components/CatalogView';
import { BasketView } from './features/basket/components/BasketView';

export const App: FC = () => {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/login" element={<LoginForm />} />
                <Route path="/register" element={<RegisterForm />} />

                <Route path="/" element={
                    <ProtectedRoute>
                        <MainLayout />
                    </ProtectedRoute>
                }>
                    <Route index element={<CatalogView />} />
                    <Route path="basket" element={<BasketView />} />
                    <Route path="profile" element={<ProfileView />} />
                </Route>
            </Routes>
        </BrowserRouter>
    );
};

export default App;