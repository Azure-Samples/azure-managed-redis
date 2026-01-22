# ASP.NET Core Razor Pages: Azure Managed Redis Cache with Entra ID

This project demonstrates how to securely connect an ASP.NET Core Razor Pages application to an Azure Managed Redis Cache using Microsoft Entra ID authentication and the `StackExchange.Redis` client library.

## Prerequisites

- .NET 9 SDK
- Azure CLI (for local development authentication)
- Access to an Azure Redis Cache instance with Entra ID authentication enabled

## Key Features

The application demonstrates Redis connectivity through three main sections:

1. **Connection Section**
   - Uses `DefaultAzureCredential` from the `Azure.Identity` package for secure Entra ID authentication.
   - Connects to Azure Redis Cache using the `Microsoft.Azure.StackExchangeRedis` extension.
   - Includes robust error handling and null reference protection.

2. **Ping Section**
   - Verifies Redis connectivity using the `PingAsync` method.
   - Displays round-trip latency in milliseconds.

3. **Set/Get Section**
   - Demonstrates basic Redis operations by setting and retrieving key/value pairs.
   - Sets three sample keys ("Key1", "Key2", "Key3") with values ("Alpha", "Bravo", "Charlie").

## Dependencies

The project uses the following NuGet packages:

- [StackExchange.Redis](https://www.nuget.org/packages/StackExchange.Redis) - Redis client library
- [Azure.Identity](https://www.nuget.org/packages/Azure.Identity) - Azure authentication
- [Microsoft.Azure.StackExchangeRedis](https://www.nuget.org/packages/Microsoft.Azure.StackExchangeRedis) - Azure Redis extensions

## Installation

1. Clone this repository.
2. Navigate to the project directory.
3. Install dependencies:

    ```bash
    dotnet add package StackExchange.Redis
    dotnet add package Azure.Identity
    dotnet add package Microsoft.Azure.StackExchangeRedis
    ```

4. Restore packages:

    ```bash
    dotnet restore
    ```

## Configuration

### appsettings.Development.json

Configure your Redis endpoint in the configuration file:

```json
{
  "DetailedErrors": true,
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Redis": {
    "Endpoint": "<your-redis-host>:<port>"
  }
}
```
**Example:**
```json
{
  "Redis": {
    "Endpoint": "contosoak15.westus3.redis.azure.net:10000"
  }
}
```

### Authentication Setup

For local development, authenticate with Azure CLI:

```sh
az login
```

For production deployments, ensure your application has appropriate Azure role assignments to access the Redis cache.

## How It Works

### Code Structure

The main logic is implemented in `Pages/Index.cshtml.cs`:

- **Dependency Injection**: Receives `ILogger` and `IConfiguration` through constructor injection.
- **Configuration Reading**: Retrieves Redis endpoint from configuration with null safety.
- **Azure Authentication**: Uses `DefaultAzureCredential` for seamless authentication.
- **Connection Management**: Establishes secure connection using Azure Entra ID.
- **Error Handling**: Comprehensive exception handling with user-friendly error messages.

### Key Implementation Details

- **Null Safety**: The code includes proper null checks to prevent runtime exceptions.
- **Async Operations**: All Redis operations use async/await for better performance.
- **Configuration Validation**: Throws meaningful exceptions if Redis endpoint is not configured.
- **Resource Management**: Properly manages `ConnectionMultiplexer` instances.

## Error Handling

The application includes robust error handling:

- Configuration validation with clear error messages.
- Null reference protection for Redis operations.
- Exception catching with user-friendly error display.
- Graceful degradation when Redis is unavailable.

## Running the Application

1. Ensure you're authenticated with Azure CLI (`az login`).
2. Update the configuration with your Redis endpoint.
3. Run the application:

    ```bash
    dotnet run
    ```

4. Navigate to `https://localhost:5001` (or the configured port).

## Troubleshooting

### Common Issues

1. **"Redis: Endpoint not configured"**: Ensure the `Redis:Endpoint` value is properly set in `appsettings.Development.json`.

2. **Authentication failures**: Verify you're logged in with `az login` and have access to the Redis cache.

3. **Connection timeouts**: Check network connectivity and Redis cache availability.

4. **"Failed to get database instance"**: This typically indicates a connection issue - check your endpoint configuration and authentication.

### Required Azure Permissions

Ensure your user or managed identity has the following roles on the Redis cache:
- Redis Cache Contributor (for management operations)
- Redis Cache Data Access (for data operations)

---

For more details, see the implementation in `Pages/Index.cshtml.cs` and the UI rendering in `Pages/Index.cshtml`.

## Project Structure

```
??? Pages/
?   ??? Index.cshtml          # Main page UI with Redis results display
?   ??? Index.cshtml.cs       # Page model with Redis connection logic
??? appsettings.Development.json  # Configuration file
??? README.md                 # This file
