# Steve's Bot 🤖

This is a Discord bot built with .NET 9, designed to be a full-featured and extensible solution for having my own assistant in my [Discord server](https://discord.stevanfreeborn.com).

## ✨ Features

- **Custom Discord Gateway Client**: Full-featured implementation with:
  - WebSocket connection management
  - Automatic heartbeat handling
  - Session resumption and reconnection logic
  - Event-driven architecture
- **Observability**: Built-in telemetry with OpenTelemetry support
- **Resilient Architecture**: Graceful error handling and automatic recovery
- **Containerized Deployment**: Ready for Docker deployment

## 🚀 Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Discord Bot Token (from [Discord Developer Portal](https://discord.com/developers/applications))

### Configuration

1. Copy the example configuration:

   ```powershell
   Copy-Item src/StevesBot.Worker/appsettings.Example.json src/StevesBot.Worker/appsettings.Development.json
   ```

2. Update `appsettings.Development.json` with your Discord bot credentials:

   ```json
   {
     "DiscordClientOptions": {
       "ApiUrl": "https://discord.com/api/",
       "AppToken": "YOUR_BOT_TOKEN_HERE",
       "Intents": 512
     }
   }
   ```

### Running the Bot

#### Using VS Code Tasks

```powershell
# Build the project
dotnet build src/StevesBot.sln

# Run the bot
dotnet run --project src/StevesBot.Worker
```

#### Using Docker

```powershell
# Build the Docker image
docker build -t steves-bot src/

# Run the container
docker run -d --name steves-bot steves-bot
```

## 🏗️ Architecture

### Project Structure

```txt
src/
├── StevesBot.Worker/             # Main bot application
│   ├── Discord/                  # Discord client implementation
│   │   ├── Gateway/              # WebSocket gateway client
│   │   ├── Rest/                 # REST API client
│   │   └── Shared/               # Common Discord models
│   ├── Handlers/                 # Event handlers
│   ├── Telemetry/                # Observability setup
│   ├── Threading/                # Async utilities
│   └── WebSockets/               # WebSocket abstractions
└── StevesBot.Worker.Tests/       # Comprehensive test suite
```

### Key Components

- **DiscordGatewayClient**: Custom WebSocket client for Discord Gateway API
- **Worker**: Background service that manages the bot lifecycle
- **WebSocket Management**: Custom WebSocket factory and connection handling
- **AsyncLock**: Thread-safe async locking mechanism

## 🔧 Configuration

### Discord Client Options

| Setting                          | Description                             | Required |
|----------------------------------|-----------------------------------------|----------|
| `DiscordClientOptions__ApiUrl`   | Discord API base URL                    | Yes      |
| `DiscordClientOptions__AppToken` | Bot token from Discord Developer Portal | Yes      |
| `DiscordClientOptions__Intents`  | Discord Gateway intents                 | Yes      |

### Telemetry Options

| Setting                    | Description            | Required |
|----------------------------|------------------------|----------|
| `SeqOptions__ServerUrl`    | Seq logging server URL | No       |
| `SeqOptions__ApiKey`       | Seq API key            | No       |
| `SeqOptions__ApiKeyHeader` | Seq API key header     | No       |

## 🧪 Testing

The project includes a comprehensive test suite with both unit and integration tests:

```powershell
# Run all tests
dotnet test src/StevesBot.sln

# Run with coverage
dotnet test src/StevesBot.sln --collect:"XPlat Code Coverage"
```

### Test Coverage

- **Unit Tests**: Extensive coverage of Discord Gateway client, event handling, and utilities
- **Integration Tests**: WebSocket connection and Discord API integration
- **Mock-based Testing**: Isolated testing with proper dependency injection

## 🌟 Discord Gateway Features

Steve's Bot implements a full-featured Discord Gateway client with:

### Connection Management

- Automatic connection establishment
- Session resumption on disconnection
- Graceful reconnection with exponential backoff

### Heartbeat System

- Automatic heartbeat sending
- Heartbeat acknowledgment tracking
- Connection health monitoring

### Event Handling

- Type-safe event deserialization
- Extensible event handler system
- Proper error handling and logging

### Resilience

- Automatic reconnection on connection loss
- Session state preservation
- Proper cleanup on shutdown

## 📊 Observability

The bot includes comprehensive observability features:

- **Structured Logging**: JSON-formatted logs with contextual information
- **OpenTelemetry**: Distributed tracing and metrics
- **Error Tracking**: Detailed error logging and alerting

## 🐳 Deployment

### Docker Deployment

The project includes a multi-stage Dockerfile for optimized production builds:

```dockerfile
# Build stage with .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
# ... build steps ...

# Runtime stage with optimized ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
# ... runtime setup ...
```

### Configuration for Production

Use environment variables or configuration providers for production secrets.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines

- Follow the existing code style and patterns
- Add comprehensive tests for new features
- Update documentation for API changes
- Ensure all tests pass before submitting PR

## 📄 License

This project is licensed under the terms found in the [LICENSE.md](LICENSE.md) file.

## 🔗 Links

- [Discord Developer Portal](https://discord.com/developers/applications)
- [Discord API Documentation](https://discord.com/developers/docs)
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)

---

Built with ❤️ using .NET 9 and a lot of coffee ☕

*Steve's Bot - Helping Stevan and friends since 2025* 🚀
