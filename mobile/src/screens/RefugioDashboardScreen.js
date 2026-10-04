import { useCallback, useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  FlatList,
  Image,
  RefreshControl,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';
import { api } from '../services/api';
import { resolveImageUrl } from '../config/env';
import { useFocusEffect, useNavigation } from '@react-navigation/native';
import { useAuth } from '../context/AuthContext';

const estadoColor = (estado) => {
  switch (estado) {
    case 'Pendiente':
      return colors.warning;
    case 'Aprobada':
      return colors.success;
    case 'Rechazada':
      return colors.danger;
    default:
      return colors.muted;
  }
};

const estadoBg = (estado) => {
  switch (estado) {
    case 'Pendiente':
      return colors.warningBg;
    case 'Aprobada':
      return colors.successBg;
    case 'Rechazada':
      return colors.dangerBg;
    default:
      return colors.surfaceMuted;
  }
};

export default function RefugioDashboardScreen() {
  const [mascotas, setMascotas] = useState([]);
  const [solicitudes, setSolicitudes] = useState([]);
  const [reportes, setReportes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [activeTab, setActiveTab] = useState('mascotas');
  const [error, setError] = useState(null);
  const { user } = useAuth();
  const nav = useNavigation();

  const fetchData = async () => {
    try {
      setError(null);
      if (activeTab === 'mascotas') {
        const data = await api.getMascotasByRefugio(user.idRefugio);
        setMascotas(data);
      } else if (activeTab === 'solicitudes') {
        const data = await api.getSolicitudesRefugio();
        setSolicitudes(data);
      } else {
        // HU-15: reportes pendientes, con distancia y bandera de cercania.
        const data = await api.getReportesRescate('Pendiente');
        setReportes(Array.isArray(data) ? data : []);
      }
    } catch (err) {
      setError(err.message);
      console.error('Error fetching data:', err);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  const onRefresh = () => {
    setRefreshing(true);
    fetchData();
  };

  // Refresca al volver de MascotaForm (alta/edicion) o al cambiar de pestaña.
  useFocusEffect(
    useCallback(() => {
      fetchData();
    }, [activeTab, user.idRefugio]),
  );

  const confirmarEliminar = (mascota) => {
    Alert.alert(
      'Eliminar mascota',
      `¿Seguro que quieres eliminar a ${mascota.nombre || 'esta mascota'}?`,
      [
        { text: 'Cancelar', style: 'cancel' },
        {
          text: 'Eliminar',
          style: 'destructive',
          onPress: async () => {
            try {
              await api.eliminarMascota(mascota.idMascota);
              fetchData();
            } catch (err) {
              Alert.alert('Error', err.message);
            }
          },
        },
      ],
    );
  };

  const atenderReporte = (reporte) => {
    Alert.alert(
      'Marcar como atendido',
      '¿Confirmas que este reporte ya fue atendido?',
      [
        { text: 'Cancelar', style: 'cancel' },
        {
          text: 'Sí, atender',
          onPress: async () => {
            try {
              await api.marcarReporteAtendido(reporte.idReporte);
              fetchData();
            } catch (err) {
              Alert.alert('Error', err.message);
            }
          },
        },
      ],
    );
  };

  const renderReporte = ({ item }) => (
    <View style={styles.card}>
      <View style={styles.mascotaRow}>
        {resolveImageUrl(item.fotoUrl) ? (
          <Image source={{ uri: resolveImageUrl(item.fotoUrl) }} style={styles.mascotaThumb} />
        ) : (
          <View style={[styles.mascotaThumb, styles.mascotaThumbEmpty]}>
            <Ionicons name="image-outline" size={20} color={colors.muted} />
          </View>
        )}
        <View style={styles.cardDetails}>
          <Text style={styles.petName} numberOfLines={2}>
            {item.descripcion}
          </Text>
          <View style={styles.reporteMeta}>
            <Ionicons
              name={item.cerca ? 'location' : 'navigate-outline'}
              size={14}
              color={item.cerca ? colors.success : colors.warning}
            />
            <Text
              style={[
                styles.detail,
                { color: item.cerca ? colors.success : colors.warning },
              ]}
            >
              {item.distanciaKm != null
                ? `${item.distanciaKm} km ${item.cerca ? '· cerca' : '· lejos'}`
                : 'Distancia no disponible'}
            </Text>
          </View>
        </View>
      </View>
      <View style={styles.cardActions}>
        <TouchableOpacity
          style={styles.approveButton}
          onPress={() => atenderReporte(item)}
        >
          <Text style={styles.approveText}>Marcar atendido</Text>
        </TouchableOpacity>
      </View>
    </View>
  );

  const renderMascota = ({ item }) => (
    <TouchableOpacity
      style={styles.card}
      onPress={() => nav.navigate('PetDetail', { pet: item })}
      activeOpacity={0.8}
    >
      <View style={styles.mascotaRow}>
        {resolveImageUrl(item.imagenUrl) ? (
          <Image
            source={{ uri: resolveImageUrl(item.imagenUrl) }}
            style={styles.mascotaThumb}
          />
        ) : (
          <View style={[styles.mascotaThumb, styles.mascotaThumbEmpty]}>
            <Ionicons name="paw" size={20} color={colors.muted} />
          </View>
        )}
        <View style={styles.cardDetails}>
          <View style={styles.cardHeader}>
            <Text style={styles.petName} numberOfLines={1}>
              {item.nombre}
            </Text>
            <View style={styles.estadoMascotaBadge}>
              <Text style={styles.estadoMascotaText}>{item.estado}</Text>
            </View>
          </View>
          <Text style={styles.detail}>
            <Ionicons name="paw" size={14} color={colors.muted} />
            {item.especie} · {item.tamano}
          </Text>
          <Text style={styles.detail}>
            <Ionicons name="time" size={14} color={colors.muted} />
            {item.edadMeses} meses
          </Text>
          <Text style={styles.detail}>
            <Ionicons name="medical" size={14} color={colors.muted} />
            {item.estadoSalud}
          </Text>
        </View>
      </View>

      <View style={styles.cardActions}>
        <TouchableOpacity
          style={styles.actionButton}
          onPress={() => nav.navigate('MascotaForm', { mascota: item })}
        >
          <Ionicons name="create-outline" size={16} color={colors.primary} />
          <Text style={styles.actionText}>Editar</Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={styles.actionButton}
          onPress={() => confirmarEliminar(item)}
        >
          <Ionicons name="trash-outline" size={16} color={colors.danger} />
          <Text style={[styles.actionText, styles.actionTextDanger]}>Eliminar</Text>
        </TouchableOpacity>
      </View>
    </TouchableOpacity>
  );

  const renderSolicitud = ({ item }) => (
    <View style={styles.card}>
      <View style={styles.solicitudHeader}>
        <View style={styles.solicitudMascota}>
          <Text style={styles.petName}>{item.mascota?.nombre || 'Mascota'}</Text>
          <Text style={styles.speciesText} numberOfLines={1}>
            {item.mascota?.especie || ''}
          </Text>
        </View>
        <View style={[styles.statusBadge, { backgroundColor: estadoBg(item.estado) }]}>
          <View style={[styles.statusDot, { backgroundColor: estadoColor(item.estado) }]} />
          <Text style={[styles.statusText, { color: estadoColor(item.estado) }]}>
            {item.estado}
          </Text>
        </View>
      </View>
      <View style={styles.solicitudDetails}>
        <Text style={styles.detail}>
          <Ionicons name="person" size={14} color={colors.muted} />
          {item.usuario?.nombre || 'Usuario'}
        </Text>
        <Text style={styles.detail}>
          <Ionicons name="mail" size={14} color={colors.muted} />
          {item.correoContacto}
        </Text>
        <Text style={styles.detail}>
          <Ionicons name="call" size={14} color={colors.muted} />
          {item.telefonoContacto}
        </Text>
      </View>
      {item.estado === 'Pendiente' ? (
        <View style={styles.solicitudActions}>
          <TouchableOpacity
            style={styles.approveButton}
            onPress={() => handleDecision(item.idSolicitud, true)}
          >
            <Text style={styles.approveText}>Aprobar</Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={styles.rejectButton}
            onPress={() => handleDecision(item.idSolicitud, false)}
          >
            <Text style={styles.rejectText}>Rechazar</Text>
          </TouchableOpacity>
        </View>
      ) : null}
    </View>
  );

  const handleDecision = async (id, approve) => {
    const comentario = approve 
      ? 'Solicitud aprobada por el refugio'
      : 'Solicitud rechazada por el refugio';
    
    try {
      if (approve) {
        await api.aprobarSolicitud(id, comentario);
      } else {
        await api.rechazarSolicitud(id, comentario);
      }
      fetchData();
    } catch (err) {
      alert(err.message);
    }
  };

  if (loading) {
    return (
      <SafeAreaView style={styles.container} edges={['top']}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color={colors.primary} />
          <Text style={styles.loadingText}>Cargando...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container} edges={['top']}>
      {/* El logout estaba en este encabezado con nav.dispatch({type:'RESET'}),
          que no es una accion valida aqui: noenia nada. Ahora vive en la
          pestaña Perfil, que es donde el usuario lo espera. */}
      <View style={styles.header}>
        <View style={styles.headerText}>
          <Text style={styles.welcome}>Bienvenido,</Text>
          <Text style={styles.refugioName} numberOfLines={1}>
            {user.nombre}
          </Text>
        </View>
        <TouchableOpacity
          style={styles.needsButton}
          onPress={() => nav.navigate('Necesidades')}
          activeOpacity={0.7}
          accessibilityLabel="Necesidades de donación"
        >
          <Ionicons name="heart-outline" size={22} color={colors.primary} />
        </TouchableOpacity>
      </View>

      <View style={styles.tabs}>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'mascotas' && styles.tabActive]}
          onPress={() => setActiveTab('mascotas')}
          activeOpacity={0.7}
        >
          <Ionicons
            name="paw"
            size={18}
            color={activeTab === 'mascotas' ? colors.primary : colors.muted}
          />
          <Text
            style={[styles.tabText, activeTab === 'mascotas' && styles.tabTextActive]}
          >
            Mis mascotas
          </Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'solicitudes' && styles.tabActive]}
          onPress={() => setActiveTab('solicitudes')}
          activeOpacity={0.7}
        >
          <Ionicons
            name="document-text-outline"
            size={18}
            color={activeTab === 'solicitudes' ? colors.primary : colors.muted}
          />
          <Text
            style={[
              styles.tabText,
              activeTab === 'solicitudes' && styles.tabTextActive,
            ]}
          >
            Solicitudes
          </Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'rescates' && styles.tabActive]}
          onPress={() => setActiveTab('rescates')}
          activeOpacity={0.7}
        >
          <Ionicons
            name="alert-circle-outline"
            size={18}
            color={activeTab === 'rescates' ? colors.primary : colors.muted}
          />
          <Text
            style={[styles.tabText, activeTab === 'rescates' && styles.tabTextActive]}
          >
            Rescates
          </Text>
        </TouchableOpacity>
      </View>

      {activeTab === 'mascotas' ? (
        <TouchableOpacity
          style={styles.addButton}
          onPress={() => nav.navigate('MascotaForm')}
          activeOpacity={0.8}
        >
          <Ionicons name="add" size={18} color={colors.onPrimary} />
          <Text style={styles.addButtonText}>Nueva mascota</Text>
        </TouchableOpacity>
      ) : null}

      {/* El error se guardaba pero nunca se mostraba. */}
      {error ? (
        <View style={styles.errorBanner}>
          <Ionicons name="alert-circle" size={18} color={colors.danger} />
          <Text style={styles.errorText}>{error}</Text>
          <TouchableOpacity onPress={fetchData}>
            <Text style={styles.errorRetry}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      ) : null}

      <FlatList
        data={
          activeTab === 'mascotas'
            ? mascotas
            : activeTab === 'solicitudes'
              ? solicitudes
              : reportes
        }
        keyExtractor={(item, index) => index.toString()}
        renderItem={
          activeTab === 'mascotas'
            ? renderMascota
            : activeTab === 'solicitudes'
              ? renderSolicitud
              : renderReporte
        }
        contentContainerStyle={styles.list}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
        }
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Ionicons
              name={
                activeTab === 'mascotas'
                  ? 'paw'
                  : activeTab === 'solicitudes'
                    ? 'document-text'
                    : 'alert-circle-outline'
              }
              size={64}
              color={colors.muted}
            />
            <Text style={styles.emptyText}>
              {activeTab === 'mascotas'
                ? 'No hay mascotas registradas'
                : activeTab === 'solicitudes'
                  ? 'No hay solicitudes'
                  : 'No hay reportes pendientes'}
            </Text>
            <Text style={styles.emptySubtext}>
              {activeTab === 'mascotas'
                ? 'Agrega tu primera mascota'
                : activeTab === 'solicitudes'
                  ? 'Las solicitudes aparecerán aquí'
                  : 'Cuando la comunidad reporte animales, aparecerán aquí'}
            </Text>
          </View>
        }
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    gap: spacing.md,
  },
  loadingText: {
    fontSize: 14,
    color: colors.muted,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: spacing.page,
    paddingVertical: spacing.lg,
    backgroundColor: colors.background,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  headerText: {
    flex: 1,
    marginRight: spacing.md,
  },
  needsButton: {
    width: 40,
    height: 40,
    borderRadius: 20,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: colors.surfaceAlt,
    borderWidth: 1,
    borderColor: colors.border,
  },
  welcome: {
    fontSize: 13,
    color: colors.muted,
  },
  refugioName: {
    fontSize: 20,
    fontWeight: '800',
    color: colors.text,
    marginTop: 2,
  },
  errorBanner: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
    backgroundColor: colors.dangerBg,
    marginHorizontal: spacing.page,
    marginTop: spacing.md,
    padding: spacing.md,
    borderRadius: radius.md,
  },
  errorText: {
    flex: 1,
    fontSize: 13,
    color: colors.danger,
  },
  errorRetry: {
    fontSize: 13,
    fontWeight: '600',
    color: colors.danger,
  },
  tabs: {
    flexDirection: 'row',
    gap: spacing.sm,
    padding: spacing.md,
    backgroundColor: colors.background,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  tab: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: spacing.sm,
    paddingVertical: 10,
    borderRadius: radius.md,
    backgroundColor: colors.surfaceMuted,
  },
  tabActive: {
    backgroundColor: colors.chipActive,
  },
  tabText: {
    fontSize: 13,
    fontWeight: '600',
    color: colors.muted,
  },
  tabTextActive: {
    color: colors.text,
  },
  addButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: spacing.sm,
    backgroundColor: colors.primary,
    borderRadius: radius.md,
    paddingVertical: 12,
    marginHorizontal: spacing.page,
    marginTop: spacing.md,
  },
  addButtonText: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.onPrimary,
  },
  list: {
    padding: spacing.page,
  },
  card: {
    backgroundColor: colors.card,
    borderRadius: radius.md,
    marginBottom: spacing.md,
    padding: spacing.lg,
    borderWidth: 1,
    borderColor: colors.border,
  },
  mascotaRow: {
    flexDirection: 'row',
    gap: spacing.md,
  },
  mascotaThumb: {
    width: 64,
    height: 64,
    borderRadius: radius.sm,
    backgroundColor: colors.surfaceMuted,
  },
  mascotaThumbEmpty: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  cardActions: {
    flexDirection: 'row',
    gap: spacing.md,
    marginTop: spacing.md,
    paddingTop: spacing.md,
    borderTopWidth: 1,
    borderTopColor: colors.border,
  },
  actionButton: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    paddingVertical: 6,
    paddingHorizontal: spacing.md,
  },
  actionText: {
    fontSize: 13,
    fontWeight: '600',
    color: colors.primary,
  },
  actionTextDanger: {
    color: colors.danger,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: spacing.sm,
    gap: spacing.sm,
  },
  solicitudHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: spacing.md,
    gap: spacing.sm,
  },
  solicitudMascota: {
    flex: 1,
  },
  petName: {
    fontSize: 16,
    fontWeight: '700',
    color: colors.text,
  },
  speciesText: {
    fontSize: 13,
    color: colors.muted,
    textTransform: 'capitalize',
  },
  estadoMascotaBadge: {
    backgroundColor: colors.primary,
    borderRadius: radius.pill,
    paddingHorizontal: 10,
    paddingVertical: 4,
  },
  estadoMascotaText: {
    fontSize: 11,
    fontWeight: '600',
    color: colors.onPrimary,
    textTransform: 'capitalize',
  },
  statusBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    paddingHorizontal: 12,
    paddingVertical: 5,
    borderRadius: radius.pill,
  },
  statusDot: {
    width: 8,
    height: 8,
    borderRadius: 4,
  },
  statusText: {
    fontSize: 12,
    fontWeight: '600',
    textTransform: 'capitalize',
  },
  cardDetails: {
    gap: spacing.xs,
  },
  solicitudDetails: {
    gap: spacing.xs,
  },
  detail: {
    fontSize: 13,
    color: colors.muted,
  },
  reporteMeta: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    marginTop: 6,
  },
  solicitudActions: {
    flexDirection: 'row',
    gap: spacing.md,
    marginTop: spacing.md,
    paddingTop: spacing.md,
    borderTopWidth: 1,
    borderTopColor: colors.border,
  },
  approveButton: {
    flex: 1,
    backgroundColor: colors.success,
    borderRadius: radius.sm,
    paddingVertical: 11,
    alignItems: 'center',
  },
  approveText: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.onPrimary,
  },
  rejectButton: {
    flex: 1,
    backgroundColor: colors.danger,
    borderRadius: radius.sm,
    paddingVertical: 11,
    alignItems: 'center',
  },
  rejectText: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.onPrimary,
  },
  emptyContainer: {
    alignItems: 'center',
    gap: spacing.md,
    padding: 40,
  },
  emptyText: {
    fontSize: 18,
    fontWeight: '600',
    color: colors.text,
    textAlign: 'center',
  },
  emptySubtext: {
    fontSize: 14,
    color: colors.muted,
    textAlign: 'center',
  },
});