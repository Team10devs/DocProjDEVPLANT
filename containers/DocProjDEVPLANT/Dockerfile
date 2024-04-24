FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app

<<<<<<< HEAD:Dockerfile
=======
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DocProjDEVPLANT/DocProjDEVPLANT.csproj", "DocProjDEVPLANT/"]
RUN dotnet restore "DocProjDEVPLANT/DocProjDEVPLANT.csproj"
>>>>>>> c9de7be47c84bf676995ec419ca0a2cb066af4e9:containers/DocProjDEVPLANT/Dockerfile
COPY . .

ENV ASPNETCORE_ENVIRONMENT=Development

<<<<<<< HEAD:Dockerfile
ENTRYPOINT dotnet run --project containers/DocProjDEVPLANT/DocProjDEVPLANT/DocProjDEVPLANT.csproj
=======
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DocProjDEVPLANT.dll"]
>>>>>>> c9de7be47c84bf676995ec419ca0a2cb066af4e9:containers/DocProjDEVPLANT/Dockerfile
