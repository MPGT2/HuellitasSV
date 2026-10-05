// Algunos endpoints devuelven un arreglo y otros un objeto con el arreglo
// dentro (por ejemplo { mascotas: [...] }). Esto acepta ambos.
export function comoLista(data) {
  if (Array.isArray(data)) return data;
  if (data && typeof data === "object") {
    return Object.values(data).find(Array.isArray) ?? [];
  }
  return [];
}

export function fecha(valor) {
  if (!valor) return "";
  const d = new Date(valor);
  return Number.isNaN(d.getTime())
    ? ""
    : d.toLocaleDateString("es-SV", { day: "numeric", month: "short", year: "numeric" });
}

export function etiquetaEstado(estado) {
  return String(estado ?? "").replace(/_/g, " ");
}

// Color del distintivo según el estado.
export function tonoEstado(estado) {
  switch (String(estado ?? "").toLowerCase()) {
    case "aprobada":
    case "aprobado":
    case "disponible":
    case "atendido":
    case "atendida":
    case "activo":
    case "activa":
    case "cubierta":
      return "ok";
    case "pendiente":
    case "reservada":
    case "en_tratamiento":
      return "aviso";
    case "rechazada":
    case "rechazado":
    case "fallecida":
    case "vencido":
      return "peligro";
    case "adoptada":
      return "info";
    default:
      return "neutro";
  }
}

export function edadTexto(meses) {
  const m = Number(meses);
  if (!Number.isFinite(m)) return "";
  if (m < 12) return `${m} ${m === 1 ? "mes" : "meses"}`;
  const anios = Math.floor(m / 12);
  const resto = m % 12;
  const a = `${anios} ${anios === 1 ? "año" : "años"}`;
  return resto ? `${a} y ${resto} ${resto === 1 ? "mes" : "meses"}` : a;
}

export function dinero(valor) {
  const n = Number(valor);
  return Number.isFinite(n)
    ? n.toLocaleString("es-SV", { style: "currency", currency: "USD" })
    : "";
}
