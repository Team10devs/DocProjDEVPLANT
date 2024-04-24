FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DocProjDEVPLANT/DocProjDEVPLANT.csproj", "DocProjDEVPLANT/"]
RUN dotnet restore "DocProjDEVPLANT/DocProjDEVPLANT.csproj"
COPY . .

ENV ASPNETCORE_ENVIRONMENT=Development

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DocProjDEVPLANT.dll"]
