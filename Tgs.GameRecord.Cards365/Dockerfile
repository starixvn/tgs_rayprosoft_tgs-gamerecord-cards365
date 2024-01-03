#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GameRecord.Cards365/Tgs.GameRecord.Cards365.csproj", "Tgs.GameRecord.Cards365/"]
COPY ["Tgs.GameRecord.Cards365.Common/Tgs.GameRecord.Cards365.Common.csproj", "Tgs.GameRecord.Cards365.Common/"]
COPY ["Tgs.GameRecord.Cards365.Core/Tgs.GameRecord.Cards365.Core.csproj", "Tgs.GameRecord.Cards365.Core/"]
RUN dotnet restore "./Tgs.GameRecord.Cards365/./Tgs.GameRecord.Cards365.csproj"
COPY . .
WORKDIR "/src/Tgs.GameRecord.Cards365"
RUN dotnet build "./Tgs.GameRecord.Cards365.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.GameRecord.Cards365.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameRecord.Cards365.dll"]