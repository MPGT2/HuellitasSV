import { useState } from 'react';
import {
  Alert,
  ActivityIndicator,
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
import { colors } from '../theme/colors';
import { api } from '../services/api';
import { useNavigation, useRoute } from '@react-navigation/native';
import { useAuth } from '../context/AuthContext';
import FormField from '../components/FormField';
import PrimaryButton from '../components/PrimaryButton';

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const PHONE_REGEX = /^[\d\s\-\+\(\)]{8,}$/;

export default function AdoptionRequestScreen() {
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState({
    nombreContacto: '',
    telefonoContacto: '',
    correoContacto: '',
  });
  const [errors, setErrors] = useState({});
  const { user } = useAuth();
  const nav = useNavigation();
  const route = useRoute();
  const pet = route.params?.pet;

  const fetchUserData = async () => {
    try {
      if (user.idUsuario) {
        const data = await api.getUsuarioPerfil();
        setForm(prev => ({
          ...prev,
          nombreContacto: data.nombre,
          correoContacto: data.correo,
        }));
      }
    } catch (err) {
      console.error('Error fetching user data:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (field, value) => {
    setForm(prev => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: undefined }));
    }
  };

  const validate = () => {
    const next = {};
    if (!form.nombreContacto.trim()) next.nombreContacto = 'Ingresa tu nombre de contacto';
    if (!form.telefonoContacto.trim()) next.telefonoContacto = 'Ingresa tu teléfono';
    else if (!PHONE_REGEX.test(form.telefonoContacto.trim()))
      next.telefonoContacto = 'Ingresa un teléfono válido';
    if (!form.correoContacto.trim()) next.correoContacto = 'Ingresa tu correo';
    else if (!EMAIL_REGEX.test(form.correoContacto.trim()))
      next.correoContacto = 'Ingresa un correo válido';
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const handleSubmit = async () => {
    if (!validate()) return;
    if (!pet?.idMascota) {
      Alert.alert('Error', 'No se encontró la mascota');
      return;
    }
    setSubmitting(true);
    try {
      await api.crearSolicitud(
        pet.idMascota,
        form.nombreContacto.trim(),
        form.telefonoContacto.trim(),
        form.correoContacto.trim().toLowerCase()
      );
      Alert.alert(
        '¡Solicitud enviada!',
        'Tu solicitud de adopción ha sido enviada al refugio. Te notificarán cuando respondan.',
        [{ text: 'OK', onPress: () => nav.goBack() }]
      );
    } catch (err) {
      Alert.alert('Error', err.message);
    } finally {
      setSubmitting(false);
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
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        style={styles.keyboardView}
      >
        <ScrollView
          contentContainerStyle={styles.scroll}
          keyboardShouldPersistTaps="handled"
        >
          {pet && (
            <View style={styles.petPreview}>
              <Image
                source={{ uri: pet.imagenUrl || 'https://via.placeholder.com/100' }}
                style={styles.petImage}
              />
              <View style={styles.petInfo}>
                <Text style={styles.petName}>{pet.nombre}</Text>
                <Text style={styles.petDetails}>
                  {pet.especie} · {pet.tamano} · {pet.edadMeses} meses
                </Text>
              </View>
            </View>
          )}

          <View style={styles.section}>
            <Text style={styles.sectionTitle}>Datos de contacto</Text>
            <Text style={styles.sectionSubtitle}>
              El refugio usará esta información para contactarte
            </Text>

            <FormField
              label="NOMBRE DE CONTACTO"
              placeholder="Tu nombre completo"
              value={form.nombreContacto}
              onChangeText={(v) => handleChange('nombreContacto', v)}
              error={errors.nombreContacto}
            />

            <FormField
              label="TELÉFONO DE CONTACTO"
              placeholder="Ej. 2222-0000"
              keyboardType="phone-pad"
              value={form.telefonoContacto}
              onChangeText={(v) => handleChange('telefonoContacto', v)}
              error={errors.telefonoContacto}
            />

            <FormField
              label="CORREO DE CONTACTO"
              placeholder="correo@ejemplo.com"
              keyboardType="email-address"
              autoCapitalize="none"
              autoComplete="email"
              value={form.correoContacto}
              onChangeText={(v) => handleChange('correoContacto', v)}
              error={errors.correoContacto}
            />
          </View>

          <View style={styles.infoBox}>
            <Ionicons name="information-circle" size={20} color={colors.primary} />
            <Text style={styles.infoText}>
              Al enviar la solicitud, el refugio recibirá una notificación y revisará tu perfil.
              Si la mascota ya tiene una adopción aprobada, tu solicitud será rechazada automáticamente.
            </Text>
          </View>

          <PrimaryButton
            title={submitting ? 'Enviando...' : 'Enviar solicitud de adopción'}
            icon={submitting ? undefined : 'heart'}
            onPress={handleSubmit}
            disabled={submitting}
          />

          <TouchableOpacity
            style={styles.cancelButton}
            onPress={() => nav.goBack()}
            disabled={submitting}
          >
            <Text style={styles.cancelText}>Cancelar</Text>
          </TouchableOpacity>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

// Need to import Image
import { Image } from 'react-native';

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
  keyboardView: {
    flex: 1,
  },
  scroll: {
    flexGrow: 1,
    paddingHorizontal: 24,
    paddingTop: 20,
    paddingBottom: 40,
  },
  petPreview: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    padding: 14,
    backgroundColor: colors.card,
    borderRadius: 12,
    marginBottom: 20,
  },
  petImage: {
    width: 70,
    height: 70,
    borderRadius: 10,
  },
  petInfo: {
    flex: 1,
  },
  petName: {
    fontSize: 16,
    fontWeight: '700',
    color: colors.text,
    marginBottom: 4,
  },
  petDetails: {
    fontSize: 13,
    color: colors.muted,
  },
  section: {
    marginBottom: 24,
  },
  sectionTitle: {
    fontSize: 16,
    fontWeight: '700',
    color: colors.text,
    marginBottom: 4,
  },
  sectionSubtitle: {
    fontSize: 13,
    color: colors.muted,
    marginBottom: 16,
  },
  infoBox: {
    flexDirection: 'row',
    gap: 10,
    padding: 14,
    backgroundColor: '#EFF6FF',
    borderRadius: 10,
    marginBottom: 20,
  },
  infoText: {
    fontSize: 13,
    color: '#1E40AF',
    lineHeight: 18,
    flex: 1,
  },
  cancelButton: {
    paddingVertical: 14,
    alignItems: 'center',
    marginTop: 12,
  },
  cancelText: {
    fontSize: 16,
    fontWeight: '600',
    color: colors.muted,
  },
});