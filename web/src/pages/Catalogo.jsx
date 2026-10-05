import { useState } from "react";
import { api } from "../services/api";
import { useDatos } from "../hooks/useDatos";
import { comoLista } from "../utils/formato";
import { DEPARTAMENTOS, ESPECIES, SALUD, TAMANOS } from "../config/opciones";
import TarjetaMascota from "../components/TarjetaMascota";
import { Cargando, ErrorMensaje, Vacio } from "../components/Estado";

const FILTROS_VACIOS = {
  especie: "",
  tamano: "",
  edadMaxMeses: "",
  estadoSalud: "",
  departamento: "",
  municipio: "",
};

// Misma lógica que el catálogo de la app móvil: la API tiene endpoints
// distintos según el filtro, y lo que no cubren se acota aquí.
async function consultar(f) {
  const edad = f.edadMaxMeses ? parseInt(f.edadMaxMeses, 10) : null;
  const coincide = (m) =>
    (!f.especie || m.especie?.toLowerCase() === f.especie) &&
    (!f.tamano || m.tamano?.toLowerCase() === f.tamano) &&
    (!f.estadoSalud || m.estadoSalud?.toLowerCase() === f.estadoSalud) &&
    (edad == null || Number.isNaN(edad) || (m.edadMeses ?? 0) <= edad);

  if (f.departamento || f.municipio) {
    const data = await api.mascotasPorUbicacion({
      departamento: f.departamento || undefined,
      municipio: f.municipio || undefined,
    });
    return comoLista(data).filter(coincide);
  }

  const hayAtributos = f.tamano || f.edadMaxMeses || f.estadoSalud;
  if (!f.especie && !hayAtributos) return comoLista(await api.catalogo());
  if (f.especie && !hayAtributos) return comoLista(await api.mascotasPorEspecie(f.especie));

  const data = await api.mascotasPorAtributos({
    tamano: f.tamano || undefined,
    edadMaxMeses: edad ?? undefined,
    estadoSalud: f.estadoSalud || undefined,
  });
  return comoLista(data).filter(coincide);
}

export default function Catalogo() {
  const [borrador, setBorrador] = useState(FILTROS_VACIOS);
  const [aplicados, setAplicados] = useState(FILTROS_VACIOS);
  const { datos, cargando, error } = useDatos(
    () => consultar(aplicados),
    JSON.stringify(aplicados)
  );

  const cambiar = (campo) => (e) =>
    setBorrador({
      ...borrador,
      [campo]: e.target.value,
      // El municipio solo tiene sentido dentro del departamento elegido.
      ...(campo === "departamento" ? { municipio: "" } : {}),
    });

  function aplicar(e) {
    e.preventDefault();
    setAplicados(borrador);
  }

  function limpiar() {
    setBorrador(FILTROS_VACIOS);
    setAplicados(FILTROS_VACIOS);
  }

  return (
    <>
      <h1>Catálogo de mascotas</h1>

      <form className="filtros" onSubmit={aplicar}>
        <label>
          Especie
          <select value={borrador.especie} onChange={cambiar("especie")}>
            <option value="">Todas</option>
            {ESPECIES.map((o) => <option key={o.valor} value={o.valor}>{o.etiqueta}</option>)}
          </select>
        </label>
        <label>
          Tamaño
          <select value={borrador.tamano} onChange={cambiar("tamano")}>
            <option value="">Todos</option>
            {TAMANOS.map((o) => <option key={o.valor} value={o.valor}>{o.etiqueta}</option>)}
          </select>
        </label>
        <label>
          Edad máxima (meses)
          <input type="number" min="0" value={borrador.edadMaxMeses} onChange={cambiar("edadMaxMeses")} />
        </label>
        <label>
          Salud
          <select value={borrador.estadoSalud} onChange={cambiar("estadoSalud")}>
            <option value="">Todas</option>
            {SALUD.map((o) => <option key={o.valor} value={o.valor}>{o.etiqueta}</option>)}
          </select>
        </label>
        <label>
          Departamento
          <select value={borrador.departamento} onChange={cambiar("departamento")}>
            <option value="">Todos</option>
            {DEPARTAMENTOS.map((d) => <option key={d} value={d}>{d}</option>)}
          </select>
        </label>
        <label>
          Municipio
          <input value={borrador.municipio} onChange={cambiar("municipio")} />
        </label>
        <div className="filtros-botones">
          <button className="btn" type="submit">Aplicar filtros</button>
          <button className="btn btn-sec" type="button" onClick={limpiar}>Limpiar</button>
        </div>
      </form>

      {cargando && <Cargando />}
      <ErrorMensaje mensaje={error} />
      {datos && datos.length === 0 && <Vacio>No hay mascotas con esos filtros.</Vacio>}
      {datos && datos.length > 0 && (
        <div className="rejilla">
          {datos.map((m) => (
            <TarjetaMascota key={m.idMascota ?? m.id} mascota={m} />
          ))}
        </div>
      )}
    </>
  );
}
