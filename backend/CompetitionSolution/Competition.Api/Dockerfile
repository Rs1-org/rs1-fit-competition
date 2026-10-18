# ═══════════════════════════════════════════════════════════════
#  Stage 1 – Restore
# ═══════════════════════════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

RUN mkdir -p /root/.nuget/NuGet && \
    printf '<?xml version="1.0" encoding="utf-8"?>\n\
<configuration>\n\
  <packageSources>\n\
    <clear />\n\
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />\n\
  </packageSources>\n\
  <fallbackPackageFolders>\n\
    <clear />\n\
  </fallbackPackageFolders>\n\
</configuration>\n' > /root/.nuget/NuGet/NuGet.Config

COPY *.sln .
COPY Competition.Api/Competition.Api.csproj                         Competition.Api/
COPY Competition.Application/Competition.Application.csproj         Competition.Application/
COPY Competition.Domain/Competition.Domain.csproj                   Competition.Domain/
COPY Competition.Infrastructure/Competition.Infrastructure.csproj   Competition.Infrastructure/

RUN dotnet restore Competition.Api/Competition.Api.csproj

# ═══════════════════════════════════════════════════════════════
#  Stage 2 – Publish API
# ═══════════════════════════════════════════════════════════════
FROM restore AS publish
COPY . .

RUN dotnet publish Competition.Api/Competition.Api.csproj \
    -c Release \
    --no-restore \
    -p:UseAppHost=false \
    -o /app/publish

# ═══════════════════════════════════════════════════════════════
#  Stage 3 – EF Migrator
# ═══════════════════════════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS migrator
WORKDIR /src

RUN dotnet tool install --global dotnet-ef --version 10.0.12
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy NuGet packages and full built source from publish stage
COPY --from=restore /root/.nuget /root/.nuget
COPY --from=publish /src /src

# dotnet-ef needs the built binaries — build in Release so deps.json is in Release folder
RUN dotnet build Competition.Api/Competition.Api.csproj \
    -c Release \
    --no-restore \
    -o /src/efbuild

CMD ["dotnet-ef", "--version"]

# ═══════════════════════════════════════════════════════════════
#  Stage 4 – Runtime
# ═══════════════════════════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ARG APP_REVISION=local
ENV Deployment__Revision=$APP_REVISION

COPY --from=publish /app/publish .

USER $APP_UID

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Competition.Api.dll"]
