#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Spedition.FuelAPI/Nuget.config", ""]
COPY ["Spedition.FuelAPI/Spedition.FuelAPI.csproj", "Spedition.FuelAPI/"]
COPY ["Spedition.Fuel.BusinessLayer/Spedition.Fuel.BusinessLayer.csproj", "Spedition.Fuel.BusinessLayer/"]
COPY ["Spedition.Fuel.BFF/Spedition.Fuel.BFF.csproj", "Spedition.Fuel.BFF/"]
COPY ["Spedition.Fuel.Shared/Spedition.Fuel.Shared.csproj", "Spedition.Fuel.Shared/"]
COPY ["Spedition.Fuel.DataAccess/Spedition.Fuel.DataAccess.csproj", "Spedition.Fuel.DataAccess/"]
COPY ["Spedition.Fuel.Dependencies/Spedition.Fuel.Dependencies.csproj", "Spedition.Fuel.Dependencies/"]
RUN dotnet restore "Spedition.FuelAPI/Spedition.FuelAPI.csproj" --configfile Nuget.config
COPY . .
WORKDIR "/src/Spedition.FuelAPI"
RUN dotnet build "Spedition.FuelAPI.csproj" -c Release -o /app/build
RUN rm ./Nuget.config

FROM build AS publish
RUN dotnet publish "Spedition.FuelAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Spedition.FuelAPI.dll"]