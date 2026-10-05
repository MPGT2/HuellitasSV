import { useState } from "react";
import { api } from "../services/api";
import { useDatos } from "../hooks/useDatos";
import { mensajeDeError } from "../services/errores";
import { resolveImageUrl } from "../config/env";
import { comoLista, dinero, fecha } from "../utils/formato";
import EstadoBadge from "../components/EstadoBadge";
import { Cargando, ErrorMensaje, Vacio } from "../components/Estado";

const FILTROS_REFUGIO = [
  { valor: "pendiente", etiqueta: "Pendientes" },
  { valor: "aprobado", etiqueta: "Aprobados" },
  { valor: "rechazado", etiqueta: "Rechazados" },
  { valor: "todos", etiqueta: "Todos" },
];

function TabRefugios() {
  const [filtro, setFiltro] = useState("pendiente");
  const [errorAccion, setErrorAccion] = useState("");
  const { datos, cargando, error, recargar } = useDatos(
    async () => comoLista(await api.todosLosRefugios()),
    "todos"
  );

  async function decidir(r, estado) {
    setErrorAccion("");
    try {
      await api.cambiarEstadoRefugio(r.idRefugio, estado);
      recargar();
    } catch (err) {
      setErrorAccion(mensajeDeError(err, "No se pudo cambiar el estado del refugio."));
    }
  }

  const visibles = (datos ?? []).filter(
    (r) => filtro === "todos" || String(r.estadoAprobacion).toLowerCase() === filtro
  );

  return (
    <>
      <div className="pestanas">
        {FILTROS_REFUGIO.map((f) => (
          <button key={f.valor} className={filtro === f.valor ? "activa" : ""} onClick={() => setFiltro(f.valor)}>
            {f.etiqueta}
          </button>
        ))}
      </div>
      {filtro === "pendiente" && (
        <p className="muted">Revisa la documentación antes de aprobar un refugio.</p>
      )}

      {cargando && <Cargando />}
      <ErrorMensaje mensaje={error || errorAccion} />
      {datos && visibles.length === 0 && <Vacio>No hay refugios en este estado.</Vacio>}

      <div className="lista">
        {visibles.map((r) => {
          const doc = resolveImageUrl(r.documentacionUrl);
          const estado = String(r.estadoAprobacion).toLowerCase();
          return (
            <article className="item" key={r.idRefugio}>
              <div className="item-cabecera">
                <h2>{r.nombreOrganizacion}</h2>
                <EstadoBadge estado={r.estadoAprobacion} />
              </div>
              <p className="muted">
                {r.municipio}, {r.departamento} · {r.contacto}
                {r.cuenta?.correo && ` · ${r.cuenta.correo}`}
              </p>
              <p>
                {doc ? (
                  <a href={doc} target="_blank" rel="noreferrer">Ver documentación</a>
                ) : (
                  <span className="muted">Sin documentación adjunta</span>
                )}
              </p>
              <div className="tarjeta-acciones">
                {estado !== "aprobado" && (
                  <button className="btn" onClick={() => decidir(r, "aprobado")}>Aprobar</button>
                )}
                {estado !== "rechazado" && (
                  <button className="btn btn-peligro" onClick={() => decidir(r, "rechazado")}>Rechazar</button>
                )}
              </div>
            </article>
          );
        })}
      </div>
    </>
  );
}

function FormAnuncio({ alCrear }) {
  const [form, setForm] = useState({
    nombreTienda: "",
    contactoTienda: "",
    descripcion: "",
    precio: "",
    fechaInicio: "",
    fechaFin: "",
  });
  const [error, setError] = useState("");
  const [enviando, setEnviando] = useState(false);
  const cambiar = (c) => (e) => setForm({ ...form, [c]: e.target.value });

  async function enviar(e) {
    e.preventDefault();
    if (form.fechaFin <= form.fechaInicio) {
      return setError("La fecha de fin debe ser posterior a la de inicio.");
    }
    setError("");
    setEnviando(true);
    try {
      await api.crearAnuncio({
        nombreTienda: form.nombreTienda.trim(),
        contactoTienda: form.contactoTienda.trim() || undefined,
        descripcion: form.descripcion.trim() || undefined,
        precio: Number(form.precio),
        fechaInicio: new Date(form.fechaInicio).toISOString(),
        fechaFin: new Date(form.fechaFin).toISOString(),
      });
      setForm({ nombreTienda: "", contactoTienda: "", descripcion: "", precio: "", fechaInicio: "", fechaFin: "" });
      alCrear();
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo registrar el anuncio."));
    } finally {
      setEnviando(false);
    }
  }

  return (
    <details className="tarjeta-form">
      <summary>Registrar un anuncio nuevo</summary>
      <form onSubmit={enviar}>
        <label>
          Nombre de la tienda
          <input value={form.nombreTienda} onChange={cambiar("nombreTienda")} maxLength={100} required />
        </label>
        <label>
          Contacto
          <input value={form.contactoTienda} onChange={cambiar("contactoTienda")} maxLength={150} />
        </label>
        <label>
          Descripción
          <input value={form.descripcion} onChange={cambiar("descripcion")} maxLength={255} />
        </label>
        <label>
          Precio (USD)
          <input type="number" min="0.01" step="0.01" value={form.precio} onChange={cambiar("precio")} required />
        </label>
        <div className="coordenadas">
          <label>
            Inicio
            <input type="date" value={form.fechaInicio} onChange={cambiar("fechaInicio")} required />
          </label>
          <label>
            Fin
            <input type="date" value={form.fechaFin} onChange={cambiar("fechaFin")} required />
          </label>
        </div>
        <ErrorMensaje mensaje={error} />
        <button className="btn" type="submit" disabled={enviando}>{enviando ? "Guardando..." : "Registrar anuncio"}</button>
      </form>
    </details>
  );
}

function Resumen() {
  const { datos: m } = useDatos(() => api.monetizacion(), "resumen");
  if (!m) return null;
  return (
    <div className="resumen">
      <div><strong>{dinero(m.ingresosMes)}</strong><span>Ingresos del mes</span></div>
      <div><strong>{m.campanasActivas}</strong><span>Activas</span></div>
      <div><strong>{m.campanasPendientes}</strong><span>Pendientes</span></div>
      <div><strong>{m.campanasVencidas}</strong><span>Vencidas</span></div>
    </div>
  );
}

function TabAnuncios() {
  const [estado, setEstado] = useState("pendiente");
  const [errorAccion, setErrorAccion] = useState("");
  const [version, setVersion] = useState(0);
  const { datos, cargando, error, recargar } = useDatos(
    async () => comoLista(await api.anuncios(estado)),
    `${estado}-${version}`
  );

  function refrescar() {
    setVersion((v) => v + 1);
    recargar();
  }

  async function ejecutar(accion, a) {
    setErrorAccion("");
    try {
      await accion(a.idAnuncio);
      refrescar();
    } catch (err) {
      setErrorAccion(mensajeDeError(err, "No se pudo completar la acción."));
    }
  }

  async function eliminar(a) {
    if (!window.confirm(`¿Eliminar el anuncio de ${a.nombreTienda}?`)) return;
    ejecutar(api.eliminarAnuncio, a);
  }

  return (
    <>
      <Resumen key={version} />
      <FormAnuncio alCrear={refrescar} />

      <div className="pestanas">
        {["pendiente", "activo", "vencido", "todas"].map((v) => (
          <button key={v} className={estado === v ? "activa" : ""} onClick={() => setEstado(v)}>{v}</button>
        ))}
      </div>

      {cargando && <Cargando />}
      <ErrorMensaje mensaje={error || errorAccion} />
      {datos && datos.length === 0 && <Vacio>No hay anuncios en este estado.</Vacio>}

      <div className="lista">
        {datos?.map((a) => {
          const pendiente = String(a.estado).toLowerCase() === "pendiente";
          return (
            <article className="item" key={a.idAnuncio}>
              <div className="item-cabecera">
                <h2>{a.nombreTienda}</h2>
                <div>
                  <EstadoBadge estado={a.estado} />{" "}
                  <span className={a.pagoConfirmado ? "badge badge-ok" : "badge badge-aviso"}>
                    {a.pagoConfirmado ? "Pago confirmado" : "Pago pendiente"}
                  </span>
                </div>
              </div>
              {a.descripcion && <p>{a.descripcion}</p>}
              <p className="muted">
                {a.contactoTienda && `${a.contactoTienda} · `}
                {dinero(a.precio)} · {fecha(a.fechaInicio)} a {fecha(a.fechaFin)}
              </p>
              <div className="tarjeta-acciones">
                {pendiente && <button className="btn" onClick={() => ejecutar(api.aprobarAnuncio, a)}>Aprobar</button>}
                {!a.pagoConfirmado && <button className="btn btn-sec" onClick={() => ejecutar(api.confirmarPagoAnuncio, a)}>Confirmar pago</button>}
                {pendiente && <button className="btn btn-peligro" onClick={() => ejecutar(api.rechazarAnuncio, a)}>Rechazar</button>}
                <button className="btn btn-sec" onClick={() => eliminar(a)}>Eliminar</button>
              </div>
            </article>
          );
        })}
      </div>
    </>
  );
}

export default function PanelAdmin() {
  const [tab, setTab] = useState("refugios");

  return (
    <>
      <h1>Panel de administración</h1>
      <div className="pestanas" role="tablist">
        <button role="tab" aria-selected={tab === "refugios"} className={tab === "refugios" ? "activa" : ""} onClick={() => setTab("refugios")}>
          Refugios
        </button>
        <button role="tab" aria-selected={tab === "anuncios"} className={tab === "anuncios" ? "activa" : ""} onClick={() => setTab("anuncios")}>
          Anuncios
        </button>
      </div>
      {tab === "refugios" ? <TabRefugios /> : <TabAnuncios />}
    </>
  );
}
