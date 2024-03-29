#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

ENV HUSKY 0

WORKDIR /src
COPY ["examples/ConductorSharp.NoApi/ConductorSharp.NoApi.csproj", "examples/ConductorSharp.NoApi/"]
COPY ["src/ConductorSharp.Engine/ConductorSharp.Engine.csproj", "src/ConductorSharp.Engine/"]
COPY ["src/ConductorSharp.Client/ConductorSharp.Client.csproj", "src/ConductorSharp.Client/"]
RUN dotnet restore "examples/ConductorSharp.NoApi/ConductorSharp.NoApi.csproj"
COPY . .
WORKDIR "/src/examples/ConductorSharp.NoApi"
RUN dotnet build "ConductorSharp.NoApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConductorSharp.NoApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConductorSharp.NoApi.dll"]
