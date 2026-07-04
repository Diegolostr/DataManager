FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["UnityDataImporter/UnityDataImporter.csproj", "UnityDataImporter/"]
RUN dotnet restore "UnityDataImporter/UnityDataImporter.csproj"
COPY . .
RUN dotnet publish "UnityDataImporter/UnityDataImporter.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UnityDataImporter.dll"]