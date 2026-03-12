FROM node:25-alpine AS web-build
WORKDIR /src/mPanel.Web
RUN npm install -g pnpm
COPY src/mPanel.Web/package.json src/mPanel.Web/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile
COPY src/mPanel.Web/ ./
RUN pnpm generate

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV HUSKY=0
WORKDIR /src
COPY ["src/mPanel.API/mPanel.API.csproj", "src/mPanel.API/"]
RUN dotnet restore "src/mPanel.API/mPanel.API.csproj"
COPY . .
WORKDIR "/src/src/mPanel.API"
RUN dotnet build "./mPanel.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./mPanel.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=web-build /src/mPanel.Web/.output/public ./wwwroot
ENTRYPOINT ["dotnet", "mPanel.API.dll"]
