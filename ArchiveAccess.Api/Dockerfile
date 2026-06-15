FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ArchiveAccess.Api.csproj ./
RUN dotnet restore ArchiveAccess.Api.csproj

COPY . ./
RUN dotnet publish ArchiveAccess.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

RUN mkdir -p /app/data /app/Files

ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/archive_access.db"
ENV FileStorage__BasePath="/app/Files"

EXPOSE 8080

ENTRYPOINT ["dotnet", "ArchiveAccess.Api.dll"]