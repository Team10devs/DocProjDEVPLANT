FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "DocProjDEVPLANT/containers/DocProjDEVPLANT/DocProjDEVPLANT.csproj" --disable-parallel
RUN dotnet publish "DocProjDEVPLANT/containers/DocProjDEVPLANT/DocProjDEVPLANT.csproj" -c release -o /app --no-restore


# server
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
COPY --from=build /app ./

EXPOSE 8080

ENTRYPOINT ["dotnet", "DocProjDEVPLANT.dll"]