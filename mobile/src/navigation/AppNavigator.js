import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { Ionicons } from '@expo/vector-icons';
import { useAuth } from '../context/AuthContext';
import { colors } from '../theme/colors';
import RoleSelectionScreen from '../screens/RoleSelectionScreen';
import RegisterScreen from '../screens/RegisterScreen';
import LoginScreen from '../screens/LoginScreen';
import CatalogScreen from '../screens/CatalogScreen';
import RefugioDashboardScreen from '../screens/RefugioDashboardScreen';
import PetDetailScreen from '../screens/PetDetailScreen';
import ProfileScreen from '../screens/ProfileScreen';
import AdoptionRequestScreen from '../screens/AdoptionRequestScreen';
import MyRequestsScreen from '../screens/MyRequestsScreen';
import SolicitudDetailScreen from '../screens/SolicitudDetailScreen';
import MascotaFormScreen from '../screens/MascotaFormScreen';
import RefugioProfileScreen from '../screens/RefugioProfileScreen';
import ReporteFormScreen from '../screens/ReporteFormScreen';
import MisReportesScreen from '../screens/MisReportesScreen';
import AdminNoDisponibleScreen from '../screens/AdminNoDisponibleScreen';

const Stack = createNativeStackNavigator();
const Tab = createBottomTabNavigator();

// El detalle de la mascota y el formulario de adopcion van por encima de las
// pestañas, asi la barra inferior no compite con el contenido.
const screenOptions = {
  headerStyle: { backgroundColor: colors.background },
  headerTintColor: colors.text,
  headerTitleStyle: { fontWeight: '700' },
  contentStyle: { backgroundColor: colors.background },
};

const tabBarStyle = {
  backgroundColor: colors.background,
  borderTopColor: colors.border,
  height: 84,
  paddingTop: 8,
  paddingBottom: 24,
};

function tabIcon(name, focusedName) {
  return ({ focused, color, size }) => (
    <Ionicons name={focused ? focusedName : name} size={size} color={color} />
  );
}

function PublicStack() {
  return (
    <Stack.Navigator initialRouteName="RoleSelection" screenOptions={{ headerShown: false }}>
      <Stack.Screen name="RoleSelection" component={RoleSelectionScreen} />
      <Stack.Screen name="Register" component={RegisterScreen} />
      <Stack.Screen name="Login" component={LoginScreen} />
    </Stack.Navigator>
  );
}

function UsuarioTabs() {
  return (
    <Tab.Navigator
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.muted,
        tabBarStyle,
        tabBarLabelStyle: { fontSize: 11, fontWeight: '600' },
      }}
    >
      <Tab.Screen
        name="Catalog"
        component={CatalogScreen}
        options={{
          title: 'Catálogo',
          tabBarIcon: tabIcon('paw-outline', 'paw'),
        }}
      />
      <Tab.Screen
        name="MyRequests"
        component={MyRequestsScreen}
        options={{
          title: 'Mis solicitudes',
          tabBarIcon: tabIcon('document-text-outline', 'document-text'),
        }}
      />
      <Tab.Screen
        name="Profile"
        component={ProfileScreen}
        options={{
          title: 'Perfil',
          tabBarIcon: tabIcon('person-outline', 'person'),
        }}
      />
    </Tab.Navigator>
  );
}

function UsuarioStack() {
  return (
    <Stack.Navigator screenOptions={screenOptions}>
      {/* Antes Perfil y MisSolicitudes estaban registradas pero nada las
          abria: no habia forma de llegar al logout ni a las solicitudes. */}
      <Stack.Screen name="UsuarioTabs" component={UsuarioTabs} options={{ headerShown: false }} />
      <Stack.Screen name="PetDetail" component={PetDetailScreen} options={{ title: 'Detalle' }} />
      <Stack.Screen name="AdoptionRequest" component={AdoptionRequestScreen} options={{ title: 'Solicitar adopción' }} />
      <Stack.Screen name="SolicitudDetail" component={SolicitudDetailScreen} options={{ title: 'Detalle de la solicitud' }} />
      <Stack.Screen name="RefugioProfile" component={RefugioProfileScreen} options={{ title: 'Refugio' }} />
      <Stack.Screen name="ReporteForm" component={ReporteFormScreen} options={{ title: 'Reportar animal' }} />
      <Stack.Screen name="MisReportes" component={MisReportesScreen} options={{ title: 'Mis reportes' }} />
    </Stack.Navigator>
  );
}

function RefugioTabs() {
  return (
    <Tab.Navigator
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.muted,
        tabBarStyle,
        tabBarLabelStyle: { fontSize: 11, fontWeight: '600' },
      }}
    >
      <Tab.Screen
        name="RefugioDashboard"
        component={RefugioDashboardScreen}
        options={{
          title: 'Panel',
          tabBarIcon: tabIcon('grid-outline', 'grid'),
        }}
      />
      <Tab.Screen
        name="Profile"
        component={ProfileScreen}
        options={{
          title: 'Perfil',
          tabBarIcon: tabIcon('person-outline', 'person'),
        }}
      />
    </Tab.Navigator>
  );
}

function RefugioStack() {
  return (
    <Stack.Navigator screenOptions={screenOptions}>
      <Stack.Screen name="RefugioTabs" component={RefugioTabs} options={{ headerShown: false }} />
      <Stack.Screen name="PetDetail" component={PetDetailScreen} options={{ title: 'Detalle' }} />
      <Stack.Screen
        name="MascotaForm"
        component={MascotaFormScreen}
        options={{ title: 'Mascota' }}
      />
      <Stack.Screen name="RefugioProfile" component={RefugioProfileScreen} options={{ title: 'Refugio' }} />
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
        <Stack.Screen name="Public" component={PublicStack} />
      ) : isUsuario ? (
        <Stack.Screen name="Usuario" component={UsuarioStack} />
      ) : isRefugio ? (
        <Stack.Screen name="Refugio" component={RefugioStack} />
      ) : (
        <Stack.Screen
          name="AdminNoDisponible"
          component={AdminNoDisponibleScreen}
        />
      )}
    </Stack.Navigator>
  );
}
