﻿FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

RUN apt-get update
RUN apt-get install -y libreoffice

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DocProjDEVPLANT/DocProjDEVPLANT.csproj", "DocProjDEVPLANT/"]
RUN dotnet restore "DocProjDEVPLANT/DocProjDEVPLANT.csproj"
COPY . .
WORKDIR "/src/DocProjDEVPLANT"
RUN dotnet build "DocProjDEVPLANT.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DocProjDEVPLANT.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DocProjDEVPLANT.dll"]
