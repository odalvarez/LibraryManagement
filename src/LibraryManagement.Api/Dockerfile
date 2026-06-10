FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/LibraryManagement.Api/LibraryManagement.Api.csproj", "src/LibraryManagement.Api/"]
COPY ["src/LibraryManagement.Application/LibraryManagement.Application.csproj", "src/LibraryManagement.Application/"]
COPY ["src/LibraryManagement.Infrastructure/LibraryManagement.Infrastructure.csproj", "src/LibraryManagement.Infrastructure/"]
COPY ["src/LibraryManagement.Domain/LibraryManagement.Domain.csproj", "src/LibraryManagement.Domain/"]
RUN dotnet restore "src/LibraryManagement.Api/LibraryManagement.Api.csproj"
COPY . .
RUN dotnet publish "src/LibraryManagement.Api/LibraryManagement.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LibraryManagement.Api.dll"]
