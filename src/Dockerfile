FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
WORKDIR /app
COPY ./StevesBot.Worker/StevesBot.Worker.csproj ./
COPY ./Directory.Build.props ./
RUN dotnet restore
COPY ./StevesBot.Worker/. ./

FROM base AS publish-stage
RUN dotnet publish -c Release -o dist

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish-stage /app/dist ./
ENTRYPOINT ["dotnet", "StevesBot.Worker.dll"]