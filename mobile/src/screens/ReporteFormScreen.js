import { useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  Image,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import * as ImagePicker from 'expo-image-picker';
import * as Location from 'expo-location';
import FormField from '../components/FormField';
import PrimaryButton from '../components/PrimaryButton';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';
import { api } from '../services/api';

export default function ReporteFormScreen({ navigation }) {
  const [descripcion, setDescripcion] = useState('');
  const [foto, setFoto] = useState(null);
  const [ubicacion, setUbicacion] = useState(null);
  const [direccion, setDireccion] = useState('');
  const [obteniendoUbicacion, setObteniendoUbicacion] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [errors, setErrors] = useState({});

  const elegirFoto = async () => {
    const { status } = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (status !== 'granted') {
      Alert.alert('Permiso requerido', 'Necesitamos acceso a tus fotos para adjuntar la del animal.');
      return;
    }
    const res = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ['images'],
      allowsEditing: true,
      quality: 0.6,
    });
    if (!res.canceled) {
      setFoto(res.assets[0]);
      setErrors((prev) => ({ ...prev, foto: undefined }));
    }
  };

  const obtenerUbicacion = async () => {
    setObteniendoUbicacion(true);
    try {
      const { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== 'granted') {
        Alert.alert('Permiso requerido', 'Necesitamos tu ubicación para alertar a los refugios cercanos.');
        return;
      }
      const pos = await Location.getCurrentPositionAsync({
        accuracy: Location.Accuracy.Balanced,
      });
      setUbicacion({
        latitud: pos.coords.latitude,
        longitud: pos.coords.longitude,
      });
      setErrors((prev) => ({ ...prev, ubicacion: undefined }));

      // Best-effort: dirección legible solo para mostrar, no se envía al backend.
      try {
        const [dir] = await Location.reverseGeocodeAsync({
          latitude: pos.coords.latitude,
          longitude: pos.coords.longitude,
        });
        if (dir) {
          setDireccion(
            [dir.street, dir.district, dir.city, dir.region].filter(Boolean).join(', '),
          );
        }
      } catch {
        // Si falla el geocoding, se muestran solo las coordenadas.
      }
    } catch (err) {
      Alert.alert('Error', 'No se pudo obtener tu ubicación. Intenta de nuevo.');
    } finally {
      setObteniendoUbicacion(false);
    }
  };

  const handleSubmit = async () => {
    const next = {};
    if (!descripcion.trim()) next.descripcion = 'Describe al animal y su situación';
    if (!foto) next.foto = 'Adjunta una foto del animal';
    if (!ubicacion) next.ubicacion = 'Marca la ubicación del animal';
    setErrors(next);
    if (Object.keys(next).length > 0) return;

    setSubmitting(true);
    try {
      await api.crearReporte(
        {
          descripcion: descripcion.trim(),
          latitud: ubicacion.latitud,
          longitud: ubicacion.longitud,
        },
        {
          uri: foto.uri,
          name: foto.fileName || 'reporte.jpg',
          type: foto.mimeType || 'image/jpeg',
        },
      );
      Alert.alert(
        '¡Reporte enviado!',
        'Avisamos a los refugios cercanos (5 km) para que puedan atender el rescate.',
        [{ text: 'OK', onPress: () => navigation.goBack() }],
      );
    } catch (err) {
      Alert.alert('Error', err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <SafeAreaView style={styles.container} edges={['bottom']}>
      <KeyboardAvoidingView
        style={styles.flex}
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
      >
        <ScrollView
          contentContainerStyle={styles.scroll}
          keyboardShouldPersistTaps="handled"
          indicatorStyle="black"
        >
          <View style={styles.banner}>
            <Ionicons name="megaphone-outline" size={18} color={colors.warningText} />
            <Text style={styles.bannerText}>
              Reporta un perro o gato en situación de calle. Los refugios a 5 km reciben una alerta.
            </Text>
          </View>

          <FormField
            label="¿QUÉ VISTE?"
            placeholder="Ej. Perro pequeño sin collar en la acera"
            value={descripcion}
            onChangeText={(v) => {
              setDescripcion(v);
              if (errors.descripcion) setErrors((p) => ({ ...p, descripcion: undefined }));
            }}
            error={errors.descripcion}
            multiline
            numberOfLines={4}
            style={styles.textArea}
          />

          <Text style={styles.label}>FOTO DEL ANIMAL</Text>
          {foto ? (
            <View style={styles.previewWrap}>
              <Image source={{ uri: foto.uri }} style={styles.preview} />
              <TouchableOpacity style={styles.previewAction} onPress={elegirFoto} activeOpacity={0.7}>
                <Ionicons name="camera" size={16} color={colors.onPrimary} />
                <Text style={styles.previewActionText}>Cambiar</Text>
              </TouchableOpacity>
            </View>
          ) : (
            <TouchableOpacity style={styles.uploadBox} onPress={elegirFoto} activeOpacity={0.7}>
              <Ionicons name="camera-outline" size={26} color={colors.placeholder} />
              <Text style={styles.uploadText}>Toca para adjuntar una foto</Text>
            </TouchableOpacity>
          )}
          {errors.foto ? <Text style={styles.error}>{errors.foto}</Text> : null}

          <Text style={styles.label}>UBICACIÓN</Text>
          <TouchableOpacity
            style={styles.locationBox}
            onPress={obtenerUbicacion}
            activeOpacity={0.7}
            disabled={obteniendoUbicacion}
          >
            {obteniendoUbicacion ? (
              <ActivityIndicator size="small" color={colors.primary} />
            ) : (
              <Ionicons
                name={ubicacion ? 'location' : 'location-outline'}
                size={22}
                color={ubicacion ? colors.success : colors.muted}
              />
            )}
            <View style={styles.locationInfo}>
              <Text style={styles.locationTitle}>
                {ubicacion ? 'Ubicación marcada' : 'Usar mi ubicación actual'}
              </Text>
              {ubicacion ? (
                <Text style={styles.locationCoord}>
                  {direccion || `${ubicacion.latitud.toFixed(5)}, ${ubicacion.longitud.toFixed(5)}`}
                </Text>
              ) : null}
            </View>
            <Ionicons name="refresh" size={18} color={colors.muted} />
          </TouchableOpacity>
          {errors.ubicacion ? <Text style={styles.error}>{errors.ubicacion}</Text> : null}

          <PrimaryButton
            title={submitting ? 'Enviando...' : 'Enviar reporte'}
            icon={submitting ? undefined : 'megaphone'}
            onPress={handleSubmit}
            disabled={submitting}
          />
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  flex: {
    flex: 1,
  },
  scroll: {
    flexGrow: 1,
    paddingHorizontal: spacing.page,
    paddingTop: spacing.xl,
    paddingBottom: spacing.pageBottom,
  },
  banner: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
    backgroundColor: colors.warningBg,
    borderRadius: radius.md,
    padding: spacing.lg,
    marginBottom: spacing.xl,
  },
  bannerText: {
    flex: 1,
    fontSize: 13,
    color: colors.warningText,
    lineHeight: 19,
  },
  label: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.text,
    letterSpacing: 1,
    marginBottom: spacing.sm,
  },
  textArea: {
    marginBottom: spacing.xl,
  },
  uploadBox: {
    backgroundColor: colors.surfaceAlt,
    borderRadius: radius.md,
    borderWidth: 1.5,
    borderStyle: 'dashed',
    borderColor: colors.borderStrong,
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: 30,
    marginBottom: spacing.sm,
    gap: spacing.sm,
  },
  uploadText: {
    fontSize: 13,
    color: colors.muted,
  },
  previewWrap: {
    marginBottom: spacing.sm,
    borderRadius: radius.md,
    overflow: 'hidden',
  },
  preview: {
    width: '100%',
    height: 200,
    backgroundColor: colors.surfaceMuted,
  },
  previewAction: {
    position: 'absolute',
    bottom: spacing.md,
    right: spacing.md,
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    backgroundColor: 'rgba(0,0,0,0.6)',
    borderRadius: radius.pill,
    paddingHorizontal: spacing.md,
    paddingVertical: 6,
  },
  previewActionText: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.onPrimary,
  },
  locationBox: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
    backgroundColor: colors.surfaceAlt,
    borderRadius: radius.md,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.md,
    marginBottom: spacing.sm,
  },
  locationInfo: {
    flex: 1,
  },
  locationTitle: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.text,
  },
  locationCoord: {
    fontSize: 12,
    color: colors.muted,
    marginTop: 2,
  },
  error: {
    fontSize: 12,
    color: colors.danger,
    marginBottom: spacing.md,
  },
});
