FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
WORKDIR /app

# solution level
COPY *.sln ./
COPY Directory.Build.props ./

# src level
COPY src/StevesBot.Library/*.csproj src/StevesBot.Library/
COPY src/StevesBot.Worker/*.csproj src/StevesBot.Worker/
COPY src/StevesBot.Webhook/*.csproj src/StevesBot.Webhook/
COPY src/Directory.Build.props src/
COPY src/Directory.Packages.props src/

# test level
COPY tests/StevesBot.Library.Tests/*.csproj tests/StevesBot.Library.Tests/
COPY tests/StevesBot.Worker.Tests/*.csproj tests/StevesBot.Worker.Tests/
COPY tests/StevesBot.Webhook.Tests/*.csproj tests/StevesBot.Webhook.Tests/
COPY tests/Directory.Build.props tests/
COPY tests/Directory.Packages.props tests/

COPY ./src/StevesBot.Worker/StevesBot.Worker.csproj ./
COPY ./Directory.Build.props ./

RUN dotnet restore StevesBot.sln

COPY . .

FROM base AS publish-stage
RUN dotnet publish -c Release -o dist src/StevesBot.Worker/StevesBot.Worker.csproj

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish-stage /app/dist ./
ENTRYPOINT ["dotnet", "StevesBot.Worker.dll"]
