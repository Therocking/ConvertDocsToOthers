# Imagen base de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 8080
EXPOSE 443

# Imagen de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos solo los .csproj para restaurar primero
COPY ConvertDocsToOthers/ConvertDocsToOthers.csproj ConvertDocsToOthers/
COPY ConvertDocsToOthers.Services/ConvertDocsToOthers.Services.csproj ConvertDocsToOthers.Services/

RUN dotnet restore ConvertDocsToOthers/ConvertDocsToOthers.csproj

# Copiamos todo lo demás
COPY . .

# Compilar el proyecto principal
WORKDIR /src/ConvertDocsToOthers
RUN dotnet build ConvertDocsToOthers.csproj -c Release -o /app/build

# Publicar
FROM build AS publish
WORKDIR /src/ConvertDocsToOthers
RUN dotnet publish ConvertDocsToOthers.csproj -c Release -o /app/publish

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConvertDocsToOthers.dll"]
