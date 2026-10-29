FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY personapi-dotnet.sln .
COPY personapi-dotnet/personapi-dotnet.csproj personapi-dotnet/
RUN dotnet restore personapi-dotnet/personapi-dotnet.csproj
COPY . .
RUN dotnet publish personapi-dotnet/personapi-dotnet.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:5142
EXPOSE 5142
ENTRYPOINT ["dotnet", "personapi-dotnet.dll"]
