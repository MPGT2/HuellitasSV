import { StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme/colors';

export default function PrimaryButton({ title, icon, color, onPress, disabled = false }) {
  return (
    <TouchableOpacity
      style={[
        styles.button,
        color ? { backgroundColor: color } : null,
        // Sin esto el boton sigue activo durante el envio y el usuario puede
        // disparar el login o el registro varias veces.
        disabled ? styles.disabled : null,
      ]}
      onPress={disabled ? undefined : onPress}
      activeOpacity={0.8}
      disabled={disabled}
      accessibilityState={{ disabled }}
    >
      {icon ? <Ionicons name={icon} size={18} color="#FFFFFF" /> : null}
      <Text style={styles.title}>{title}</Text>
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  button: {
    backgroundColor: colors.primary,
    borderRadius: 12,
    padding: 14,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
  },
  title: {
    color: '#FFFFFF',
    textAlign: 'center',
    fontSize: 16,
    fontWeight: '600',
  },
  disabled: {
    opacity: 0.6,
  },
});
