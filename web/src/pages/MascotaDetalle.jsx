import { Link, useParams } from "react-router-dom";
import { api } from "../services/api";
import { useDatos } from "../hooks/useDatos";
import { useAuth } from "../context/useAuth";
import { resolveImageUrl } from "../config/env";
import { edadTexto, fecha } from "../utils/formato";
import EstadoBadge from "../components/EstadoBadge";
import Estrellas from "../components/Estrellas";
import { Cargando, ErrorMensaje } from "../components/Estado";

export default function MascotaDetalle() {
  const { id } = useParams();
  const { autenticado, esUsuario } = useAuth();
  const { datos: m, cargando, error } = useDatos(() => api.mascota(id), id);

  if (cargando) return <Cargando />;
  if (error || !m) return <ErrorMensaje mensaje={error || "No se encontró la mascota."} />;

  const imagen = resolveImageUrl(m.imagenUrl);
  const disponible = String(m.estado).toLowerCase() === "disponible";

  return (
    <>
      <p><Link to="/catalogo">← Volver al catálogo</Link></p>
      <article className="detalle">
        <div className="detalle-foto">
          {imagen ? (
            <img src={imagen} alt={`Foto de ${m.nombre}`} />
          ) : (
            <div className="sin-foto grande" aria-hidden="true">🐾</div>
          )}
        </div>
        <div className="detalle-info">
          <h1>{m.nombre} <EstadoBadge estado={m.estado} /></h1>
          <dl>
            <dt>Especie</dt><dd>{m.especie}</dd>
            <dt>Tamaño</dt><dd>{m.tamano}</dd>
            <dt>Edad</dt><dd>{edadTexto(m.edadMeses)}</dd>
            <dt>Estado de salud</dt><dd>{String(m.estadoSalud ?? "").replace(/_/g, " ")}</dd>
            {m.fechaRegistro && (<><dt>Registrada</dt><dd>{fecha(m.fechaRegistro)}</dd></>)}
            {m.refugio?.nombreOrganizacion && (
              <>
                <dt>Refugio</dt>
                <dd>
                  {m.refugio.idRefugio ? (
                    <Link to={`/refugios/${m.refugio.idRefugio}`}>{m.refugio.nombreOrganizacion}</Link>
                  ) : (
                    m.refugio.nombreOrganizacion
                  )}
                  {m.refugio.municipio && `, ${m.refugio.municipio}`}
                  {m.refugio.departamento && ` (${m.refugio.departamento})`}
                  {m.refugio.totalCalificaciones > 0 && (
                    <> <Estrellas valor={m.refugio.promedioEstrellas} total={m.refugio.totalCalificaciones} /></>
                  )}
                </dd>
              </>
            )}
          </dl>

          {disponible && esUsuario && (
            <Link className="btn" to={`/mascotas/${id}/solicitar`}>Solicitar adopción</Link>
          )}
          {disponible && !autenticado && (
            <p className="info">
              <Link to="/login" state={{ desde: `/mascotas/${id}/solicitar` }}>Inicia sesión</Link>{" "}
              como usuario para solicitar la adopción.
            </p>
          )}
          {!disponible && <p className="muted">Esta mascota ya no está disponible para adopción.</p>}
        </div>
      </article>
    </>
  );
}
