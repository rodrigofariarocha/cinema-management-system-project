# ---- Etapa 1: compilar ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY CinemaRocha.csproj ./
RUN dotnet restore CinemaRocha.csproj

COPY . .
RUN dotnet publish CinemaRocha.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Etapa 2: imagem final ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CinemaRocha.dll"]
