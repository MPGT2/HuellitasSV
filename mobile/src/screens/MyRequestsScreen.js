import { useEffect, useState } from 'react';
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
import { colors } from '../theme/colors';
import { api } from '../services/api';

export default function MyRequestsScreen() {
  const [solicitudes, setSolicitudes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [activeFilter, setActiveFilter] = useState('todas');
  const [error, setError] = useState(null);

  const fetchSolicitudes = async () => {
    try {
      setError(null);
      const estado = activeFilter === 'todas' ? undefined : activeFilter;
      const data = await api.getMisSolicitudes(estado);
      setSolicitudes(data);
    } catch (err) {
      setError(err.message);
      console.error('Error fetching solicitudes:', err);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  const onRefresh = () => {
    setRefreshing(true);
    fetchSolicitudes();
  };

  useEffect(() => {
    fetchSolicitudes();
  }, [activeFilter]);

  const getStatusColor = (estado) => {
    switch (estado) {
      case 'Pendiente': return '#F59E0B';
      case 'Aprobada': return '#10B981';
      case 'Rechazada': return '#EF4444';
      default: return colors.muted;
    }
  };

  // La tarjeta ya muestra todo el detalle (mascota, estado, fecha y el
  // comentario de la decision). Antes era pulsable y navegaba a
  // 'SolicitudDetail', una pantalla que nunca se registro en AppNavigator:
  // React Navigation lanzaba "The action 'NAVIGATE' with payload ... was not
  // handled by any navigator" al tocar cualquier fila.
  const renderSolicitud = ({ item }) => (
    <View style={styles.card}>
      <View style={styles.cardHeader}>
        <View style={styles.mascotaInfo}>
          <Text style={styles.mascotaNombre}>{item.mascota?.nombre || 'Mascota'}</Text>
          <Text style={styles.mascotaDetails}>
            {item.mascota?.especie} · {item.mascota?.tamano}
          </Text>
        </View>
        <View style={styles.statusBadge}>
          <Text style={[styles.statusText, { color: getStatusColor(item.estado) }]}>
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
        {item.comentarioDecision && (
          <Text style={styles.comentario}>
            <Ionicons name="chatbox" size={14} color={colors.muted} />
            {item.comentarioDecision}
          </Text>
        )}
      </View>
    </View>
  );

  if (loading && solicitudes.length === 0) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color={colors.primary} />
          <Text style={styles.loadingText}>Cargando solicitudes...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.filterBar}>
        {['todas', 'Pendiente', 'Aprobada', 'Rechazada'].map((filter) => (
          <TouchableOpacity
            key={filter}
            style={[
              styles.filterChip,
              activeFilter === filter && styles.filterChipActive,
            ]}
            onPress={() => setActiveFilter(filter)}
          >
            <Text style={[
              styles.filterChipText,
              activeFilter === filter && styles.filterChipTextActive,
            ]}>
              {filter}
            </Text>
          </TouchableOpacity>
        ))}
      </View>

      <FlatList
        data={solicitudes}
        keyExtractor={(item) => item.idSolicitud.toString()}
        renderItem={renderSolicitud}
        contentContainerStyle={styles.list}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
        }
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Ionicons name="document-text" size={64} color={colors.muted} />
            <Text style={styles.emptyText}>
              {activeFilter === 'todas' ? 'No tienes solicitudes' : `No hay solicitudes ${activeFilter.toLowerCase()}`}
            </Text>
            <Text style={styles.emptySubtext}>
              {activeFilter === 'todas' ? 'Cuando solicites adopción, aparecerán aquí' : 'Intenta con otro filtro'}
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
    gap: 12,
  },
  loadingText: {
    fontSize: 14,
    color: colors.muted,
  },
  filterBar: {
    flexDirection: 'row',
    gap: 8,
    paddingHorizontal: 16,
    paddingVertical: 12,
    backgroundColor: colors.card,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  filterChip: {
    paddingHorizontal: 14,
    paddingVertical: 8,
    borderRadius: 20,
    backgroundColor: '#F1F5F9',
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
    color: '#FFFFFF',
  },
  list: {
    padding: 16,
  },
  card: {
    backgroundColor: colors.card,
    borderRadius: 12,
    marginBottom: 12,
    padding: 14,
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 2,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
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
  },
  statusBadge: {
    paddingHorizontal: 12,
    paddingVertical: 4,
    borderRadius: 12,
    backgroundColor: '#F1F5F9',
  },
  statusText: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.muted,
    textTransform: 'capitalize',
  },
  cardDetails: {
    gap: 6,
  },
  detail: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    fontSize: 13,
    color: colors.muted,
  },
  comentario: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    gap: 6,
    fontSize: 13,
    color: colors.text,
    lineHeight: 18,
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    gap: 12,
    padding: 32,
  },
  emptyText: {
    fontSize: 18,
    fontWeight: '600',
    color: colors.text,
  },
  emptySubtext: {
    fontSize: 14,
    color: colors.muted,
    textAlign: 'center',
  },
});