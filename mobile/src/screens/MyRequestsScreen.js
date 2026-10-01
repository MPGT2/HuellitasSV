import { useCallback, useState } from 'react';
import {
  ActivityIndicator,
  FlatList,
  RefreshControl,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useFocusEffect, useNavigation } from '@react-navigation/native';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';
import { api } from '../services/api';

const FILTROS = [
  { valor: 'todas', label: 'Todas' },
  { valor: 'Pendiente', label: 'Pendientes' },
  { valor: 'Aprobada', label: 'Aprobadas' },
  { valor: 'Rechazada', label: 'Rechazadas' },
];

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

export default function MyRequestsScreen() {
  const nav = useNavigation();
  const [solicitudes, setSolicitudes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [activeFilter, setActiveFilter] = useState('todas');
  const [error, setError] = useState(null);

  const fetchSolicitudes = useCallback(async (filtro) => {
    try {
      setError(null);
      const estado = filtro === 'todas' ? undefined : filtro;
      const data = await api.getMisSolicitudes(estado);
      setSolicitudes(data);
    } catch (err) {
      setError(err.message || 'No se pudieron cargar tus solicitudes.');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  // Se recarga al enfocar la pestaña: asi el usuario ve el estado actualizado
  // (Aprobada/Rechazada) apenas el refugio decide, sin reiniciar la app.
  useFocusEffect(
    useCallback(() => {
      fetchSolicitudes(activeFilter);
    }, [activeFilter, fetchSolicitudes]),
  );

  const onRefresh = () => {
    setRefreshing(true);
    fetchSolicitudes(activeFilter);
  };

  // Cada tarjeta abre el detalle de la solicitud (linea de tiempo del proceso,
  // comentario del refugio y datos de contacto).
  const renderSolicitud = ({ item }) => (
    <TouchableOpacity
      style={styles.card}
      activeOpacity={0.8}
      onPress={() => nav.navigate('SolicitudDetail', { solicitud: item })}
    >
      <View style={styles.cardHeader}>
        <View style={styles.mascotaInfo}>
          <Text style={styles.mascotaNombre}>
            {item.mascota?.nombre || 'Mascota'}
          </Text>
          <Text style={styles.mascotaDetails}>
            {item.mascota?.especie} · {item.mascota?.tamano}
          </Text>
        </View>
        <View style={[styles.statusBadge, { backgroundColor: estadoBg(item.estado) }]}>
          <View style={[styles.statusDot, { backgroundColor: estadoColor(item.estado) }]} />
          <Text style={[styles.statusText, { color: estadoColor(item.estado) }]}>
            {item.estado}
          </Text>
        </View>
      </View>

      <View style={styles.cardDetails}>
        <Text style={styles.detail}>
          <Ionicons name="calendar" size={14} color={colors.muted} />
          {new Date(item.fechaSolicitud).toLocaleDateString('es-ES', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
          })}
        </Text>
        {item.comentarioDecision ? (
          <Text style={styles.comentario}>
            <Ionicons
              name="chatbox-ellipses-outline"
              size={14}
              color={colors.muted}
            />
            {item.comentarioDecision}
          </Text>
        ) : null}
      </View>

      <View style={styles.cardFooter}>
        <Text style={styles.cardFooterText}>Ver detalle</Text>
        <Ionicons name="chevron-forward" size={16} color={colors.primary} />
      </View>
    </TouchableOpacity>
  );

  if (loading && solicitudes.length === 0) {
    return (
      <SafeAreaView style={styles.container} edges={['top']}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color={colors.primary} />
          <Text style={styles.loadingText}>Cargando solicitudes...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container} edges={['top']}>
      <View style={styles.topBar}>
        <Text style={styles.topTitle}>Mis solicitudes</Text>
        <Text style={styles.topSubtitle}>
          {solicitudes.length}{' '}
          {solicitudes.length === 1 ? 'solicitud' : 'solicitudes'}
          {activeFilter === 'todas' ? '' : ` · ${activeFilter.toLowerCase()}`}
        </Text>
      </View>

      <View style={styles.filterBar}>
        {FILTROS.map((f) => (
          <TouchableOpacity
            key={f.valor}
            style={[styles.filterChip, activeFilter === f.valor && styles.filterChipActive]}
            onPress={() => setActiveFilter(f.valor)}
            activeOpacity={0.7}
          >
            <Text
              style={[
                styles.filterChipText,
                activeFilter === f.valor && styles.filterChipTextActive,
              ]}
            >
              {f.label}
            </Text>
          </TouchableOpacity>
        ))}
      </View>

      {/* Antes el error se guardaba en el estado pero nunca se renderizaba:
          una falla de red dejaba la lista vacia sin explicar por que. */}
      {error ? (
        <View style={styles.errorBanner}>
          <Ionicons name="alert-circle" size={18} color={colors.danger} />
          <Text style={styles.errorText}>{error}</Text>
          <TouchableOpacity onPress={() => fetchSolicitudes(activeFilter)}>
            <Text style={styles.errorRetry}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      ) : null}

      <FlatList
        data={solicitudes}
        keyExtractor={(item, index) => index.toString()}
        renderItem={renderSolicitud}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Ionicons name="document-text" size={64} color={colors.muted} />
            <Text style={styles.emptyText}>
              {activeFilter === 'todas'
                ? 'No tienes solicitudes'
                : `No hay solicitudes ${activeFilter.toLowerCase()}`}
            </Text>
            <Text style={styles.emptySubtext}>
              {activeFilter === 'todas'
                ? 'Cuando solicites una adopcion, apareceran aqui'
                : 'Proba con otro filtro'}
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
  topBar: {
    paddingHorizontal: spacing.page,
    paddingVertical: spacing.md,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  topTitle: {
    fontSize: 18,
    fontWeight: '800',
    color: colors.text,
  },
  topSubtitle: {
    fontSize: 12,
    color: colors.muted,
    marginTop: 2,
  },
  filterBar: {
    flexDirection: 'row',
    gap: spacing.sm,
    paddingHorizontal: spacing.page,
    paddingVertical: spacing.md,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  filterChip: {
    paddingHorizontal: spacing.lg,
    paddingVertical: 8,
    borderRadius: radius.pill,
    backgroundColor: colors.surfaceMuted,
  },
  filterChipActive: {
    backgroundColor: colors.primary,
  },
  filterChipText: {
    fontSize: 13,
    fontWeight: '600',
    color: colors.text,
  },
  filterChipTextActive: {
    color: colors.onPrimary,
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
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: spacing.sm,
    gap: spacing.sm,
  },
  mascotaInfo: {
    flex: 1,
  },
  mascotaNombre: {
    fontSize: 16,
    fontWeight: '700',
    color: colors.text,
    marginBottom: 2,
  },
  mascotaDetails: {
    fontSize: 13,
    color: colors.muted,
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
    gap: spacing.sm,
  },
  detail: {
    fontSize: 13,
    color: colors.muted,
  },
  comentario: {
    fontSize: 13,
    color: colors.text,
    lineHeight: 19,
    backgroundColor: colors.surfaceAlt,
    padding: spacing.md,
    borderRadius: radius.sm,
  },
  cardFooter: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'flex-end',
    gap: 4,
    marginTop: spacing.sm,
    paddingTop: spacing.sm,
    borderTopWidth: 1,
    borderTopColor: colors.border,
  },
  cardFooterText: {
    fontSize: 13,
    fontWeight: '600',
    color: colors.primary,
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
