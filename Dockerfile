FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/OhtohsBGList/OhtohsBGList.csproj", "src/OhtohsBGList/"]
RUN dotnet restore "src/OhtohsBGList/OhtohsBGList.csproj"
COPY . .
WORKDIR "/src/src/OhtohsBGList"
RUN dotnet build "OhtohsBGList.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OhtohsBGList.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OhtohsBGList.dll"]
