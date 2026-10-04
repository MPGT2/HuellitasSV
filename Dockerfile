FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY HuellitasSV.API/HuellitasSV.API.csproj HuellitasSV.API/
RUN dotnet restore HuellitasSV.API/HuellitasSV.API.csproj
COPY HuellitasSV.API/ HuellitasSV.API/
RUN dotnet publish HuellitasSV.API/HuellitasSV.API.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV PORT=10000
EXPOSE 10000
CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT} exec dotnet HuellitasSV.API.dll"]