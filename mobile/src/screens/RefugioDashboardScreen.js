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
import { useAuth, useNavigation } from '@react-navigation/native';

export default function RefugioDashboardScreen() {
  const [mascotas, setMascotas] = useState([]);
  const [solicitudes, setSolicitudes] = useState([]);
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
      } else {
        const data = await api.getSolicitudesRefugio();
        setSolicitudes(data);
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

  useEffect(() => {
    fetchData();
  }, [activeTab, user.idRefugio]);

  const renderMascota = ({ item }) => (
    <TouchableOpacity
      style={styles.card}
      onPress={() => nav.navigate('PetDetail', { pet: item })}
      activeOpacity={0.8}
    >
      <View style={styles.cardHeader}>
        <Text style={styles.petName}>{item.nombre}</Text>
        <View style={styles.statusBadge}>
          <Text style={styles.statusText}>{item.estado}</Text>
        </View>
      </View>
      <View style={styles.cardDetails}>
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
    </TouchableOpacity>
  );

  const renderSolicitud = ({ item }) => (
    <TouchableOpacity
      style={[styles.card, styles.solicitudCard]}
      activeOpacity={0.8}
    >
      <View style={styles.solicitudHeader}>
        <View style={styles.solicitudMascota}>
          <Text style={styles.petName}>{item.mascota?.nombre || 'Mascota'}</Text>
          <Text style={styles.speciesText}>{item.mascota?.especie || ''}</Text>
        </View>
        <View style={styles.statusBadge}>
          <Text style={styles.statusText}>{item.estado}</Text>
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
      {item.estado === 'Pendiente' && (
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
      )}
    </TouchableOpacity>
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
      <SafeAreaView style={styles.container}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color={colors.primary} />
          <Text style={styles.loadingText}>Cargando...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <View style={styles.headerInfo}>
          <Text style={styles.welcome}>Bienvenido,</Text>
          <Text style={styles.refugioName}>{user.nombre}</Text>
        </View>
        <TouchableOpacity style={styles.logoutButton} onPress={() => nav.dispatch({ type: 'RESET', payload: { name: 'Public' } })}>
          <Ionicons name="log-out" size={22} color={colors.text} />
        </TouchableOpacity>
      </View>

      <View style={styles.tabs}>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'mascotas' && styles.tabActive]}
          onPress={() => setActiveTab('mascotas')}
        >
          <Ionicons name="paw" size={20} color={activeTab === 'mascotas' ? colors.primary : colors.muted} />
          <Text style={[styles.tabText, activeTab === 'mascotas' && styles.tabTextActive]}>
            Mis Mascotas
          </Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'solicitudes' && styles.tabActive]}
          onPress={() => setActiveTab('solicitudes')}
        >
          <Ionicons name="document-text" size={20} color={activeTab === 'solicitudes' ? colors.primary : colors.muted} />
          <Text style={[styles.tabText, activeTab === 'solicitudes' && styles.tabTextActive]}>
            Solicitudes
          </Text>
        </TouchableOpacity>
      </View>

      <FlatList
        data={activeTab === 'mascotas' ? mascotas : solicitudes}
        keyExtractor={(item) => item.idMascota?.toString() || item.idSolicitud?.toString()}
        renderItem={activeTab === 'mascotas' ? renderMascota : renderSolicitud}
        contentContainerStyle={styles.list}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
        }
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Ionicons name={activeTab === 'mascotas' ? 'paw' : 'document-text'} size={64} color={colors.muted} />
            <Text style={styles.emptyText}>
              {activeTab === 'mascotas' ? 'No hay mascotas registradas' : 'No hay solicitudes'}
            </Text>
            <Text style={styles.emptySubtext}>
              {activeTab === 'mascotas' ? 'Agrega tu primera mascota' : 'Las solicitudes aparecerán aquí'}
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
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 16,
    backgroundColor: colors.card,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  headerInfo: {
    flex: 1,
  },
  welcome: {
    fontSize: 14,
    color: colors.muted,
  },
  refugioName: {
    fontSize: 18,
    fontWeight: '700',
    color: colors.text,
  },
  logoutButton: {
    padding: 8,
  },
  tabs: {
    flexDirection: 'row',
    backgroundColor: colors.card,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  tab: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    paddingVertical: 14,
  },
  tabActive: {
    borderBottomWidth: 2,
    borderBottomColor: colors.primary,
  },
  tabText: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.muted,
  },
  tabTextActive: {
    color: colors.primary,
  },
  list: {
    padding: 16,
  },
  card: {
    backgroundColor: colors.card,
    borderRadius: 12,
    marginBottom: 12,
    padding: 12,
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 2,
  },
  solicitudCard: {
    padding: 14,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  solicitudHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
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
  },
  statusBadge: {
    backgroundColor: colors.primary,
    borderRadius: 12,
    paddingHorizontal: 10,
    paddingVertical: 4,
  },
  statusText: {
    fontSize: 11,
    fontWeight: '600',
    color: '#FFFFFF',
    textTransform: 'capitalize',
  },
  cardDetails: {
    gap: 4,
  },
  solicitudDetails: {
    gap: 4,
  },
  detail: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    fontSize: 13,
    color: colors.muted,
  },
  solicitudActions: {
    flexDirection: 'row',
    gap: 12,
    marginTop: 12,
    paddingTop: 12,
    borderTopWidth: 1,
    borderTopColor: colors.border,
  },
  approveButton: {
    flex: 1,
    backgroundColor: '#10B981',
    borderRadius: 8,
    paddingVertical: 10,
    alignItems: 'center',
  },
  approveText: {
    fontSize: 14,
    fontWeight: '600',
    color: '#FFFFFF',
  },
  rejectButton: {
    flex: 1,
    backgroundColor: '#EF4444',
    borderRadius: 8,
    paddingVertical: 10,
    alignItems: 'center',
  },
  rejectText: {
    fontSize: 14,
    fontWeight: '600',
    color: '#FFFFFF',
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