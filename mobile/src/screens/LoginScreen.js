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
import RoleTabs from '../components/RoleTabs';
import FormField from '../components/FormField';
import PrimaryButton from '../components/PrimaryButton';
import { colors } from '../theme/colors';
import { useAuth } from '../context/AuthContext';
import { useNavigation } from '@react-navigation/native';

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export default function LoginScreen({ navigation, route }) {
  const [role, setRole] = useState(route.params?.role ?? 'usuario');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const { login, clearError } = useAuth();
  const nav = useNavigation();

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
      await login(role, email.trim().toLowerCase(), password);
      Alert.alert('Éxito', 'Bienvenido a HuellitasSV');
      if (role === 'usuario') {
        nav.navigate('Catalog');
      } else {
        nav.navigate('RefugioDashboard');
      }
    } catch (err) {
      Alert.alert('Error', err.message);
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

        <RoleTabs value={role} onChange={setRole} />

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
          <TouchableOpacity onPress={() => navigation.navigate('Register', { role })}>
            <Text style={styles.dividerLink}>Regístrate</Text>
          </TouchableOpacity>
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
    paddingHorizontal: 24,
    paddingTop: 40,
    paddingBottom: 64,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 14,
    marginBottom: 28,
  },
  backButton: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: '#F1F5F9',
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
    marginTop: 20,
    gap: 6,
  },
  dividerText: {
    fontSize: 14,
    color: colors.muted,
  },
  dividerLink: {
    fontSize: 14,
    color: colors.primary,
    fontWeight: '600',
  },
});