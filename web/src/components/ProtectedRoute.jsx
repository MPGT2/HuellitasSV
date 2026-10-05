import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "../context/useAuth";

// roles (opcional): ["usuario"], ["refugio"], ["admin"]...
export default function ProtectedRoute({ children, roles }) {
  const { autenticado, user } = useAuth();
  const location = useLocation();

  if (!autenticado) {
    return <Navigate to="/login" replace state={{ desde: location.pathname }} />;
  }
  if (roles && !roles.includes(user.rol)) return <Navigate to="/" replace />;
  return children;
}
