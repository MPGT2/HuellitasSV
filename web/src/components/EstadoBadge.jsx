import { etiquetaEstado, tonoEstado } from "../utils/formato";

export default function EstadoBadge({ estado }) {
  if (!estado) return null;
  return <span className={`badge badge-${tonoEstado(estado)}`}>{etiquetaEstado(estado)}</span>;
}
