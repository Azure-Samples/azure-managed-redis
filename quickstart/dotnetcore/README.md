# Azure Managed Redis Cache with Microsoft Entra ID Authentication (.NET 10 Sample)

This sample demonstrates how to connect to Azure Managed Redis Cache using Microsoft Entra ID (formerly Azure Active Directory) authentication in a .NET 10 console application. It uses the `Microsoft.Azure.StackExchangeRedis` library for simplified Entra ID integration with automatic token management.

## Features
- Connects securely to Azure Managed Redis Cache using Microsoft Entra ID (no password or connection string required)
- Uses environment variables for configuration
- Demonstrates token acquisition with `DefaultAzureCredential`
- Automatic token refresh handling
- Extended connection timeouts and retry logic for reliability
- Pings the Redis server to verify connectivity
- Performs basic Redis operations (set/get)
- Comprehensive error handling with troubleshooting guidance

## Prerequisites
- .NET 10 SDK ([Download](https://dotnet.microsoft.com/download/dotnet/10.0))
- An Azure Managed Redis Cache instance with Microsoft Entra ID authentication enabled
- Azure CLI for authentication ([Download](https://docs.microsoft.com/cli/azure/install-azure-cli))
- Microsoft Entra ID user or service principal with **Redis Cache Data Owner** or **Redis Cache Data Contributor** role assigned

## Getting Started

1. **Clone and restore packages**
   ```bash
   git clone https://github.com/flang-msft/ConsoleAppdotnetcore.git
   cd ConsoleAppdotnetcore
   dotnet restore
   ```

2. **Configure Redis endpoint**

   Set the `REDIS_ENDPOINT` environment variable to your cache endpoint (`hostname:port`):

   ```powershell
   # PowerShell (session)
   $env:REDIS_ENDPOINT="<yourcachename>.westus3.redis.azure.net:10000"
   
   # PowerShell (permanent)
   [System.Environment]::SetEnvironmentVariable('REDIS_ENDPOINT', '<yourcachename>.westus3.redis.azure.net:10000', 'User')
   ```

3. **Authenticate with Azure**
   
   The sample uses `DefaultAzureCredential` which tries authentication methods in order: Environment Variables ? Managed Identity ? Visual Studio ? Azure CLI ? Azure PowerShell ? Interactive Browser.

   Login via Azure CLI:
   ```bash
   az login
   ```

4. **Build and run**
   ```bash
   dotnet run
   ```

## Expected Output

```
Acquiring Azure credentials...

Connecting to Azure Managed Redis Cache at <yourcachename>.westus3.redis.azure.net:10000 using Microsoft Entra ID authentication...

Configuring Azure authentication token...

Establishing connection to Redis (this may take up to 30 seconds)...

Connection established! IsConnected: True

Successfully acquired database reference from Redis connection.

Attempting to ping Redis server...

Redis ping successful! Response time: 00:00:00.0386109

Setting key 'test:key' with value 'Hello from .NET 10 with Microsoft Entra ID authentication!'...

Set value result: True

Retrieving value for key 'test:key'...

Retrieved value: Hello from .NET 10 with Microsoft Entra ID authentication!

Press any key to exit...
```
## How It Works

1. **Environment Configuration**: Reads the Redis endpoint from `REDIS_ENDPOINT` environment variable
2. **Credential Acquisition**: Uses `DefaultAzureCredential` to automatically acquire a Microsoft Entra ID access token
3. **Connection Configuration**: Configures SSL/TLS encryption, extended timeouts (30s), and connection retry logic (3 attempts)
4. **Token Integration**: `ConfigureForAzureWithTokenCredentialAsync` configures Redis to use Entra ID authentication with automatic token refresh
5. **Redis Operations**: Pings the server to verify connectivity, then performs SET/GET operations

## Configuration Options

The application uses the following connection settings:

| Setting | Value | Description |
|---------|-------|-------------|
| `Ssl` | `true` | Enables SSL/TLS encryption |
| `AbortOnConnectFail` | `false` | Allows retries if initial connection fails |
| `ConnectTimeout` | `30000` ms | Time to wait for connection establishment |
| `SyncTimeout` | `30000` ms | Timeout for synchronous operations |
| `AsyncTimeout` | `30000` ms | Timeout for asynchronous operations |
| `ConnectRetry` | `3` | Number of connection retry attempts |

## Package Dependencies

```xml
<PackageReference Include="Azure.Identity" Version="1.17.1" />
<PackageReference Include="Microsoft.Azure.StackExchangeRedis" Version="3.3.1" />
```

The `Microsoft.Azure.StackExchangeRedis` package automatically includes `StackExchange.Redis` as a transitive dependency.

**Install command:**
```bash
dotnet add package Microsoft.Azure.StackExchangeRedis --version 3.3.1
dotnet add package Azure.Identity --version 1.17.1
```

**Required using statements:**
```csharp
using System;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Azure.StackExchangeRedis;
using StackExchange.Redis;
```

## Troubleshooting

### Connection Timeout
- Verify `REDIS_ENDPOINT` is correct: `<cachename>.<region>.redis.azure.net:<port>`
- Check Redis cache firewall rules allow your IP
- Ensure you're authenticated: `az account show`
- Verify RBAC role assignment (Redis Cache Data Owner/Contributor)

### Authentication Errors
- Confirm Microsoft Entra ID authentication is enabled on your Redis cache
- Verify your principal has one of these roles:
  - **Redis Cache Data Owner** (full access)
  - **Redis Cache Data Contributor** (read/write)
  - **Redis Cache Data Reader** (read-only)

### Missing Environment Variable
If the app exits immediately, ensure `REDIS_ENDPOINT` is set correctly.

## Notes
- This sample targets .NET 10 with implicit usings disabled for clarity
- Requires appropriate Microsoft Entra ID RBAC permissions on the Redis resource
- For production, consider using Managed Identity when running in Azure
- See [Azure Managed Redis Cache documentation](https://learn.microsoft.com/azure/redis/) for more details

## License
This sample is provided as-is for demonstration purposes.
