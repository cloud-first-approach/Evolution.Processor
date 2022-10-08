#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Processor.Api.csproj", "src/Processor.Api/"]
RUN dotnet restore "src/Processor.Api/Processor.Api.csproj"
COPY . "/src/src/Processor.Api"
WORKDIR "/src/src/Processor.Api"
RUN dotnet build "Processor.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Processor.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Processor.Api.dll"]