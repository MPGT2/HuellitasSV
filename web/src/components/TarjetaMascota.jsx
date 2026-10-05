import { Link } from "react-router-dom";
import { resolveImageUrl } from "../config/env";
import { edadTexto } from "../utils/formato";
import EstadoBadge from "./EstadoBadge";
import Estrellas from "./Estrellas";

export default function TarjetaMascota({ mascota: m, acciones }) {
  const id = m.idMascota ?? m.id;
  const imagen = resolveImageUrl(m.imagenUrl);

  return (
    <article className="tarjeta-mascota">
      <Link to={`/mascotas/${id}`} className="tarjeta-enlace">
        <div className="foto">
          {imagen ? (
            <img src={imagen} alt={`Foto de ${m.nombre}`} loading="lazy" />
          ) : (
            <div className="sin-foto" aria-hidden="true">🐾</div>
          )}
          <EstadoBadge estado={m.estado} />
        </div>
        <div className="datos">
          <h2>{m.nombre}</h2>
          <p className="muted">
            {m.especie} · {m.tamano} · {edadTexto(m.edadMeses)}
          </p>
          <p className="muted">Salud: {String(m.estadoSalud ?? "").replace(/_/g, " ")}</p>
          {m.refugio?.nombreOrganizacion && (
            <p className="refugio">{m.refugio.nombreOrganizacion}</p>
          )}
          {m.refugio?.totalCalificaciones > 0 && (
            <Estrellas valor={m.refugio.promedioEstrellas} total={m.refugio.totalCalificaciones} />
          )}
        </div>
      </Link>
      {acciones && <div className="tarjeta-acciones">{acciones}</div>}
    </article>
  );
}
