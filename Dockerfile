FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["AbaDeskBack.csproj", "./"]
RUN dotnet restore "./AbaDeskBack.csproj"

COPY . .
RUN dotnet publish "AbaDeskBack.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 10000

ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

RUN mkdir -p /app/wwwroot

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AbaDeskBack.dll"]
