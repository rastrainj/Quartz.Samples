#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Quartz.Samples.AspNetCore/Quartz.Samples.AspNetCore.csproj", "Quartz.Samples.AspNetCore/"]
RUN dotnet restore "Quartz.Samples.AspNetCore/Quartz.Samples.AspNetCore.csproj"
COPY . .
WORKDIR "/src/Quartz.Samples.AspNetCore"
RUN dotnet build "Quartz.Samples.AspNetCore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Quartz.Samples.AspNetCore.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quartz.Samples.AspNetCore.dll"]