#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/ConductorSharp.Proxy/ConductorSharp.Proxy.csproj", "src/ConductorSharp.Proxy/"]
RUN dotnet restore "src/ConductorSharp.Proxy/ConductorSharp.Proxy.csproj"
COPY . .
WORKDIR "/src/src/ConductorSharp.Proxy"
RUN dotnet build "ConductorSharp.Proxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConductorSharp.Proxy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConductorSharp.Proxy.dll"]