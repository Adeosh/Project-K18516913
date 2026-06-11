import type { FC } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { MainLayout } from './components/Layout/MainLayout';
import { ProtectedRoute } from './components/Router/ProtectedRoute';
import { LoginForm } from './features/auth/components/LoginForm';
import { RegisterForm } from './features/auth/components/RegisterForm';
import { ProfileView } from './features/profile/components/ProfileView';
import { CatalogView } from './features/catalog/components/CatalogView';
import { BasketView } from './features/basket/components/BasketView';
import { ProductDetailView } from './features/catalog/components/ProductDetailView';

export const App: FC = () => {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/login" element={<LoginForm />} />
                <Route path="/register" element={<RegisterForm />} />
                <Route path="/" element={<MainLayout />}>

                    <Route index element={<CatalogView />} />
                    <Route path="product/:id" element={<ProductDetailView />} />
                    <Route path="basket" element={<BasketView />} />
                    <Route
                        path="profile"
                        element={
                            <ProtectedRoute>
                                <ProfileView />
                            </ProtectedRoute>
                        }
                    />

                </Route>
            </Routes>
        </BrowserRouter>
    );
};

export default App;