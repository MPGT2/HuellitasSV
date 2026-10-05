import { useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../services/api";
import { useDatos } from "../hooks/useDatos";
import { comoLista, fecha } from "../utils/formato";
import EstadoBadge from "../components/EstadoBadge";
import { Cargando, ErrorMensaje, Vacio } from "../components/Estado";

const FILTROS = [
  { valor: "", etiqueta: "Todas" },
  { valor: "Pendiente", etiqueta: "Pendientes" },
  { valor: "Aprobada", etiqueta: "Aprobadas" },
  { valor: "Rechazada", etiqueta: "Rechazadas" },
];

export default function MisSolicitudes() {
  const [filtro, setFiltro] = useState("");
  const { datos, cargando, error } = useDatos(
    async () => comoLista(await api.misSolicitudes(filtro || undefined)),
    filtro
  );

  return (
    <>
      <h1>Mis solicitudes de adopción</h1>
      <div className="pestanas">
        {FILTROS.map((f) => (
          <button key={f.valor} className={filtro === f.valor ? "activa" : ""} onClick={() => setFiltro(f.valor)}>
            {f.etiqueta}
          </button>
        ))}
      </div>

      {cargando && <Cargando />}
      <ErrorMensaje mensaje={error} />
      {datos && datos.length === 0 && (
        <Vacio>Todavía no tienes solicitudes. <Link to="/catalogo">Explora el catálogo</Link>.</Vacio>
      )}
      <div className="lista">
        {datos?.map((s) => (
          <article className="item" key={s.idSolicitud ?? s.id}>
            <div className="item-cabecera">
              <h2>
                {s.mascota ? (
                  <Link to={`/mascotas/${s.mascota.idMascota ?? s.idMascota}`}>{s.mascota.nombre}</Link>
                ) : (
                  `Mascota #${s.idMascota}`
                )}
              </h2>
              <EstadoBadge estado={s.estado} />
            </div>
            <p className="muted">Enviada el {fecha(s.fechaSolicitud)}</p>
            <p className="muted">Contacto: {s.nombreContacto} · {s.telefonoContacto} · {s.correoContacto}</p>
            {s.comentarioDecision && <p className="nota">Respuesta del refugio: {s.comentarioDecision}</p>}
          </article>
        ))}
      </div>
    </>
  );
}
