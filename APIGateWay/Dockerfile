# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Just copy csproj
COPY APIGateWay.csproj ./
RUN dotnet restore APIGateWay.csproj

# Copy all files and publish
COPY . ./
RUN dotnet publish APIGateWay.csproj -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /out ./
ENTRYPOINT ["dotnet", "APIGateWay.dll"]
 