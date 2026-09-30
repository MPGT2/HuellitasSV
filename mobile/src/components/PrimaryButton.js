import { StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme/colors';

export default function PrimaryButton({ title, icon, color, onPress }) {
  return (
    <TouchableOpacity
      style={[styles.button, color ? { backgroundColor: color } : null]}
      onPress={onPress}
      activeOpacity={0.8}
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
});
