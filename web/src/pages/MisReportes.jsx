import { useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../services/api";
import { useDatos } from "../hooks/useDatos";
import { mensajeDeError } from "../services/errores";
import { resolveImageUrl } from "../config/env";
import { comoLista, fecha } from "../utils/formato";
import EstadoBadge from "../components/EstadoBadge";
import { Cargando, ErrorMensaje, Vacio } from "../components/Estado";

export default function MisReportes() {
  const [errorAccion, setErrorAccion] = useState("");
  const { datos, cargando, error, recargar } = useDatos(
    async () => comoLista(await api.misReportes()),
    "mis"
  );

  async function eliminar(id) {
    if (!window.confirm("¿Eliminar este reporte?")) return;
    setErrorAccion("");
    try {
      await api.eliminarReporte(id);
      recargar();
    } catch (err) {
      setErrorAccion(mensajeDeError(err, "No se pudo eliminar el reporte."));
    }
  }

  return (
    <>
      <h1>Mis reportes</h1>
      <p><Link className="btn" to="/reportar">Reportar un animal</Link></p>
      {cargando && <Cargando />}
      <ErrorMensaje mensaje={error || errorAccion} />
      {datos && datos.length === 0 && <Vacio>Todavía no has enviado reportes.</Vacio>}
      <div className="lista">
        {datos?.map((r) => {
          const foto = resolveImageUrl(r.fotoUrl);
          return (
            <article className={foto ? "item item-foto" : "item"} key={r.idReporte}>
              {foto && <img src={foto} alt="Foto del animal reportado" loading="lazy" />}
              <div>
                <div className="item-cabecera">
                  <h2>Reporte #{r.idReporte}</h2>
                  <EstadoBadge estado={r.estado} />
                </div>
                <p>{r.descripcion}</p>
                <p className="muted">Enviado el {fecha(r.fechaRegistro)}</p>
                <div className="tarjeta-acciones">
                  <button className="btn btn-peligro" onClick={() => eliminar(r.idReporte)}>Eliminar</button>
                </div>
              </div>
            </article>
          );
        })}
      </div>
    </>
  );
}
