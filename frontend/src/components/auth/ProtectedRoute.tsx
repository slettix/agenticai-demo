import React from 'react';
import { useAuth } from '../../contexts/AuthContext.tsx';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredPermission?: string;
  requiredRole?: string;
  fallback?: React.ReactNode;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ 
  children, 
  requiredPermission,
  requiredRole,
  fallback = <div>Du har ikke tilgang til denne siden.</div>
}) => {
  const { user, hasPermission, hasRole, isLoading } = useAuth();

  if (isLoading) {
    return <div>Laster...</div>;
  }

  if (!user) {
    return <div>Du må logge inn for å se denne siden.</div>;
  }

  // Check permission if specified
  if (requiredPermission && !hasPermission(requiredPermission)) {
    return <>{fallback}</>;
  }

  // Check role if specified
  if (requiredRole && !hasRole(requiredRole)) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
};