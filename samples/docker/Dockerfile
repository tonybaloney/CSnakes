FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ExampleApp/ExampleApp.csproj", "ExampleApp/"]
RUN dotnet restore "ExampleApp/ExampleApp.csproj"
COPY . .
WORKDIR "/src/ExampleApp"
RUN dotnet build "./ExampleApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ExampleApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN dotnet tool install --global CSnakes.Stage --version 1.1.0-dev.250723-1846
ENV PATH="/root/.dotnet/tools:${PATH}"
RUN CSnakes.Stage --python 3.12

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /root/.config/CSnakes /home/app/.config/CSnakes
ENTRYPOINT ["dotnet", "ExampleApp.dll"]