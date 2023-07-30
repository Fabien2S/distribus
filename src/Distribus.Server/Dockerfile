FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Distribus.Server/Distribus.Server.csproj", "src/Distribus.Server/"]
RUN dotnet restore "src/Distribus.Server/Distribus.Server.csproj"
COPY . .
WORKDIR "/src/src/Distribus.Server"
RUN dotnet build "Distribus.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Distribus.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Distribus.Server.dll"]
