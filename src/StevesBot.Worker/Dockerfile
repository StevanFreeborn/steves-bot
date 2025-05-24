FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . .

FROM base AS publish-stage
RUN dotnet publish -c Release -o dist

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=publish-stage /app/dist ./
ENTRYPOINT ["dotnet", "StevesBot.Worker.dll"]