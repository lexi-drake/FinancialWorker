# Nuget restore
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY *.sln .
COPY Tests/*.csproj Tests/
COPY Worker/*.csproj Worker/
RUN dotnet restore
COPY . .

# Test
FROM build AS testing
WORKDIR /src/Worker
RUN dotnet build
WORKDIR /src/Tests
RUN dotnet test

# Publish
FROM build AS publish 
WORKDIR /src/Worker
RUN dotnet publish -c Release -o /src/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app
COPY --from=publish /src/publish .
CMD dotnet Worker.dll