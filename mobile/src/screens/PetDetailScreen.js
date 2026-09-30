import { useEffect, useState } from 'react';
import {
  Alert,
  ActivityIndicator,
  Image,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme/colors';
import { api } from '../services/api';
import { resolveImageUrl } from '../config/env';
import { useNavigation, useRoute } from '@react-navigation/native';
import { useAuth } from '../context/AuthContext';

export default function PetDetailScreen() {
  const [pet, setPet] = useState(null);
  const [loading, setLoading] = useState(true);
  const [adopting, setAdopting] = useState(false);
  const { isUsuario, user } = useAuth();
  const nav = useNavigation();
  const route = useRoute();

  const petFromRoute = route.params?.pet;

  const fetchPet = async () => {
    if (petFromRoute) {
      setPet(petFromRoute);
      setLoading(false);
      // El objeto que llega por ruta puede venir incompleto (por ejemplo desde
      // el panel del refugio, sin contacto ni calificacion). Se pide el detalle
      // para completarlo; si falla, la pantalla ya muestra lo que tenia.
      if (petFromRoute.idMascota) {
        try {
          const detalle = await api.getMascotaById(petFromRoute.idMascota);
          if (detalle) setPet(detalle);
        } catch (err) {
          console.warn('No se pudo completar el detalle:', err.message);
        }
      }
      return;
    }
    // If we don't have pet from route, we'd need an ID to fetch
    setLoading(false);
  };

  const formatFecha = (fecha) => {
    if (!fecha) return '—';
    const d = new Date(fecha);
    return Number.isNaN(d.getTime()) ? '—' : d.toLocaleDateString('es-ES');
  };

  useEffect(() => {
    fetchPet();
  }, []);

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color={colors.primary} />
        </View>
      </SafeAreaView>
    );
  }

  if (!pet) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.emptyContainer}>
          <Ionicons name="alert-circle" size={64} color={colors.muted} />
          <Text style={styles.emptyText}>Mascota no encontrada</Text>
        </View>
      </SafeAreaView>
    );
  }

  const handleAdoptar = () => {
    if (!isUsuario) {
      Alert.alert('Solo usuarios', 'Solo los usuarios pueden solicitar adopción');
      return;
    }
    nav.navigate('AdoptionRequest', { pet });
  };

  return (
    <SafeAreaView style={styles.container}>
      <ScrollView
        contentContainerStyle={styles.scroll}
        showsVerticalScrollIndicator={false}
      >
        <View style={styles.imageContainer}>
          <Image source={{ uri: resolveImageUrl(pet.imagenUrl) || 'https://via.placeholder.com/400x300' }} style={styles.image} />
          <View style={styles.imageOverlay}>
            <View style={styles.statusBadge}>
              <Text style={styles.statusText}>{pet.estado}</Text>
            </View>
          </View>
        </View>

        <View style={styles.content}>
          <View style={styles.header}>
            <Text style={styles.name}>{pet.nombre}</Text>
            <View style={styles.speciesBadge}>
              <Ionicons name={pet.especie === 'perro' ? 'paw' : pet.especie === 'gato' ? 'cat' : 'ellipse'} size={16} color={colors.onPrimary} />
              <Text style={styles.speciesText}>{pet.especie}</Text>
            </View>
          </View>

          <TouchableOpacity
            style={styles.refugioInfo}
            activeOpacity={0.7}
            disabled={!pet.refugio}
            onPress={() => nav.navigate('RefugioProfile', { refugio: pet.refugio })}
          >
            <Ionicons name="home" size={20} color={colors.primary} />
            <View style={styles.refugioDetails}>
              <Text style={styles.refugioName}>{pet.refugio?.nombreOrganizacion || 'Refugio'}</Text>
              <Text style={styles.refugioLocation}>
                {pet.refugio?.municipio}, {pet.refugio?.departamento}
              </Text>
            </View>
            <Ionicons name="chevron-forward" size={18} color={colors.muted} />
          </TouchableOpacity>

          <View style={styles.section}>
            <Text style={styles.sectionTitle}>Información</Text>
            <View style={styles.infoGrid}>
              <View style={styles.infoItem}>
                <Ionicons name="resize" size={20} color={colors.primary} />
                <Text style={styles.infoLabel}>Tamaño</Text>
                <Text style={styles.infoValue}>{pet.tamano}</Text>
              </View>
              <View style={styles.infoItem}>
                <Ionicons name="time" size={20} color={colors.primary} />
                <Text style={styles.infoLabel}>Edad</Text>
                <Text style={styles.infoValue}>{pet.edadMeses} meses</Text>
              </View>
              <View style={styles.infoItem}>
                <Ionicons name="medical" size={20} color={colors.primary} />
                <Text style={styles.infoLabel}>Salud</Text>
                <Text style={styles.infoValue}>{pet.estadoSalud}</Text>
              </View>
              <View style={styles.infoItem}>
                <Ionicons name="calendar" size={20} color={colors.primary} />
                <Text style={styles.infoLabel}>Registrado</Text>
                <Text style={styles.infoValue}>
                  {formatFecha(pet.fechaRegistro)}
                </Text>
              </View>
            </View>
          </View>

          {pet.refugio?.promedioEstrellas && (
            <View style={styles.section}>
              <Text style={styles.sectionTitle}>Calificación del refugio</Text>
              <View style={styles.rating}>
                <Text style={styles.ratingValue}>{pet.refugio.promedioEstrellas.toFixed(1)}</Text>
                <View style={styles.stars}>
                  {[...Array(5)].map((_, i) => (
                    <Ionicons
                      key={i}
                      name={i < Math.round(pet.refugio.promedioEstrellas) ? 'star' : 'star-outline'}
                      size={20}
                      color={colors.warningStrong}
                    />
                  ))}
                </View>
                <Text style={styles.ratingCount}>
                  ({pet.refugio.totalCalificaciones} calificaciones)
                </Text>
              </View>
            </View>
          )}

          {isUsuario && pet.estado === 'disponible' && (
            <TouchableOpacity
              style={[styles.adoptButton, adopting && styles.adoptButtonDisabled]}
              onPress={handleAdoptar}
              disabled={adopting}
            >
              <Ionicons name="heart" size={20} color={colors.onPrimary} />
              <Text style={styles.adoptText}>
                {adopting ? 'Procesando...' : 'Solicitar Adopción'}
              </Text>
            </TouchableOpacity>
          )}

          {!isUsuario && (
            <View style={styles.loginPrompt}>
              <Text style={styles.loginPromptText}>Inicia sesión como usuario para solicitar adopción</Text>
            </View>
          )}

          {pet.refugio?.contacto && (
            <View style={styles.section}>
              <Text style={styles.sectionTitle}>Contacto del refugio</Text>
              <Text style={styles.contactText}>{pet.refugio.contacto}</Text>
            </View>
          )}
        </View>
      </ScrollView>
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
  scroll: {
    flexGrow: 1,
  },
  imageContainer: {
    height: 280,
    position: 'relative',
  },
  image: {
    width: '100%',
    height: '100%',
  },
  imageOverlay: {
    position: 'absolute',
    top: 16,
    right: 16,
  },
  statusBadge: {
    backgroundColor: 'rgba(0,0,0,0.6)',
    borderRadius: 16,
    paddingHorizontal: 14,
    paddingVertical: 6,
  },
  statusText: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.onPrimary,
    textTransform: 'capitalize',
  },
  content: {
    padding: 16,
    gap: 20,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    gap: 12,
  },
  name: {
    fontSize: 28,
    fontWeight: '800',
    color: colors.text,
    flex: 1,
  },
  speciesBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    backgroundColor: colors.primary,
    borderRadius: 16,
    paddingHorizontal: 14,
    paddingVertical: 6,
  },
  speciesText: {
    fontSize: 13,
    fontWeight: '600',
    color: colors.onPrimary,
    textTransform: 'capitalize',
  },
  refugioInfo: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    padding: 14,
    backgroundColor: colors.chipActive,
    borderRadius: 12,
  },
  refugioDetails: {
    flex: 1,
  },
  refugioName: {
    fontSize: 16,
    fontWeight: '700',
    color: colors.text,
  },
  refugioLocation: {
    fontSize: 13,
    color: colors.muted,
  },
  section: {
    gap: 12,
  },
  sectionTitle: {
    fontSize: 16,
    fontWeight: '700',
    color: colors.text,
  },
  infoGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 12,
  },
  infoItem: {
    flex: 1,
    minWidth: '45%',
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
    padding: 12,
    backgroundColor: colors.surfaceMuted,
    borderRadius: 10,
  },
  infoLabel: {
    fontSize: 11,
    fontWeight: '600',
    color: colors.muted,
    letterSpacing: 0.5,
    marginBottom: 2,
  },
  infoValue: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.text,
  },
  rating: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  ratingValue: {
    fontSize: 28,
    fontWeight: '800',
    color: colors.primary,
  },
  stars: {
    flexDirection: 'row',
    gap: 2,
  },
  ratingCount: {
    fontSize: 14,
    color: colors.muted,
  },
  adoptButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    backgroundColor: colors.primary,
    borderRadius: 12,
    paddingVertical: 16,
    marginTop: 8,
  },
  adoptButtonDisabled: {
    opacity: 0.6,
  },
  adoptText: {
    fontSize: 16,
    fontWeight: '700',
    color: colors.onPrimary,
  },
  loginPrompt: {
    padding: 16,
    backgroundColor: colors.chipActive,
    borderRadius: 12,
    alignItems: 'center',
  },
  loginPromptText: {
    fontSize: 14,
    color: colors.warningTextStrong,
    textAlign: 'center',
  },
  contactText: {
    fontSize: 15,
    color: colors.text,
    fontWeight: '500',
  },
});