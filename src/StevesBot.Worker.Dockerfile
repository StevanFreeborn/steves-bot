FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
WORKDIR /app

# solution level
COPY src/StevesBot.sln src/
COPY src/Directory.Build.props src/

# src level
COPY src/src/StevesBot.Library/*.csproj src/src/StevesBot.Library/
COPY src/src/StevesBot.Worker/*.csproj src/src/StevesBot.Worker/
COPY src/src/StevesBot.Webhook/*.csproj src/src/StevesBot.Webhook/
COPY src/src/Directory.Build.props src/src/
COPY src/src/Directory.Packages.props src/src/

# test level
COPY src/tests/StevesBot.Library.Tests/*.csproj src/tests/StevesBot.Library.Tests/
COPY src/tests/StevesBot.Worker.Tests/*.csproj src/tests/StevesBot.Worker.Tests/
COPY src/tests/StevesBot.Webhook.Tests/*.csproj src/tests/StevesBot.Webhook.Tests/
COPY src/tests/Directory.Build.props src/tests/
COPY src/tests/Directory.Packages.props src/tests/

RUN dotnet restore src/StevesBot.sln

COPY . .

FROM base AS publish-stage
RUN dotnet publish -c Release -o dist src/src/StevesBot.Worker/StevesBot.Worker.csproj

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish-stage /app/dist ./
ENTRYPOINT ["dotnet", "StevesBot.Worker.dll"]
