// Saca un mensaje legible de un error de axios.
// La API a veces responde texto plano y a veces JSON.
export function mensajeDeError(err, porDefecto = "Ocurrió un error.") {
  if (!err.response) {
    return "No se pudo conectar con la API. Revisa tu conexión.";
  }
  const d = err.response.data;
  if (typeof d === "string" && d) return d;
  const m = d?.error ?? d?.message ?? d?.errores?.[0];
  if (m) return m;
  if (d?.errors) {
    const primero = Object.values(d.errors).flat()[0];
    if (primero) return primero;
  }
  return d?.title ?? porDefecto;
}
