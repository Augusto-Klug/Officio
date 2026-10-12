FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY src/ ./src/
RUN dotnet restore src/Officio.WebApi/Officio.WebApi.csproj
RUN --mount=type=secret,id=sixlabors_license,required=true \
    dotnet publish src/Officio.WebApi/Officio.WebApi.csproj -c Release --no-restore \
    -p:SixLaborsLicenseFile=/run/secrets/sixlabors_license -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Officio.WebApi.dll"]
