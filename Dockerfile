# ---- Etapa 1: compilar ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia so o .csproj primeiro para o restore ficar em cache
COPY RochaCinema.csproj ./
RUN dotnet restore RochaCinema.csproj

COPY . .
RUN dotnet publish RochaCinema.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Etapa 2: imagem final (so o runtime, muito mais pequena) ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RochaCinema.dll"]
