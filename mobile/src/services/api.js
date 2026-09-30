// API Service for HuellitasSV Backend
import AsyncStorage from '@react-native-async-storage/async-storage';
import { API_BASE_URL } from '../config/env';

const TOKEN_KEY = '@huellitas_token';
const USER_KEY = '@huellitas_user';

class ApiService {
  constructor() {
    this.token = null;
    this.user = null;
  }

  async init() {
    this.token = await AsyncStorage.getItem(TOKEN_KEY);
    const userStr = await AsyncStorage.getItem(USER_KEY);
    if (userStr) this.user = JSON.parse(userStr);
  }

  async setAuth(token, user) {
    this.token = token;
    this.user = user;
    await AsyncStorage.setItem(TOKEN_KEY, token);
    await AsyncStorage.setItem(USER_KEY, JSON.stringify(user));
  }

  async clearAuth() {
    this.token = null;
    this.user = null;
    await AsyncStorage.removeItem(TOKEN_KEY);
    await AsyncStorage.removeItem(USER_KEY);
  }

  getHeaders(includeAuth = true) {
    const headers = {
      'Content-Type': 'application/json',
    };
    if (includeAuth && this.token) {
      headers['Authorization'] = `Bearer ${this.token}`;
    }
    return headers;
  }

  async request(endpoint, options = {}) {
    const url = `${API_BASE_URL}${endpoint}`;
    const isForm = options.body instanceof FormData;

    const headers = { ...this.getHeaders(options.auth !== false), ...options.headers };
    // Con FormData la plataforma debe fijar el Content-Type con su boundary.
    // Dejar "application/json" aqui rompe el parseo multipart en el servidor.
    if (isForm) delete headers['Content-Type'];

    const config = {
      ...options,
      headers,
    };

    // FormData ya fija su propio boundary: no se puede stringify.
    if (config.body && !isForm) {
      config.body = JSON.stringify(config.body);
    }

    let response;
    try {
      response = await fetch(url, config);
    } catch (error) {
      // fetch solo rechaza cuando no hubo respuesta (API apagada, IP mal,
      // firewall). Un 4xx o 5xx es una respuesta valida y se reporta arriba.
      throw new Error(
        `No se pudo conectar con la API (${API_BASE_URL}). ` +
          'Verifica que la API este corriendo con el perfil "lan" y que el telefono este en la misma red Wi-Fi.'
      );
    }

    // 204 y algunos 200 vienen sin cuerpo. Algunos endpoints devuelven errores
    // como texto plano (no JSON), asi que el parse es tolerante: si no es JSON
    // se conserva el texto para poder mostrarlo al usuario.
    const texto = await response.text();
    let data = null;
    if (texto) {
      try {
        data = JSON.parse(texto);
      } catch {
        data = texto;
      }
    }

    if (!response.ok) {
      const mensaje =
        (typeof data === 'string' ? data : null) ||
        data?.error ||
        data?.message ||
        data?.title ||
        data?.errores?.[0] ||
        data?.errors?.[0] ||
        `Error ${response.status}`;
      throw new Error(mensaje);
    }

    return data;
  }

  // Auth endpoints
  // El login es unico: la API deduce el rol desde la cuenta. La app no elige.
  // El registro sigue separado porque cada rol tiene su propio contrato.
  async login(correo, contrasena) {
    return this.request('/Auth/login', {
      method: 'POST',
      body: { correo, contrasena },
      auth: false,
    });
  }

  async registerUsuario(nombre, correo, contrasena) {
    return this.request('/Usuarios/registro', {
      method: 'POST',
      body: { nombre, correo, contrasena },
      auth: false,
    });
  }

  async registerRefugio(nombreOrganizacion, correo, contrasena, departamento, municipio, contacto, documento) {
    // El endpoint usa [FromForm]: todo viaja como multipart/form-data.
    const formData = new FormData();
    formData.append('NombreOrganizacion', nombreOrganizacion);
    formData.append('Correo', correo);
    formData.append('Contrasena', contrasena);
    formData.append('Departamento', departamento);
    formData.append('Municipio', municipio);
    formData.append('Contacto', contacto);
    if (documento) {
      formData.append('Documentacion', {
        uri: documento.uri,
        type: documento.mimeType || 'application/octet-stream',
        name: documento.name || 'documento.pdf',
      });
    }
    return this.request('/Refugios/registro', {
      method: 'POST',
      body: formData,
      auth: false,
    });
  }

  // User profile
  async getUsuarioPerfil() {
    return this.request('/Usuarios/perfil');
  }

  async updateUsuarioPerfil(data) {
    return this.request('/Usuarios/perfil', {
      method: 'PUT',
      body: data,
    });
  }

  async deleteUsuarioPerfil() {
    return this.request('/Usuarios/perfil', {
      method: 'DELETE',
    });
  }

  // Refugio profile
  async getRefugioPerfil() {
    return this.request('/Refugios/perfil');
  }

  // Perfil publico de un refugio (nombre, ubicacion, contacto y calificacion).
  async getRefugioById(id) {
    return this.request(`/Refugios/${id}`, { auth: false });
  }

  // Calificar un refugio (1-5 estrellas). Si ya se califico, se actualiza.
  async calificarRefugio(id, estrellas, comentario) {
    return this.request(`/Refugios/${id}/calificar`, {
      method: 'POST',
      body: { estrellas, comentario },
    });
  }

  // Calificacion que el usuario autenticado le dio a un refugio (0 si no ha calificado).
  async getMiCalificacion(id) {
    return this.request(`/Refugios/${id}/mi-calificacion`);
  }

  async updateRefugioPerfil(data) {
    return this.request('/Refugios/perfil', {
      method: 'PUT',
      body: data,
    });
  }

  async deleteRefugioPerfil() {
    return this.request('/Refugios/perfil', {
      method: 'DELETE',
    });
  }

  // Mascotas (Catalog)
  async getCatalogo() {
    return this.request('/Mascotas', { auth: false });
  }

  async getMascotaById(id) {
    return this.request(`/Mascotas/${id}`, { auth: false });
  }

  async getMascotasByEspecie(especie) {
    return this.request(`/Mascotas/especie/${especie}`, { auth: false });
  }

  async getMascotasByAtributos({ tamano, edadMaxMeses, estadoSalud }) {
    const params = new URLSearchParams();
    if (tamano) params.append('tamano', tamano);
    if (edadMaxMeses) params.append('edadMaxMeses', edadMaxMeses);
    if (estadoSalud) params.append('estadoSalud', estadoSalud);
    return this.request(`/Mascotas/atributos?${params}`, { auth: false });
  }

  async getMascotasByUbicacion({ departamento, municipio }) {
    const params = new URLSearchParams();
    if (departamento) params.append('departamento', departamento);
    if (municipio) params.append('municipio', municipio);
    return this.request(`/Mascotas/ubicacion?${params}`, { auth: false });
  }

  async getMascotasByRefugio(idRefugio, estado) {
    const params = new URLSearchParams();
    if (estado) params.append('estado', estado);
    return this.request(`/Mascotas/refugio/${idRefugio}?${params}`, { auth: false });
  }

  async registrarMascota(data, imagen) {
    const formData = new FormData();
    Object.keys(data).forEach((key) => {
      const valor = data[key];
      if (valor !== undefined && valor !== null) {
        formData.append(key, String(valor));
      }
    });
    if (imagen) {
      formData.append('imagen', {
        uri: imagen.uri,
        type: imagen.type || 'image/jpeg',
        name: imagen.name || 'mascota.jpg',
      });
    }
    // No se fija Content-Type: la plataforma lo arma con el boundary correcto.
    // Declararlo a mano produce "Missing boundary" en el servidor.
    return this.request('/Mascotas', {
      method: 'POST',
      body: formData,
    });
  }

  async actualizarMascota(id, data, imagen) {
    // El PUT usa [FromForm] en el backend: se envia multipart y la imagen es opcional.
    const formData = new FormData();
    Object.keys(data).forEach((key) => {
      const valor = data[key];
      if (valor !== undefined && valor !== null) {
        formData.append(key, String(valor));
      }
    });
    if (imagen) {
      formData.append('imagen', {
        uri: imagen.uri,
        type: imagen.type || 'image/jpeg',
        name: imagen.name || 'mascota.jpg',
      });
    }
    return this.request(`/Mascotas/${id}`, {
      method: 'PUT',
      body: formData,
    });
  }

  async eliminarMascota(id) {
    return this.request(`/Mascotas/${id}`, {
      method: 'DELETE',
    });
  }

  // Solicitudes de adopcion
  async getMisSolicitudes(estado) {
    const params = estado ? `?estado=${estado}` : '';
    return this.request(`/SolicitudesAdopcion${params}`);
  }

  async getSolicitudById(id) {
    return this.request(`/SolicitudesAdopcion/${id}`);
  }

  async crearSolicitud(idMascota, nombreContacto, telefonoContacto, correoContacto) {
    return this.request('/SolicitudesAdopcion', {
      method: 'POST',
      body: { idMascota, nombreContacto, telefonoContacto, correoContacto },
    });
  }

  // Gestion de solicitudes (Refugio)
  async getSolicitudesRefugio(estado) {
    const params = estado ? `?estado=${estado}` : '';
    return this.request(`/GestionSolicitudes${params}`);
  }

  async aprobarSolicitud(id, comentarioDecision) {
    return this.request(`/GestionSolicitudes/${id}/aprobar`, {
      method: 'PUT',
      body: { comentarioDecision },
    });
  }

  async rechazarSolicitud(id, comentarioDecision) {
    return this.request(`/GestionSolicitudes/${id}/rechazar`, {
      method: 'PUT',
      body: { comentarioDecision },
    });
  }

  // Reportes
  async getMisReportes(estado) {
    const params = estado ? `?estado=${estado}` : '';
    return this.request(`/ReportesAnimales${params}`);
  }

  async getReporteById(id) {
    return this.request(`/ReportesAnimales/${id}`);
  }

  async crearReporte({ descripcion, latitud, longitud, fotoUrl }, foto) {
    // multipart/form-data: la foto viaja como archivo; FotoUrl es el respaldo.
    const formData = new FormData();
    formData.append('Descripcion', descripcion);
    formData.append('Latitud', String(latitud));
    formData.append('Longitud', String(longitud));
    if (fotoUrl) formData.append('FotoUrl', fotoUrl);
    if (foto) {
      formData.append('Foto', {
        uri: foto.uri,
        type: foto.mimeType || 'image/jpeg',
        name: foto.fileName || 'reporte.jpg',
      });
    }
    return this.request('/ReportesAnimales', {
      method: 'POST',
      body: formData,
    });
  }

  // Reportes rescate (Refugio)
  async getReportesRescate(estado) {
    const params = estado ? `?estado=${estado}` : '';
    return this.request(`/ReportesRescate${params}`);
  }

  async marcarReporteAtendido(id) {
    return this.request(`/ReportesRescate/${id}/atendido`, {
      method: 'PUT',
    });
  }

  // Necesidades de donacion
  async getNecesidades(estado = 'activa') {
    return this.request(`/NecesidadesDonacion?estado=${estado}`, { auth: false });
  }

  async publicarNecesidad(tipoInsumo, descripcion, cantidadRequerida) {
    return this.request('/NecesidadesDonacion', {
      method: 'POST',
      body: { tipoInsumo, descripcion, cantidadRequerida },
    });
  }

  async aportarNecesidad(id, cantidad) {
    return this.request(`/NecesidadesDonacion/${id}/aportar`, {
      method: 'POST',
      body: { cantidad },
    });
  }

  async eliminarNecesidad(id) {
    return this.request(`/NecesidadesDonacion/${id}`, {
      method: 'DELETE',
    });
  }

  // Notificaciones
  async getNotificaciones(leidas) {
    const params = leidas !== undefined ? `?leidas=${leidas}` : '';
    return this.request(`/Notificaciones${params}`);
  }

  async marcarNotificacionLeida(id) {
    return this.request(`/Notificaciones/${id}/leida`, {
      method: 'PUT',
    });
  }

  // Anuncios
  async getAnuncios(estado = 'activo') {
    return this.request(`/Anuncios?estado=${estado}`, { auth: false });
  }

  async getAnuncioById(id) {
    return this.request(`/Anuncios/${id}`, { auth: false });
  }

  async getAnunciosPorRefugio(idRefugio) {
    return this.request(`/Anuncios/refugio/${idRefugio}`, { auth: false });
  }

  async crearAnuncio(data) {
    return this.request('/Anuncios', {
      method: 'POST',
      body: data,
    });
  }

  async aprobarAnuncio(id) {
    return this.request(`/Anuncios/${id}/aprobar`, {
      method: 'POST',
    });
  }

  async confirmarPagoAnuncio(id) {
    return this.request(`/Anuncios/${id}/confirmar-pago`, {
      method: 'POST',
    });
  }

  async rechazarAnuncio(id) {
    return this.request(`/Anuncios/${id}/rechazar`, {
      method: 'POST',
    });
  }

  async actualizarAnuncio(id, data) {
    return this.request(`/Anuncios/${id}`, {
      method: 'PUT',
      body: data,
    });
  }

  async eliminarAnuncio(id) {
    return this.request(`/Anuncios/${id}`, {
      method: 'DELETE',
    });
  }

  // Auth helpers
  isLoggedIn() {
    return !!this.token;
  }

  getUserRole() {
    return this.user?.rol;
  }

  getUserId() {
    return this.user?.idUsuario || this.user?.idRefugio;
  }

  getUserName() {
    return this.user?.nombre || this.user?.nombreOrganizacion;
  }

  isUsuario() {
    return this.user?.rol === 'Usuario';
  }

  isRefugio() {
    return this.user?.rol === 'Refugio';
  }

  isAdmin() {
    return this.user?.rol === 'Admin';
  }
}

export const api = new ApiService();
export default api;