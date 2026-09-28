#!/usr/bin/env bash
# Preparación del Codespace para HuellitasSV (BD en MonsterASP — sin SQL local).
set -e

echo "==> Restaurando paquetes NuGet..."
dotnet restore HuellitasSV.API/HuellitasSV.API.csproj

cd HuellitasSV.API
dotnet user-secrets init --force

if [ -n "${MONSTERASP_CONNECTION_STRING:-}" ]; then
  echo "==> Configurando ConnectionStrings desde MONSTERASP_CONNECTION_STRING..."
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "$MONSTERASP_CONNECTION_STRING"
  echo "✔ Cadena de conexión configurada."
else
  echo ""
  echo "⚠ No se encontró MONSTERASP_CONNECTION_STRING."
  echo "  Configure la BD de MonsterASP antes de ejecutar la API:"
  echo ""
  echo "  export MONSTERASP_CONNECTION_STRING='Server=....monsterasp.net;Database=...;User Id=...;Password=...;TrustServerCertificate=True;Encrypt=True;'"
  echo "  dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"\$MONSTERASP_CONNECTION_STRING\""
  echo ""
  echo "  O copie appsettings.Local.json.example → appsettings.Local.json y complete los valores."
  echo ""
fi

echo ""
echo "✔ Entorno listo. Para iniciar la API:"
echo "    cd HuellitasSV.API && dotnet run"
echo "  Swagger: puerto 5299 (pestaña Puertos del Codespace)."
