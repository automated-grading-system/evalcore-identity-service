FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY IdentityService.sln ./
COPY src/Identity.Domain/Identity.Domain.csproj src/Identity.Domain/
COPY src/Identity.Application/Identity.Application.csproj src/Identity.Application/
COPY src/Identity.Infrastructure/Identity.Infrastructure.csproj src/Identity.Infrastructure/
COPY src/Identity.Api/Identity.Api.csproj src/Identity.Api/
COPY tests/Identity.Tests/Identity.Tests.csproj tests/Identity.Tests/

RUN dotnet restore IdentityService.sln

COPY . .
RUN dotnet publish src/Identity.Api/Identity.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "Identity.Api.dll"]
