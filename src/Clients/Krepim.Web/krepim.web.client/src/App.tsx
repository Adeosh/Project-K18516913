import type { FC } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { MainLayout } from './components/Layout/MainLayout';
import { ProtectedRoute } from './components/Router/ProtectedRoute';
import { LoginForm } from './features/auth/components/LoginForm';
import { RegisterForm } from './features/auth/components/RegisterForm';
import { ProfileView } from './features/profile/components/ProfileView';
import { CatalogView } from './features/catalog/components/CatalogView';
import { BasketView } from './features/basket/components/BasketView';
import { ProductDetailView } from './features/catalog/components/ProductDetailView';
import { ManagerLayout } from './layouts/ManagerLayout';
import { ManagerDashboard } from './features/catalog/components/ManagerDashboard';
import { InventoryDashboard } from './features/catalog/components/InventoryDashboard';
import { OrderDetailView } from './features/ordering/components/OrderDetailView';
import { MockPayView } from './features/payment/components/MockPayView';
import { HomeView } from './features/home/components/HomeView';
import { OrdersDashboard } from './features/ordering/components/OrdersDashboard';

export const App: FC = () => {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/login" element={<LoginForm />} />
                <Route path="/register" element={<RegisterForm />} />
                <Route path="/mock-pay" element={<MockPayView />} />

                <Route path="/" element={<MainLayout />}>

                    <Route index element={<HomeView />} />
                    <Route path="catalog" element={<CatalogView />} />

                    <Route path="product/:id" element={<ProductDetailView />} />
                    <Route path="basket" element={<BasketView />} />
                    <Route path="order/:id" element={<OrderDetailView />} />
                    <Route
                        path="profile"
                        element={
                            <ProtectedRoute>
                                <ProfileView />
                            </ProtectedRoute>
                        }
                    />
                    <Route path="/manager" element={<ManagerLayout />}>
                        <Route index element={<Navigate to="catalog" replace />} />
                        <Route path="catalog" element={<ManagerDashboard />} />
                        <Route path="inventory" element={<InventoryDashboard />} />
                        <Route path="orders" element={<OrdersDashboard />} />
                    </Route>
                </Route>
            </Routes>
        </BrowserRouter>
    );
};

export default App;