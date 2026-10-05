import { Navigate, Route, Routes } from "react-router-dom";
import { useAuth } from "./context/useAuth";
import Layout from "./components/Layout";
import ProtectedRoute from "./components/ProtectedRoute";
import Login from "./pages/Login";
import Registro from "./pages/Registro";
import Catalogo from "./pages/Catalogo";
import MascotaDetalle from "./pages/MascotaDetalle";
import RefugioPerfil from "./pages/RefugioPerfil";
import Necesidades from "./pages/Necesidades";
import SolicitarAdopcion from "./pages/SolicitarAdopcion";
import MisSolicitudes from "./pages/MisSolicitudes";
import Reportar from "./pages/Reportar";
import MisReportes from "./pages/MisReportes";
import Notificaciones from "./pages/Notificaciones";
import PanelRefugio from "./pages/PanelRefugio";
import MascotaForm from "./pages/MascotaForm";
import PanelAdmin from "./pages/PanelAdmin";

// Cada rol aterriza en su pantalla principal.
function Inicio() {
  const { user, autenticado } = useAuth();
  if (autenticado && user.rol === "admin") return <Navigate to="/admin" replace />;
  if (autenticado && user.rol === "refugio") return <Navigate to="/refugio" replace />;
  return <Navigate to="/catalogo" replace />;
}

const solo = (roles, pagina) => <ProtectedRoute roles={roles}>{pagina}</ProtectedRoute>;

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Inicio />} />
        <Route path="/login" element={<Login />} />
        <Route path="/registro" element={<Registro />} />

        {/* Públicas */}
        <Route path="/catalogo" element={<Catalogo />} />
        <Route path="/mascotas/:id" element={<MascotaDetalle />} />
        <Route path="/refugios/:id" element={<RefugioPerfil />} />
        <Route path="/necesidades" element={<Necesidades />} />

        {/* Usuario */}
        <Route path="/mascotas/:id/solicitar" element={solo(["usuario"], <SolicitarAdopcion />)} />
        <Route path="/mis-solicitudes" element={solo(["usuario"], <MisSolicitudes />)} />
        <Route path="/reportar" element={solo(["usuario"], <Reportar />)} />
        <Route path="/mis-reportes" element={solo(["usuario"], <MisReportes />)} />

        {/* Usuario y refugio */}
        <Route path="/notificaciones" element={solo(["usuario", "refugio"], <Notificaciones />)} />

        {/* Refugio */}
        <Route path="/refugio" element={solo(["refugio"], <PanelRefugio />)} />
        <Route path="/refugio/mascotas/nueva" element={solo(["refugio"], <MascotaForm />)} />
        <Route path="/refugio/mascotas/:id/editar" element={solo(["refugio"], <MascotaForm />)} />

        {/* Administrador */}
        <Route path="/admin" element={solo(["admin"], <PanelAdmin />)} />

        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}
