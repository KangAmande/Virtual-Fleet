FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/IngestionAPI/IngestionAPI.csproj", "IngestionAPI/"]
COPY ["src/Shared/", "Shared/"]
RUN dotnet restore "IngestionAPI/IngestionAPI.csproj"
COPY src/ .
WORKDIR "/src/IngestionAPI"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 5000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IngestionAPI.dll"]