# Multi-stage build para otimizar tamanho da imagem
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar arquivo de projeto e restaurar dependências
COPY ["CreditConsult/CreditConsult.csproj", "CreditConsult/"]
RUN dotnet restore "CreditConsult/CreditConsult.csproj"

# Copiar todo o código e compilar
COPY . .
WORKDIR "/src/CreditConsult"
RUN dotnet build "CreditConsult.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicar aplicação
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CreditConsult.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Imagem final runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

# Copiar apenas os arquivos publicados
COPY --from=publish /app/publish .

# Criar usuário não-root para segurança
RUN addgroup --system --gid 1000 dotnetuser && \
    adduser --system --uid 1000 --ingroup dotnetuser dotnetuser && \
    chown -R dotnetuser:dotnetuser /app

USER dotnetuser

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "CreditConsult.dll"]
