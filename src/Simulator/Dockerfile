FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Simulator/Simulator.csproj", "Simulator/"]
COPY ["src/Shared/", "Shared/"]
RUN dotnet restore "Simulator/Simulator.csproj"
COPY src/ .
WORKDIR "/src/Simulator"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Simulator.dll"]