FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Dependencias del sistema necesarias para PDFiumSharp
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

# Etapa de restauración (cachable si los .csproj no cambian)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src
COPY ConvertDocsToOthers/*.csproj ConvertDocsToOthers/
COPY ConvertDocsToOthers.Services/*.csproj ConvertDocsToOthers.Services/
RUN dotnet restore ConvertDocsToOthers/ConvertDocsToOthers.csproj

FROM restore AS build
COPY . .
WORKDIR /src/ConvertDocsToOthers
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConvertDocsToOthers.dll"]
