# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy solution and project files
COPY ../Looplet.sln .
COPY Looplet.Abstractions/Looplet.Abstractions.csproj Looplet.Abstractions/
COPY Looplet.Hub/Looplet.Hub.csproj Looplet.Hub/

# restore dependencies
RUN dotnet restore Looplet.Hub/Looplet.Hub.csproj

# copy the rest of the sources
COPY . .
WORKDIR /src/Looplet.Hub

# build and publish
RUN dotnet publish -c Release -o /app/publish

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# configure environment and expose port
ENV ASPNETCORE_URLS=http://:80
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80

ENTRYPOINT ["dotnet", "Looplet.Hub.dll"]
