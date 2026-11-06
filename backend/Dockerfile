# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/Cadastro.Domain/Cadastro.Domain.csproj src/Cadastro.Domain/
COPY src/Cadastro.Application/Cadastro.Application.csproj src/Cadastro.Application/
COPY src/Cadastro.Infrastructure/Cadastro.Infrastructure.csproj src/Cadastro.Infrastructure/
COPY src/Cadastro.Api/Cadastro.Api.csproj src/Cadastro.Api/
RUN dotnet restore src/Cadastro.Api/Cadastro.Api.csproj

COPY src/ src/
RUN dotnet publish src/Cadastro.Api/Cadastro.Api.csproj -c Release -o /app/publish --no-restore

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ConnectionStrings__Default="Data Source=/app/data/cadastro.db"

RUN mkdir -p /app/data && chown -R $APP_UID /app/data
USER $APP_UID

COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Cadastro.Api.dll"]
