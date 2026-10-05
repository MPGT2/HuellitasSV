import { Link } from "react-router-dom";
import { useAuth } from "../context/useAuth";

export default function Encabezado({ titulo }) {
  const { tipo, logout } = useAuth();

  return (
    <header className="encabezado">
      <h1>{titulo}</h1>
      <nav className="acciones">
        <Link to="/mascotas">Mascotas</Link>
        {tipo === "administrador" && <Link to="/admin">Panel de administración</Link>}
        {tipo === "refugio" && (
          <Link className="boton" to="/mascotas/nueva">
            Registrar mascota
          </Link>
        )}
        <button onClick={logout}>Cerrar sesión</button>
      </nav>
    </header>
  );
}
