import { useEffect, useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  FlatList,
  Image,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';
import { api } from '../services/api';
import { resolveImageUrl } from '../config/env';
import { useAuth } from '../context/AuthContext';
import { useNavigation, useRoute } from '@react-navigation/native';

export default function RefugioProfileScreen() {
  const nav = useNavigation();
  const route = useRoute();
  const refugioParam = route.params?.refugio;
  const idRefugio = refugioParam?.idRefugio ?? route.params?.idRefugio;
  const { isUsuario } = useAuth();

  const [refugio, setRefugio] = useState(refugioParam ?? null);
  const [mascotas, setMascotas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [miEstrellas, setMiEstrellas] = useState(0);
  const [miComentario, setMiComentario] = useState('');
  const [enviandoCalif, setEnviandoCalif] = useState(false);

  useEffect(() => {
    let activo = true;

    const cargar = async () => {
      try {
        const [perfil, pets, miCalif] = await Promise.all([
          idRefugio
            ? api.getRefugioById(idRefugio).catch(() => null)
            : Promise.resolve(null),
          idRefugio
            ? api.getMascotasByRefugio(idRefugio, 'disponible').catch(() => [])
            : Promise.resolve([]),
          idRefugio && isUsuario
            ? api.getMiCalificacion(idRefugio).catch(() => null)
            : Promise.resolve(null),
        ]);
        if (!activo) return;
        if (perfil) setRefugio((prev) => ({ ...prev, ...perfil }));
        setMascotas(Array.isArray(pets) ? pets : []);
        if (miCalif) {
          setMiEstrellas(miCalif.estrellas || 0);
          setMiComentario(miCalif.comentario || '');
        }
      } finally {
        if (activo) setLoading(false);
      }
    };

    cargar();
    return () => {
      activo = false;
    };
  }, [idRefugio, isUsuario]);

  const enviarCalificacion = async () => {
    if (!miEstrellas) {
      Alert.alert('Falta la calificación', 'Elige de 1 a 5 estrellas.');
      return;
    }
    setEnviandoCalif(true);
    try {
      const r = await api.calificarRefugio(idRefugio, miEstrellas, miComentario);
      setRefugio((prev) => ({
        ...prev,
        promedioEstrellas: r.promedioEstrellas,
        totalCalificaciones: r.totalCalificaciones,
      }));
      Alert.alert('Gracias', 'Tu calificación fue registrada.');
    } catch (err) {
      Alert.alert('Error', err.message);
    } finally {
      setEnviandoCalif(false);
    }
  };

  const renderMascota = ({ item }) => (
    <TouchableOpacity
      style={styles.card}
      activeOpacity={0.8}
      onPress={() => nav.navigate('PetDetail', { pet: item })}
    >
      {resolveImageUrl(item.imagenUrl) ? (
        <Image
          source={{ uri: resolveImageUrl(item.imagenUrl) }}
          style={styles.cardImage}
        />
      ) : (
        <View style={[styles.cardImage, styles.cardImageEmpty]}>
          <Ionicons name="paw" size={24} color={colors.muted} />
        </View>
      )}
      <View style={styles.cardInfo}>
        <Text style={styles.cardName}>{item.nombre}</Text>
        <Text style={styles.cardMeta}>
          {item.especie} · {item.tamano} · {item.edadMeses} meses
        </Text>
        <Text style={styles.cardMeta}>{item.estadoSalud}</Text>
      </View>
    </TouchableOpacity>
  );

  const promedio = refugio?.promedioEstrellas;

  const header = (
    <View style={styles.headerBlock}>
      <View style={styles.avatar}>
        <Ionicons name="home" size={28} color={colors.onPrimary} />
      </View>
      <Text style={styles.nombre}>
        {refugio?.nombreOrganizacion || 'Refugio'}
      </Text>

      {(refugio?.municipio || refugio?.departamento) && (
        <View style={styles.locationRow}>
          <Ionicons name="location-outline" size={16} color={colors.muted} />
          <Text style={styles.locationText}>
            {[refugio?.municipio, refugio?.departamento].filter(Boolean).join(', ')}
          </Text>
        </View>
      )}

      <View style={styles.ratingRow}>
        {promedio ? (
          <>
            {[...Array(5)].map((_, i) => (
              <Ionicons
                key={i}
                name={i < Math.round(promedio) ? 'star' : 'star-outline'}
                size={18}
                color={colors.warningStrong}
              />
            ))}
            <Text style={styles.ratingText}>
              {promedio.toFixed(1)}
              {refugio?.totalCalificaciones
                ? ` · ${refugio.totalCalificaciones} calificaciones`
                : ''}
            </Text>
          </>
        ) : (
          <Text style={styles.ratingMuted}>Sin calificaciones</Text>
        )}
      </View>

      {refugio?.contacto ? (
        <View style={styles.contactRow}>
          <Ionicons name="call-outline" size={16} color={colors.primary} />
          <Text style={styles.contactText}>{refugio.contacto}</Text>
        </View>
      ) : null}

      {isUsuario && idRefugio ? (
        <View style={styles.calificarBox}>
          <Text style={styles.calificarTitle}>
            {miEstrellas ? 'Tu calificación' : 'Califica este refugio'}
          </Text>
          <View style={styles.starsPick}>
            {[1, 2, 3, 4, 5].map((n) => (
              <TouchableOpacity key={n} onPress={() => setMiEstrellas(n)}>
                <Ionicons
                  name={n <= miEstrellas ? 'star' : 'star-outline'}
                  size={30}
                  color={colors.warningStrong}
                />
              </TouchableOpacity>
            ))}
          </View>
          <TextInput
            style={styles.calificarInput}
            placeholder="Comentario (opcional)"
            placeholderTextColor={colors.placeholder}
            value={miComentario}
            onChangeText={setMiComentario}
            multiline
          />
          <TouchableOpacity
            style={[styles.calificarBtn, enviandoCalif && styles.calificarBtnDisabled]}
            onPress={enviarCalificacion}
            disabled={enviandoCalif}
          >
            <Text style={styles.calificarBtnText}>
              {enviandoCalif ? 'Enviando…' : 'Enviar calificación'}
            </Text>
          </TouchableOpacity>
        </View>
      ) : null}

      <Text style={styles.sectionTitle}>
        Mascotas disponibles ({mascotas.length})
      </Text>
    </View>
  );

  return (
    <SafeAreaView style={styles.container} edges={['bottom']}>
      <FlatList
        data={mascotas}
        keyExtractor={(item) => item.idMascota.toString()}
        renderItem={renderMascota}
        ListHeaderComponent={header}
        contentContainerStyle={styles.list}
        ListEmptyComponent={
          loading ? (
            <ActivityIndicator
              style={styles.loader}
              size="small"
              color={colors.primary}
            />
          ) : (
            <View style={styles.emptyContainer}>
              <Ionicons name="paw-outline" size={48} color={colors.muted} />
              <Text style={styles.emptyText}>
                Este refugio no tiene mascotas disponibles
              </Text>
            </View>
          )
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
  list: {
    padding: spacing.page,
  },
  headerBlock: {
    alignItems: 'center',
    gap: spacing.sm,
    marginBottom: spacing.xl,
  },
  avatar: {
    width: 64,
    height: 64,
    borderRadius: 32,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: spacing.xs,
  },
  nombre: {
    fontSize: 22,
    fontWeight: '800',
    color: colors.text,
    textAlign: 'center',
  },
  locationRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
  },
  locationText: {
    fontSize: 14,
    color: colors.muted,
  },
  ratingRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 3,
    marginTop: spacing.xs,
  },
  ratingText: {
    fontSize: 13,
    color: colors.muted,
    marginLeft: spacing.xs,
  },
  ratingMuted: {
    fontSize: 13,
    color: colors.muted,
    fontStyle: 'italic',
  },
  contactRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.xs,
    marginTop: spacing.xs,
  },
  contactText: {
    fontSize: 14,
    color: colors.primary,
    fontWeight: '600',
  },
  calificarBox: {
    alignSelf: 'stretch',
    marginTop: spacing.xl,
    padding: spacing.md,
    borderRadius: radius.md,
    borderWidth: 1,
    borderColor: colors.border,
    backgroundColor: colors.surfaceAlt,
    gap: spacing.sm,
  },
  calificarTitle: {
    fontSize: 14,
    fontWeight: '700',
    color: colors.text,
  },
  starsPick: {
    flexDirection: 'row',
    gap: 6,
    justifyContent: 'center',
  },
  calificarInput: {
    backgroundColor: colors.background,
    borderRadius: radius.sm,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.md,
    fontSize: 14,
    color: colors.text,
    minHeight: 44,
  },
  calificarBtn: {
    backgroundColor: colors.primary,
    borderRadius: radius.md,
    paddingVertical: 12,
    alignItems: 'center',
  },
  calificarBtnDisabled: {
    opacity: 0.6,
  },
  calificarBtnText: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.onPrimary,
  },
  sectionTitle: {
    alignSelf: 'flex-start',
    fontSize: 16,
    fontWeight: '700',
    color: colors.text,
    marginTop: spacing.xl,
  },
  card: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
    backgroundColor: colors.card,
    borderRadius: radius.md,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.md,
    marginBottom: spacing.md,
  },
  cardImage: {
    width: 60,
    height: 60,
    borderRadius: radius.sm,
    backgroundColor: colors.surfaceMuted,
  },
  cardImageEmpty: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  cardInfo: {
    flex: 1,
  },
  cardName: {
    fontSize: 16,
    fontWeight: '700',
    color: colors.text,
  },
  cardMeta: {
    fontSize: 13,
    color: colors.muted,
    marginTop: 2,
    textTransform: 'capitalize',
  },
  loader: {
    marginTop: spacing.lg,
  },
  emptyContainer: {
    alignItems: 'center',
    gap: spacing.sm,
    paddingVertical: spacing.xxxl,
  },
  emptyText: {
    fontSize: 14,
    color: colors.muted,
    textAlign: 'center',
  },
});
