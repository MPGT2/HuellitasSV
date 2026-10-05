import { useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../services/api";
import { useDatos } from "../hooks/useDatos";
import { useAuth } from "../context/useAuth";
import { mensajeDeError } from "../services/errores";
import { comoLista, fecha } from "../utils/formato";
import EstadoBadge from "../components/EstadoBadge";
import { Cargando, ErrorMensaje, Vacio } from "../components/Estado";

function Progreso({ cubierta, requerida }) {
  const pct = requerida > 0 ? Math.min(100, Math.round((cubierta / requerida) * 100)) : 0;
  return (
    <div className="progreso" role="progressbar" aria-valuenow={pct} aria-valuemin="0" aria-valuemax="100">
      <div style={{ width: `${pct}%` }} />
    </div>
  );
}

function FormNecesidad({ idRefugio, alPublicar }) {
  const [form, setForm] = useState({ tipoInsumo: "", descripcion: "", cantidadRequerida: "" });
  const [error, setError] = useState("");
  const [enviando, setEnviando] = useState(false);
  const cambiar = (c) => (e) => setForm({ ...form, [c]: e.target.value });

  async function enviar(e) {
    e.preventDefault();
    setError("");
    setEnviando(true);
    try {
      await api.publicarNecesidad({
        idRefugio: Number(idRefugio),
        tipoInsumo: form.tipoInsumo.trim(),
        descripcion: form.descripcion.trim() || undefined,
        cantidadRequerida: Number(form.cantidadRequerida),
      });
      setForm({ tipoInsumo: "", descripcion: "", cantidadRequerida: "" });
      alPublicar();
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo publicar la necesidad."));
    } finally {
      setEnviando(false);
    }
  }

  return (
    <form className="tarjeta-form" onSubmit={enviar}>
      <h2>Publicar una necesidad</h2>
      <label>
        Tipo de insumo
        <input value={form.tipoInsumo} onChange={cambiar("tipoInsumo")} placeholder="Alimento, medicinas..." required />
      </label>
      <label>
        Descripción
        <input value={form.descripcion} onChange={cambiar("descripcion")} maxLength={255} />
      </label>
      <label>
        Cantidad requerida
        <input type="number" min="0.01" step="any" value={form.cantidadRequerida} onChange={cambiar("cantidadRequerida")} required />
      </label>
      <ErrorMensaje mensaje={error} />
      <button className="btn" type="submit" disabled={enviando}>{enviando ? "Publicando..." : "Publicar"}</button>
    </form>
  );
}

function Aportar({ necesidad, alAportar }) {
  const [cantidad, setCantidad] = useState("");
  const [error, setError] = useState("");
  const [ok, setOk] = useState(false);

  async function enviar(e) {
    e.preventDefault();
    setError("");
    setOk(false);
    try {
      await api.aportarNecesidad(necesidad.idNecesidad, Number(cantidad));
      setCantidad("");
      setOk(true);
      alAportar();
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo registrar el aporte."));
    }
  }

  return (
    <form className="en-linea" onSubmit={enviar}>
      <input type="number" min="0.01" step="any" value={cantidad} onChange={(e) => setCantidad(e.target.value)} placeholder="Cantidad que aportas" aria-label="Cantidad que aportas" required />
      <button className="btn" type="submit">Aportar</button>
      <ErrorMensaje mensaje={error} />
      {ok && <span className="ok">¡Gracias por tu aporte!</span>}
    </form>
  );
}

export default function Necesidades() {
  const { user, autenticado, esUsuario, esRefugio } = useAuth();
  const [estado, setEstado] = useState("activa");
  const [errorAccion, setErrorAccion] = useState("");
  const { datos, cargando, error, recargar } = useDatos(
    async () => comoLista(await api.necesidades(estado)),
    estado
  );

  async function eliminar(n) {
    if (!window.confirm(`¿Eliminar la necesidad "${n.tipoInsumo}"?`)) return;
    setErrorAccion("");
    try {
      await api.eliminarNecesidad(n.idNecesidad);
      recargar();
    } catch (err) {
      setErrorAccion(mensajeDeError(err, "No se pudo eliminar la necesidad."));
    }
  }

  return (
    <>
      <h1>Necesidades de donación</h1>
      <p className="muted">Insumos que los refugios necesitan para cuidar a sus mascotas.</p>

      {esRefugio && <FormNecesidad idRefugio={user.idRefugio} alPublicar={recargar} />}

      <div className="pestanas">
        {["activa", "cubierta", "todas"].map((v) => (
          <button key={v} className={estado === v ? "activa" : ""} onClick={() => setEstado(v)}>{v}</button>
        ))}
      </div>

      {cargando && <Cargando />}
      <ErrorMensaje mensaje={error || errorAccion} />
      {datos && datos.length === 0 && <Vacio>No hay necesidades en este estado.</Vacio>}

      <div className="lista">
        {datos?.map((n) => (
          <article className="item" key={n.idNecesidad}>
            <div className="item-cabecera">
              <h2>{n.tipoInsumo}</h2>
              <EstadoBadge estado={n.estado} />
            </div>
            {n.descripcion && <p>{n.descripcion}</p>}
            <Progreso cubierta={n.cantidadCubierta} requerida={n.cantidadRequerida} />
            <p className="muted">
              Cubierto {n.cantidadCubierta} de {n.cantidadRequerida} · publicada el {fecha(n.fechaPublicacion)}
            </p>
            <p><Link to={`/refugios/${n.idRefugio}`}>Ver refugio</Link></p>

            {esUsuario && String(n.estado).toLowerCase() === "activa" && <Aportar necesidad={n} alAportar={recargar} />}
            {!autenticado && String(n.estado).toLowerCase() === "activa" && (
              <p className="info"><Link to="/login">Inicia sesión</Link> como usuario para aportar.</p>
            )}
            {esRefugio && Number(n.idRefugio) === Number(user.idRefugio) && (
              <div className="tarjeta-acciones">
                <button className="btn btn-peligro" onClick={() => eliminar(n)}>Eliminar</button>
              </div>
            )}
          </article>
        ))}
      </div>
    </>
  );
}
