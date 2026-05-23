# ═══════════════════════════════════════════════════════════════
#  Stage 1 – Restore
# ═══════════════════════════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY *.sln .
COPY Competition.Api/Competition.Api.csproj                         Competition.Api/
COPY Competition.Application/Competition.Application.csproj         Competition.Application/
COPY Competition.Domain/Competition.Domain.csproj                   Competition.Domain/
COPY Competition.Infrastructure/Competition.Infrastructure.csproj   Competition.Infrastructure/

RUN dotnet restore

# ═══════════════════════════════════════════════════════════════
#  Stage 2 – Publish
# ═══════════════════════════════════════════════════════════════
FROM restore AS publish
COPY . .

RUN dotnet publish Competition.Api/Competition.Api.csproj \
    -c Release \
    -o /app/publish

# ═══════════════════════════════════════════════════════════════
#  Stage 3 – Runtime
# ═══════════════════════════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Competition.Api.dll"]