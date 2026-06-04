import type { FC, ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuthStore } from '../../features/auth/store/authStore';

interface ProtectedRouteProps {
    children: ReactNode;
}

export const ProtectedRoute: FC<ProtectedRouteProps> = ({ children }) => {
    const token = useAuthStore((state) => state.token);

    if (!token) {
        return <Navigate to="/login" replace />;
    }

    return <>{children}</>;
};