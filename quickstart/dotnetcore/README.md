# Azure Managed Redis Cache with Microsoft Entra ID Authentication (.NET 9 Sample)

This sample demonstrates how to connect to Azure Managed Redis Cache using Microsoft Entra ID (formerly Azure Active Directory) authentication in a .NET 9 console application. It uses the `Microsoft.Azure.StackExchangeRedis` library for simplified Entra ID integration with automatic token management.

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
- .NET 9 SDK ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- An Azure Managed Redis Cache instance with Microsoft Entra ID authentication enabled
- Azure CLI installed and configured ([Download](https://docs.microsoft.com/cli/azure/install-azure-cli))
- Microsoft Entra ID user or service principal with **Redis Cache Data Owner** or **Redis Cache Data Contributor** role assigned to the Redis cache

## Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/flang-msft/ConsoleAppdotnetcore.git
   cd ConsoleAppdotnetcore
   ```

2. **Restore NuGet packages**
   
   The project requires only one package. Transitive dependencies (`Azure.Identity` and `StackExchange.Redis`) are included automatically:
   ```bash
   dotnet restore
   ```

3. **Set the Redis endpoint environment variable**

   Set the `REDIS_ENDPOINT` environment variable to your Azure Managed Redis Cache endpoint in the format `hostname:port`:

   **Windows (PowerShell):**
   ```powershell
   $env:REDIS_ENDPOINT="<yourcachename>.westus3.redis.azure.net:9000"
   ```

   **Windows (Command Prompt):**
   ```cmd
   set REDIS_ENDPOINT=<yourcachename>.westus3.redis.azure.net:10000
   ```

   **Linux/macOS:**
   ```bash
   export REDIS_ENDPOINT="<yourcachename>.westus3.redis.azure.net:10000"
   ```

   **To set permanently (Windows PowerShell):**
   ```powershell
   [System.Environment]::SetEnvironmentVariable('REDIS_ENDPOINT', '<yourcachename>.westus3.redis.azure.net:10000', 'User')
   ```
   
   > **Note:** Replace `<yourcachename>.westus3.redis.azure.net:10000` with your actual Redis cache endpoint.

4. **Authenticate with Azure**
   
   The sample uses `DefaultAzureCredential`, which supports multiple authentication methods in this order:
   - Environment variables
   - Managed Identity (when running in Azure)
   - Visual Studio
   - Azure CLI
   - Azure PowerShell
   - Interactive browser (if enabled)

   To authenticate using Azure CLI:
   ```bash
   az login
   ```

   Verify you're logged in:
   ```bash
   az account show
   ```

5. **Build the project**
   ```bash
   dotnet build
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

## Expected Output

When successfully connected, you should see output similar to:

```
Acquiring Azure credentials...

Connecting to Azure Managed Redis Cache at <yourcachename>.westus3.redis.azure.net:10000 using Microsoft Entra ID authentication...

Configuring Azure authentication token...

Establishing connection to Redis (this may take up to 30 seconds)...

Connection established! IsConnected: True

Successfully acquired database reference from Redis connection.

Attempting to ping Redis server...

Redis ping successful! Response time: 00:00:00.0123456

Setting key 'test:key' with value 'Hello from .NET 9 with Microsoft Entra ID authentication!'...

Set value result: True

Retrieving value for key 'test:key'...

Retrieved value: Hello from .NET 9 with Microsoft Entra ID authentication!

Press any key to exit...
```
## How It Works

1. **Environment Configuration**: The application reads the Redis endpoint from the `REDIS_ENDPOINT` environment variable
2. **Credential Acquisition**: Creates a `DefaultAzureCredential` to automatically acquire a Microsoft Entra ID access token
3. **Connection Configuration**: Parses the endpoint and configures connection options including:
   - SSL/TLS encryption enabled
   - Extended timeouts (30 seconds) for initial connection
   - Connection retry logic (3 attempts)
4. **Token Integration**: The `ConfigureForAzureWithTokenCredentialAsync` extension method configures the Redis connection to use Entra ID authentication with automatic token refresh
5. **Connection Establishment**: Connects to Redis using the configured options
6. **Database Operations**: 
   - Pings the server to verify connectivity
   - Performs a SET operation to store a value
   - Performs a GET operation to retrieve the value
7. **Error Handling**: Comprehensive try-catch blocks provide detailed error messages and troubleshooting guidance

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

This project requires only one direct NuGet package:

```xml
<PackageReference Include="Microsoft.Azure.StackExchangeRedis" Version="3.3.1" />
```

### Transitive Dependencies (automatically included)

The following packages are automatically included as dependencies:
- **[Azure.Identity](https://www.nuget.org/packages/Azure.Identity)** - Azure SDK authentication library providing `DefaultAzureCredential`
- **[StackExchange.Redis](https://www.nuget.org/packages/StackExchange.Redis)** - High-performance Redis client for .NET

### Installation Command

```bash
dotnet add package Microsoft.Azure.StackExchangeRedis
```

### Required Using Statements

```csharp
using System;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Azure.StackExchangeRedis;
using StackExchange.Redis;
```

## Troubleshooting

### Connection Timeout Errors

If you encounter timeout errors:

1. **Verify the endpoint**: Ensure `REDIS_ENDPOINT` is set correctly with the hostname and port (e.g., `yourCache.region.redis.azure.net:10000`)
   ```bash
   echo $env:REDIS_ENDPOINT  # PowerShell
   echo $REDIS_ENDPOINT       # Linux/macOS
   ```

2. **Check firewall rules**: Verify your IP address is allowed in the Redis cache firewall
   ```bash
   az redis firewall-rules list --name <cache-name> --resource-group <rg-name>
   ```

3. **Verify authentication**: Ensure you're logged in to Azure
   ```bash
   az account show
   ```

4. **Check permissions**: Verify you have the correct RBAC role assigned
   ```bash
   az role assignment list --scope /subscriptions/<subscription-id>/resourceGroups/<rg-name>/providers/Microsoft.Cache/redis/<cache-name> --query "[?principalName=='<your-email>']"
   ```

### Authentication Errors

If you see authentication errors:

1. Ensure Microsoft Entra ID authentication is enabled on your Redis cache
2. Verify you have one of these roles assigned:
   - **Redis Cache Data Owner** (full access)
   - **Redis Cache Data Contributor** (read/write access)
   - **Redis Cache Data Reader** (read-only access)

### Environment Variable Not Set

If the application exits immediately with an error message, set the `REDIS_ENDPOINT` environment variable as shown in the Getting Started section.

## Notes
- This sample targets .NET 9 and uses C# 13 features.
- Make sure your Microsoft Entra ID principal has the correct permissions to access the Redis resource.
- For more information, see the [Azure Managed Redis Cache documentation](https://learn.microsoft.com/azure/redis/).

## License
This sample is provided as-is for demonstration purposes.
