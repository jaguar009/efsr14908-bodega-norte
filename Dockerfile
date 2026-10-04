FROM node:22-bookworm-slim AS frontend-build
WORKDIR /src
RUN corepack enable && corepack prepare pnpm@11.19.0 --activate
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile
COPY index.html vite.config.js ./
COPY src ./src
COPY public/favicon.svg ./public/favicon.svg
COPY .env.visualstudio ./.env.visualstudio
RUN pnpm run build:visualstudio

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src
COPY backend/BodegaNorte.Api/BodegaNorte.Api.csproj backend/BodegaNorte.Api/
RUN dotnet restore backend/BodegaNorte.Api/BodegaNorte.Api.csproj
COPY backend/BodegaNorte.Api backend/BodegaNorte.Api
COPY --from=frontend-build /src/backend/BodegaNorte.Api/wwwroot backend/BodegaNorte.Api/wwwroot
RUN dotnet publish backend/BodegaNorte.Api/BodegaNorte.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=backend-build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000
ENTRYPOINT ["dotnet", "BodegaNorte.Api.dll"]
