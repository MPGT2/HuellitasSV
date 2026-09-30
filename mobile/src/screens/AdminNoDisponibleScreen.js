import { View, Text, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import PrimaryButton from '../components/PrimaryButton';
import { colors } from '../theme/colors';
import { useAuth } from '../context/AuthContext';

// El panel de administracion es web. Sin esto, una cuenta Admin que iniciaba
// sesion caia en el stack Publico y se quedaba sin forma de cerrar sesion.
export default function AdminNoDisponibleScreen() {
  const { logout, userName } = useAuth();

  return (
    <View style={styles.container}>
      <Ionicons name="desktop-outline" size={56} color={colors.muted} />
      <Text style={styles.title}>Panel no disponible en móvil</Text>
      <Text style={styles.body}>
        {userName ? `${userName}, tu` : 'Tu'} cuenta es de administrador. El panel de
        administración solo está disponible en la versión web de HuellitasSV.
      </Text>
      <PrimaryButton title="Cerrar sesión" icon="log-out" onPress={logout} />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
    alignItems: 'center',
    justifyContent: 'center',
    padding: 32,
    gap: 16,
  },
  title: {
    fontSize: 20,
    fontWeight: '800',
    color: colors.text,
    textAlign: 'center',
  },
  body: {
    fontSize: 14,
    color: colors.muted,
    textAlign: 'center',
    lineHeight: 20,
    marginBottom: 12,
  },
});
