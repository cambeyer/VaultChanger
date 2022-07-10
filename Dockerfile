FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["VaultChanger.csproj", "./"]
RUN dotnet restore "VaultChanger.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "VaultChanger.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VaultChanger.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY token .
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VaultChanger.dll"]
