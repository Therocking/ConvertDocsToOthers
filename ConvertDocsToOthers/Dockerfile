# Usar imagen Debian en lugar de Alpine
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Instalar dependencias necesarias para PDFiumSharp
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    libx11-6 \
    libx11-dev \
    libxrender1 \
    libxext6 \
    libfontconfig1 \
    libfreetype6 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
ENV DOTNET_VERSION=8.0
COPY ConvertDocsToOthers/ConvertDocsToOthers.csproj ConvertDocsToOthers/
COPY ConvertDocsToOthers.Services/ConvertDocsToOthers.Services.csproj ConvertDocsToOthers.Services/
RUN dotnet restore ConvertDocsToOthers/ConvertDocsToOthers.csproj
COPY . .
WORKDIR /src/ConvertDocsToOthers
RUN dotnet build ConvertDocsToOthers.csproj -c Release -o /app/build

FROM build AS publish
WORKDIR /src/ConvertDocsToOthers
RUN dotnet publish ConvertDocsToOthers.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConvertDocsToOthers.dll"]