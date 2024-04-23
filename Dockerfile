FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app

COPY . .

ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT dotnet run --project containers/DocProjDEVPLANT/DocProjDEVPLANT/DocProjDEVPLANT.csproj