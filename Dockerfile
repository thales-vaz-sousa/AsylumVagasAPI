FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia apenas o projeto primeiro para aproveitar o cache das camadas do Docker
COPY AsylumVagasAPI.csproj ./
RUN dotnet restore

# Agora copia o restante dos arquivos
COPY . ./

# Especificamos o arquivo de projeto para evitar o erro MSB1011
RUN dotnet publish "AsylumVagasAPI.csproj" -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Configurações de porta para o Railway
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "AsylumVagasAPI.dll"]