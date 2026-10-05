import { useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../services/api";
import { useDatos } from "../hooks/useDatos";
import { useAuth } from "../context/useAuth";
import { mensajeDeError } from "../services/errores";
import { resolveImageUrl } from "../config/env";
import { comoLista, fecha } from "../utils/formato";
import EstadoBadge from "../components/EstadoBadge";
import TarjetaMascota from "../components/TarjetaMascota";
import { Cargando, ErrorMensaje, Vacio } from "../components/Estado";

function TabMascotas() {
  const { user } = useAuth();
  const [errorAccion, setErrorAccion] = useState("");
  const { datos, cargando, error, recargar } = useDatos(
    async () => comoLista(await api.mascotasDeRefugio(user.idRefugio)),
    String(user.idRefugio)
  );

  async function eliminar(m) {
    if (!window.confirm(`¿Eliminar a ${m.nombre}? Esta acción no se puede deshacer.`)) return;
    setErrorAccion("");
    try {
      await api.eliminarMascota(m.idMascota);
      recargar();
    } catch (err) {
      setErrorAccion(mensajeDeError(err, "No se pudo eliminar la mascota."));
    }
  }

  return (
    <>
      <p>
        <Link className="btn" to="/refugio/mascotas/nueva">Registrar mascota</Link>
      </p>
      {cargando && <Cargando />}
      <ErrorMensaje mensaje={error || errorAccion} />
      {datos && datos.length === 0 && <Vacio>Aún no has registrado mascotas.</Vacio>}
      <div className="rejilla">
        {datos?.map((m) => (
          <TarjetaMascota
            key={m.idMascota}
            mascota={m}
            acciones={
              <>
                <Link className="btn btn-sec" to={`/refugio/mascotas/${m.idMascota}/editar`}>Editar</Link>
                <button className="btn btn-peligro" onClick={() => eliminar(m)}>Eliminar</button>
              </>
            }
          />
        ))}
      </div>
    </>
  );
}

function TabSolicitudes() {
  const [estado, setEstado] = useState("");
  const [decision, setDecision] = useState(null); // { id, tipo, comentario }
  const [errorAccion, setErrorAccion] = useState("");
  const [enviando, setEnviando] = useState(false);
  const { datos, cargando, error, recargar } = useDatos(
    async () => comoLista(await api.solicitudesRefugio(estado || undefined)),
    estado
  );

  async function confirmar() {
    setErrorAccion("");
    setEnviando(true);
    try {
      await api.decidirSolicitud(decision.id, decision.tipo, decision.comentario.trim());
      setDecision(null);
      recargar();
    } catch (err) {
      setErrorAccion(mensajeDeError(err, "No se pudo registrar la decisión."));
    } finally {
      setEnviando(false);
    }
  }

  return (
    <>
      <div className="pestanas">
        {["", "Pendiente", "Aprobada", "Rechazada"].map((v) => (
          <button key={v} className={estado === v ? "activa" : ""} onClick={() => setEstado(v)}>
            {v || "Todas"}
          </button>
        ))}
      </div>

      {cargando && <Cargando />}
      <ErrorMensaje mensaje={error || errorAccion} />
      {datos && datos.length === 0 && <Vacio>No hay solicitudes.</Vacio>}

      <div className="lista">
        {datos?.map((s) => {
          const id = s.idSolicitud ?? s.id;
          const abierta = decision?.id === id;
          return (
            <article className="item" key={id}>
              <div className="item-cabecera">
                <h2>{s.mascota?.nombre ?? `Mascota #${s.idMascota}`}</h2>
                <EstadoBadge estado={s.estado} />
              </div>
              <p className="muted">Enviada el {fecha(s.fechaSolicitud)}</p>
              <p>{s.nombreContacto} · {s.telefonoContacto} · {s.correoContacto}</p>
              {s.comentarioDecision && <p className="nota">Comentario: {s.comentarioDecision}</p>}

              {String(s.estado).toLowerCase() === "pendiente" && !abierta && (
                <div className="tarjeta-acciones">
                  <button className="btn" onClick={() => setDecision({ id, tipo: "aprobar", comentario: "" })}>Aprobar</button>
                  <button className="btn btn-peligro" onClick={() => setDecision({ id, tipo: "rechazar", comentario: "" })}>Rechazar</button>
                </div>
              )}

              {abierta && (
                <div className="decision">
                  <label>
                    Comentario para el solicitante ({decision.tipo === "aprobar" ? "aprobación" : "rechazo"})
                    <textarea
                      rows="3"
                      value={decision.comentario}
                      onChange={(e) => setDecision({ ...decision, comentario: e.target.value })}
                    />
                  </label>
                  <div className="tarjeta-acciones">
                    <button className={decision.tipo === "aprobar" ? "btn" : "btn btn-peligro"} onClick={confirmar} disabled={enviando}>
                      {enviando ? "Guardando..." : `Confirmar ${decision.tipo === "aprobar" ? "aprobación" : "rechazo"}`}
                    </button>
                    <button className="btn btn-sec" onClick={() => setDecision(null)}>Cancelar</button>
                  </div>
                </div>
              )}
            </article>
          );
        })}
      </div>
    </>
  );
}

function TabRescates() {
  const [errorAccion, setErrorAccion] = useState("");
  const { datos, cargando, error, recargar } = useDatos(
    async () => comoLista(await api.reportesRescate("Pendiente")),
    "pendientes"
  );

  async function atender(id) {
    setErrorAccion("");
    try {
      await api.marcarAtendido(id);
      recargar();
    } catch (err) {
      setErrorAccion(mensajeDeError(err, "No se pudo marcar el reporte como atendido."));
    }
  }

  return (
    <>
      {cargando && <Cargando />}
      <ErrorMensaje mensaje={error || errorAccion} />
      {datos && datos.length === 0 && <Vacio>No hay reportes de rescate pendientes.</Vacio>}
      <div className="lista">
        {datos?.map((r) => {
          const id = r.idReporte ?? r.id;
          const foto = resolveImageUrl(r.fotoUrl);
          return (
            <article className="item item-foto" key={id}>
              {foto && <img src={foto} alt="Foto del animal reportado" loading="lazy" />}
              <div>
                <div className="item-cabecera">
                  <h2>Reporte #{id}</h2>
                  <EstadoBadge estado={r.estado} />
                </div>
                <p>{r.descripcion}</p>
                <p className="muted">Registrado el {fecha(r.fechaRegistro)}</p>
                {r.latitud != null && r.longitud != null && (
                  <p>
                    <a href={`https://www.google.com/maps?q=${r.latitud},${r.longitud}`} target="_blank" rel="noreferrer">
                      Ver ubicación en el mapa
                    </a>
                  </p>
                )}
                <button className="btn" onClick={() => atender(id)}>Marcar como atendido</button>
              </div>
            </article>
          );
        })}
      </div>
    </>
  );
}

const TABS = [
  { id: "mascotas", etiqueta: "Mis mascotas" },
  { id: "solicitudes", etiqueta: "Solicitudes de adopción" },
  { id: "rescates", etiqueta: "Rescates" },
];

export default function PanelRefugio() {
  const [tab, setTab] = useState("mascotas");

  return (
    <>
      <h1>Panel del refugio</h1>
      <div className="pestanas" role="tablist">
        {TABS.map((t) => (
          <button key={t.id} role="tab" aria-selected={tab === t.id} className={tab === t.id ? "activa" : ""} onClick={() => setTab(t.id)}>
            {t.etiqueta}
          </button>
        ))}
      </div>
      {tab === "mascotas" && <TabMascotas />}
      {tab === "solicitudes" && <TabSolicitudes />}
      {tab === "rescates" && <TabRescates />}
    </>
  );
}
