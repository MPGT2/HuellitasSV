import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { useAuth } from '../context/AuthContext';
import RoleSelectionScreen from '../screens/RoleSelectionScreen';
import RegisterScreen from '../screens/RegisterScreen';
import LoginScreen from '../screens/LoginScreen';
import CatalogScreen from '../screens/CatalogScreen';
import RefugioDashboardScreen from '../screens/RefugioDashboardScreen';
import PetDetailScreen from '../screens/PetDetailScreen';
import ProfileScreen from '../screens/ProfileScreen';
import AdoptionRequestScreen from '../screens/AdoptionRequestScreen';
import MyRequestsScreen from '../screens/MyRequestsScreen';

const Stack = createNativeStackNavigator();

function PublicStack() {
  return (
    <Stack.Navigator initialRouteName="RoleSelection" screenOptions={{ headerShown: false }}>
      <Stack.Screen name="RoleSelection" component={RoleSelectionScreen} />
      <Stack.Screen name="Register" component={RegisterScreen} />
      <Stack.Screen name="Login" component={LoginScreen} />
    </Stack.Navigator>
  );
}

function UsuarioStack() {
  return (
    <Stack.Navigator screenOptions={{ headerStyle: { backgroundColor: '#FFFFFF' } }}>
      <Stack.Screen name="Catalog" component={CatalogScreen} options={{ title: 'Catálogo' }} />
      <Stack.Screen name="PetDetail" component={PetDetailScreen} options={{ title: 'Detalle' }} />
      <Stack.Screen name="AdoptionRequest" component={AdoptionRequestScreen} options={{ title: 'Solicitar Adopción' }} />
      <Stack.Screen name="MyRequests" component={MyRequestsScreen} options={{ title: 'Mis Solicitudes' }} />
      <Stack.Screen name="Profile" component={ProfileScreen} options={{ title: 'Mi Perfil' }} />
    </Stack.Navigator>
  );
}

function RefugioStack() {
  return (
    <Stack.Navigator screenOptions={{ headerStyle: { backgroundColor: '#FFFFFF' } }}>
      <Stack.Screen name="RefugioDashboard" component={RefugioDashboardScreen} options={{ title: 'Panel Refugio' }} />
      <Stack.Screen name="PetDetail" component={PetDetailScreen} options={{ title: 'Detalle' }} />
      <Stack.Screen name="Profile" component={ProfileScreen} options={{ title: 'Mi Perfil' }} />
    </Stack.Navigator>
  );
}

export default function AppNavigator() {
  const { isLoggedIn, isUsuario, isRefugio, loading } = useAuth();

  if (loading) {
    return null; // Or a loading screen
  }

  return (
    <Stack.Navigator screenOptions={{ headerShown: false }}>
      {!isLoggedIn ? (
        <>
          <Stack.Screen name="Public" component={PublicStack} />
        </>
      ) : isUsuario ? (
        <>
          <Stack.Screen name="Usuario" component={UsuarioStack} />
        </>
      ) : isRefugio ? (
        <>
          <Stack.Screen name="Refugio" component={RefugioStack} />
        </>
      ) : (
        <>
          <Stack.Screen name="Public" component={PublicStack} />
        </>
      )}
    </Stack.Navigator>
  );
}