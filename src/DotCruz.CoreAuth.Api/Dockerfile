FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER root
RUN apk add --no-cache icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DotCruz.CoreAuth.Api/DotCruz.CoreAuth.Api.csproj", "src/DotCruz.CoreAuth.Api/"]
COPY ["src/DotCruz.CoreAuth.Application/DotCruz.CoreAuth.Application.csproj", "src/DotCruz.CoreAuth.Application/"]
COPY ["src/DotCruz.CoreAuth.Domain/DotCruz.CoreAuth.Domain.csproj", "src/DotCruz.CoreAuth.Domain/"]
COPY ["src/DotCruz.CoreAuth.Infrastructure/DotCruz.CoreAuth.Infrastructure.csproj", "src/DotCruz.CoreAuth.Infrastructure/"]
COPY ["src/DotCruz.CoreAuth.Common/DotCruz.CoreAuth.Common.csproj", "src/DotCruz.CoreAuth.Common/"]
RUN dotnet restore "./src/DotCruz.CoreAuth.Api/DotCruz.CoreAuth.Api.csproj"
COPY . .
WORKDIR "/src/src/DotCruz.CoreAuth.Api"
RUN dotnet build "./DotCruz.CoreAuth.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DotCruz.CoreAuth.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotCruz.CoreAuth.Api.dll"]
