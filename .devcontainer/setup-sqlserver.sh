#!/usr/bin/env bash
# [DEVOPS] Preparación del Codespace: restaura dependencias, levanta SQL Server 2022
# como contenedor local y deja configurada la cadena de conexión del backend.
set -e

echo "==> Restaurando paquetes NuGet..."
dotnet restore HuellitasSV.API/HuellitasSV.API.csproj

echo "==> Levantando SQL Server 2022 (contenedor 'huellitas-sql')..."
if ! docker ps --format '{{.Names}}' | grep -q '^huellitas-sql$'; then
  docker run -d --name huellitas-sql \
    -e "ACCEPT_EULA=Y" \
    -e "MSSQL_SA_PASSWORD=Huellitas2026!" \
    -e "MSSQL_PID=Developer" \
    -p 1433:1433 \
    mcr.microsoft.com/mssql/server:2022-latest
  echo "Esperando a que SQL Server esté listo..."
  sleep 15
else
  echo "El contenedor ya está en ejecución."
fi

echo "==> Configurando la cadena de conexión (user secrets)..."
cd HuellitasSV.API
dotnet user-secrets init --force
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost,1433;Database=HuellitasSV;User Id=sa;Password=Huellitas2026!;TrustServerCertificate=True;"

echo ""
echo "✔ Entorno listo. Para iniciar la API:"
echo "    cd HuellitasSV.API && dotnet run"
echo "  Swagger: puerto 5299 (pestaña Puertos del Codespace)."