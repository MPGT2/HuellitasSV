// API Service for HuellitasSV Backend
import AsyncStorage from '@react-native-async-storage/async-storage';

// Use your local IP for Android emulator/device, localhost for iOS simulator
const BASE_URL = 'http://10.0.2.2:5000/api'; // Android emulator
// const BASE_URL = 'http://localhost:5000/api'; // iOS simulator

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
    const url = `${BASE_URL}${endpoint}`;
    const config = {
      headers: this.getHeaders(options.auth !== false),
      ...options,
    };

    if (config.body && typeof config.body === 'object') {
      config.body = JSON.stringify(config.body);
    }

    try {
      const response = await fetch(url, config);
      const data = await response.json();

      if (!response.ok) {
        const error = data?.error || data?.errors?.[0] || 'Error en la solicitud';
        throw new Error(error);
      }

      return data;
    } catch (error) {
      if (error instanceof TypeError && error.message.includes('Network')) {
        throw new Error('No se puede conectar al servidor. Verifica que el backend esté corriendo.');
      }
      throw error;
    }
  }

  // Auth endpoints
  async loginUsuario(correo, contrasena) {
    return this.request('/Usuarios/login', {
      method: 'POST',
      body: { correo, contrasena },
      auth: false,
    });
  }

  async loginRefugio(correo, contrasena) {
    return this.request('/Refugios/login', {
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

  async registerRefugio(nombreOrganizacion, correo, contrasena, departamento, municipio, contacto) {
    return this.request('/Refugios/registro', {
      method: 'POST',
      body: { nombreOrganizacion, correo, contrasena, departamento, municipio, contacto },
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
    Object.keys(data).forEach(key => {
      formData.append(key, data[key]);
    });
    if (imagen) {
      formData.append('imagen', {
        uri: imagen.uri,
        type: imagen.type || 'image/jpeg',
        name: imagen.name || 'imagen.jpg',
      });
    }
    return this.request('/Mascotas', {
      method: 'POST',
      body: formData,
      headers: {
        ...this.getHeaders(),
        'Content-Type': 'multipart/form-data',
      },
    });
  }

  async actualizarMascota(id, data) {
    return this.request(`/Mascotas/${id}`, {
      method: 'PUT',
      body: data,
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

  async crearReporte(descripcion, fotoUrl, latitud, longitud) {
    return this.request('/ReportesAnimales', {
      method: 'POST',
      body: { descripcion, fotoUrl, latitud, longitud },
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