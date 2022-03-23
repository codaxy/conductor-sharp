#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["examples/ConductorSharp.ApiEnabled/ConductorSharp.ApiEnabled.csproj", "examples/ConductorSharp.ApiEnabled/"]
COPY ["src/ConductorSharp.Engine/ConductorSharp.Engine.csproj", "src/ConductorSharp.Engine/"]
COPY ["src/ConductorSharp.Client/ConductorSharp.Client.csproj", "src/ConductorSharp.Client/"]
RUN dotnet restore "examples/ConductorSharp.ApiEnabled/ConductorSharp.ApiEnabled.csproj"
COPY . .
WORKDIR "/src/examples/ConductorSharp.ApiEnabled"
RUN dotnet build "ConductorSharp.ApiEnabled.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConductorSharp.ApiEnabled.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConductorSharp.ApiEnabled.dll"]