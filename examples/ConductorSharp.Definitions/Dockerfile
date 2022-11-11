#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

ENV HUSKY 0

COPY ["examples/ConductorSharp.Definitions/ConductorSharp.Definitions.csproj", "examples/ConductorSharp.Definitions/"]
COPY ["src/ConductorSharp.Patterns/ConductorSharp.Patterns.csproj", "src/ConductorSharp.Patterns/"]
COPY ["src/ConductorSharp.Engine/ConductorSharp.Engine.csproj", "src/ConductorSharp.Engine/"]
COPY ["src/ConductorSharp.Client/ConductorSharp.Client.csproj", "src/ConductorSharp.Client/"]
RUN dotnet restore "examples/ConductorSharp.Definitions/ConductorSharp.Definitions.csproj"
COPY . .
WORKDIR "/src/examples/ConductorSharp.Definitions"
RUN dotnet build "ConductorSharp.Definitions.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConductorSharp.Definitions.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConductorSharp.Definitions.dll"]