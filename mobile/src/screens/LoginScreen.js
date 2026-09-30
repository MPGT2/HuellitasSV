import { useState } from 'react';
import {
  Alert,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import FormField from '../components/FormField';
import PrimaryButton from '../components/PrimaryButton';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';
import { useAuth } from '../context/AuthContext';

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export default function LoginScreen({ navigation }) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const { login, clearError } = useAuth();

  const validate = () => {
    const next = {};
    if (!email.trim()) next.email = 'Ingresa tu correo electrónico';
    else if (!EMAIL_REGEX.test(email.trim())) next.email = 'Ingresa un correo válido';
    if (!password) next.password = 'Ingresa tu contraseña';
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const handleLogin = async () => {
    if (!validate()) return;
    setLoading(true);
    clearError();
    try {
      // No se navega a mano: al guardarse la sesion, AppNavigator cambia del
      // stack Publico al del rol y se encarga del destino.
      await login(email.trim().toLowerCase(), password);
    } catch (err) {
      Alert.alert('No se pudo iniciar sesión', err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <ScrollView
        contentContainerStyle={styles.scroll}
        keyboardShouldPersistTaps="handled"
        indicatorStyle="black"
      >
        <View style={styles.header}>
          <TouchableOpacity
            style={styles.backButton}
            onPress={() => navigation.goBack()}
          >
            <Ionicons name="chevron-back" size={20} color={colors.text} />
          </TouchableOpacity>
          <View style={styles.headerText}>
            <Text style={styles.title}>Iniciar sesión</Text>
            <Text style={styles.subtitle}>
              Accede a tu cuenta de HuellitasSV
            </Text>
          </View>
        </View>

        <FormField
          label="CORREO ELECTRÓNICO"
          placeholder="correo@ejemplo.com"
          keyboardType="email-address"
          autoCapitalize="none"
          autoComplete="email"
          value={email}
          onChangeText={setEmail}
          error={errors.email}
        />

        <FormField
          label="CONTRASEÑA"
          placeholder="Tu contraseña"
          secureTextEntry
          value={password}
          onChangeText={setPassword}
          error={errors.password}
        />

        <PrimaryButton
          title={loading ? 'Iniciando sesión...' : 'Iniciar sesión'}
          icon={loading ? undefined : 'log-in'}
          onPress={handleLogin}
          disabled={loading}
        />

        <View style={styles.divider}>
          <Text style={styles.dividerText}>¿No tienes cuenta?</Text>
          <TouchableOpacity onPress={() => navigation.navigate('Register')}>
            <Text style={styles.dividerLink}>Regístrate</Text>
          </TouchableOpacity>
        </View>

        <View style={styles.banner}>
          <Ionicons name="business-outline" size={18} color={colors.warningText} />
          <Text style={styles.bannerText}>
            ¿Tenés un refugio? También podés entrar desde acá cuando tu cuenta
            sea aprobada.
          </Text>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  scroll: {
    flexGrow: 1,
    paddingHorizontal: spacing.page,
    paddingTop: spacing.pageTop,
    paddingBottom: spacing.pageBottom,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.lg,
    marginBottom: 28,
  },
  backButton: {
    width: 40,
    height: 40,
    borderRadius: radius.circle,
    backgroundColor: colors.surfaceMuted,
    alignItems: 'center',
    justifyContent: 'center',
  },
  headerText: {
    flex: 1,
  },
  title: {
    fontSize: 20,
    fontWeight: '800',
    color: colors.text,
  },
  subtitle: {
    fontSize: 13,
    color: colors.muted,
    marginTop: 2,
  },
  divider: {
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    marginTop: spacing.xxl,
    gap: spacing.xs,
  },
  dividerText: {
    fontSize: 13,
    color: colors.muted,
    lineHeight: 20,
  },
  dividerLink: {
    fontSize: 13,
    color: colors.primary,
    fontWeight: '600',
  },
  banner: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
    backgroundColor: colors.warningBg,
    borderRadius: radius.md,
    padding: spacing.lg,
    marginTop: spacing.xxxl,
  },
  bannerText: {
    flex: 1,
    fontSize: 13,
    color: colors.warningText,
    lineHeight: 19,
  },
});