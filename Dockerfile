FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY containers/DocProjDEVPLANT/DocProjDEVPLANT.csproj ./DocProjDEVPLANT/

RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet build

# Publish
RUN dotnet publish DocProjDEVPLANT.csproj -c Release -o /app/out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

COPY docker-entrypoint.sh /usr/bin/docker-entrypoint.sh
RUN chmod +x /usr/bin/docker-entrypoint.sh
ENTRYPOINT ["docker-entrypoint.sh"]

LABEL org.opencontainers.image.source=https://github.com/Team10devs/DocProjDEVPLANT
