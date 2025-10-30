FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Usar la imagen SDK para compilar el proyecto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["./", "TODO.WebApi/"]
RUN dotnet restore "TODO.WebApi/TODO.WebApi.csproj"
COPY .
WORKDIR "/src/TODO.WebApi"
RUN dotnet build "TODO.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TODO.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TODO.WebApi.dll"]
