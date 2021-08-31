FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Youtube-GameOnlineServer/Youtube-GameOnlineServer.csproj", "Youtube-GameOnlineServer/"]
RUN dotnet restore "Youtube-GameOnlineServer/Youtube-GameOnlineServer.csproj"
COPY . .
WORKDIR "/src/Youtube-GameOnlineServer"
RUN dotnet build "Youtube-GameOnlineServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Youtube-GameOnlineServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Youtube-GameOnlineServer.dll"]
