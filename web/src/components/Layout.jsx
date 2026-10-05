import { Link, NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../context/useAuth";

export default function Layout() {
  const { autenticado, user, esUsuario, esRefugio, esAdmin, logout } = useAuth();

  return (
    <>
      <a className="saltar" href="#contenido">Saltar al contenido</a>
      <header className="barra">
        <div className="barra-interna">
          <Link to="/" className="marca">🐾 HuellitasSV</Link>
          <nav className="menu" aria-label="Principal">
            {esAdmin && <NavLink to="/admin">Administración</NavLink>}
            <NavLink to="/catalogo">Catálogo</NavLink>
            <NavLink to="/necesidades">Donaciones</NavLink>
            {esUsuario && <NavLink to="/mis-solicitudes">Mis solicitudes</NavLink>}
            {esUsuario && <NavLink to="/mis-reportes">Reportes</NavLink>}
            {esRefugio && <NavLink to="/refugio" end>Mi panel</NavLink>}
            {esRefugio && <NavLink to="/refugio/mascotas/nueva">Registrar mascota</NavLink>}
            {(esUsuario || esRefugio) && <NavLink to="/notificaciones">Notificaciones</NavLink>}
          </nav>
          <div className="sesion">
            {autenticado ? (
              <>
                <span className="nombre">{user.nombre}</span>
                <button className="btn btn-sec" onClick={logout}>Cerrar sesión</button>
              </>
            ) : (
              <>
                <Link className="btn btn-sec" to="/login">Iniciar sesión</Link>
                <Link className="btn" to="/registro">Crear cuenta</Link>
              </>
            )}
          </div>
        </div>
      </header>
      <main id="contenido" className="contenedor">
        <Outlet />
      </main>
    </>
  );
}
