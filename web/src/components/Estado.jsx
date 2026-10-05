// Mensajes comunes de carga, error y lista vacía.
export function Cargando() {
  return <p className="aviso-texto">Cargando...</p>;
}

export function ErrorMensaje({ mensaje }) {
  return mensaje ? <p className="error" role="alert">{mensaje}</p> : null;
}

export function Vacio({ children }) {
  return <p className="vacio">{children}</p>;
}
