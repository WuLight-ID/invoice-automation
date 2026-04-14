FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ia-backend/ia-backend.csproj ia-backend/
RUN dotnet restore ia-backend/ia-backend.csproj

COPY . .
RUN dotnet publish ia-backend/ia-backend.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app .
EXPOSE 10000

ENTRYPOINT ["dotnet", "ia-backend.dll"]