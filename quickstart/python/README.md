# Azure Managed Redis Python Quickstart

This project demonstrates how to securely connect to **Azure Managed Redis** using Python with Azure Entra ID authentication.

## Overview

This quickstart covers:

1. Connecting to Azure Managed Redis using Azure Entra ID authentication
2. Verifying connection with a PING operation
3. Performing basic SET and GET operations

## Prerequisites

- Python 3.9+
- Azure account with access to Azure Managed Redis
- Azure CLI installed and authenticated (`az login`)
- Azure Managed Redis instance with Azure Entra ID authentication enabled
- Redis Data Contributor role assigned to your user

## Quick Start

### 1. Clone or Download Project

```bash
git clone <repository-url>
cd python-quickstart

# Or create a new directory and copy the files
mkdir azure-redis-quickstart
cd azure-redis-quickstart
# Copy quickstart-amr.py and requirements.txt to this directory
```

### 2. Install Dependencies

```bash
pip install -r requirements.txt
```

Or install packages individually:

```bash
pip install redis azure-identity redis-entraid
```

### 3. Configure Connection

Set the `REDIS_ENDPOINT` environment variable with your Azure Managed Redis endpoint:

**PowerShell:**
```powershell
$env:REDIS_ENDPOINT = "your-cache-name.eastus.redis.azure.net:10000"
```

**Bash:**
```bash
export REDIS_ENDPOINT="your-cache-name.eastus.redis.azure.net:10000"
```

**Note:** Azure Managed Redis uses port 10000, which differs from Azure Cache for Redis (port 6380). The endpoint is masked in logs for security.

### 4. Run the Script

```bash
python quickstart-amr.py
```

## Implementation Details

### Establishing Connection

The script establishes a secure connection using Azure Entra ID authentication via `DefaultAzureCredential`:

```python
from azure.identity import DefaultAzureCredential
from redis_entraid.cred_provider import create_from_default_azure_credential

# Create credential provider using DefaultAzureCredential for Azure Entra ID authentication
credential_provider = create_from_default_azure_credential(
     ("https://redis.azure.com/.default",),)

# Create a Redis client with Azure Entra ID authentication
r = redis.Redis(host=redis_host, 
                port=redis_port, 
                ssl=True, 
                decode_responses=True, 
                credential_provider=credential_provider,
                socket_timeout=10,
                socket_connect_timeout=10
                )
```

Configuration parameters:

- `ssl=True`: Enables TLS encryption for all communication
- `decode_responses=True`: Automatically decodes byte responses to strings
- `socket_timeout` / `socket_connect_timeout`: 10-second timeout for reliability

### Connection Verification

The script tests connectivity by sending a PING command:

```python
# Test connection 
result = r.ping()
print("Ping returned : " + str(result))
```

Returns `True` if the connection and authentication are successful.

### SET and GET Operations

Basic Redis operations for storing and retrieving data:

```python
# SET operation - Store a message
result = r.set("Message", "Hello, The cache is working with Python!")
print("SET Message succeeded: " + str(result))

# GET operation - Retrieve the message
value = r.get("Message")

if value is not None:
    print("GET Message returned : " + str(value))
else:
    print("GET Message returned None")
```

## Error Handling

The script includes exception handling for common failure scenarios:

```python
except redis.ConnectionError as e:
    print(f"Connection error: {e}")
    print("Check if Redis host and port are correct, and ensure network connectivity")
except redis.AuthenticationError as e:
    print(f"Authentication error: {e}")
    print("Check if Azure Entra ID authentication is properly configured")
except redis.TimeoutError as e:
    print(f"Timeout error: {e}")
    print("Check network latency and Redis server performance")
except Exception as e:
    print(f"Unexpected error: {e}")
    if "999" in str(e):
        print("Error 999 typically indicates a network connectivity issue or firewall restriction")
```

| Error Type | Description | Resolution |
|------------|-------------|------------|
| `ConnectionError` | Network connectivity issues | Verify hostname, port, and firewall settings |
| `AuthenticationError` | Authentication failures | Check Azure login status and user permissions |
| `TimeoutError` | Request timeouts | Review network latency and Redis server performance |
| Error 999 | Network/firewall restrictions | Verify IP whitelisting and Redis accessibility |

## Project Structure

```
python-quickstart/
├── quickstart-amr.py    # Main connection script
├── requirements.txt     # Python dependencies
└── README.md            # Documentation
```

## Configuration Reference

### Redis Client Parameters

| Parameter | Value | Description |
|-----------|-------|-------------|
| `host` | Azure Managed Redis hostname | e.g., `your-cache.westus3.redis.azure.net` |
| `port` | `10000` | Default port for Azure Managed Redis |
| `ssl` | `True` | Enables TLS encryption |
| `decode_responses` | `True` | Decodes byte responses to strings |
| `socket_timeout` | `10` | Socket operation timeout in seconds |
| `socket_connect_timeout` | `10` | Connection timeout in seconds |

### Authentication Scope

```python
"https://redis.azure.com/.default"
```

## Expected Output

```
Starting Azure Redis Cache connection test...
Connecting to: your-cach***:10000

Ping returned : True

SET Message succeeded: True

GET Message returned : Hello, The cache is working with Python!

All Redis operations completed successfully!

Redis connection closed
```

**Note:** The endpoint is partially masked in logs for security purposes.

## Troubleshooting

### Authentication Errors
- Verify Azure CLI login: `az login`
- Confirm Redis Data Contributor role assignment
- Ensure Azure Entra ID is enabled on the Azure Managed Redis instance

### Connection Errors
- Ensure `REDIS_ENDPOINT` environment variable is set in `host:port` format
- Verify hostname format: `<cache-name>.<region>.redis.azure.net`
- Confirm port 10000 (not 6380)
- Check firewall IP whitelisting
- Verify the Azure Managed Redis instance is running

### Import Errors
- Install dependencies: `pip install redis>=7.0.0 azure-identity>=1.24.0 redis-entraid>=1.1.0`
- Verify virtual environment activation

## Resources

- [Azure Managed Redis Documentation](https://docs.microsoft.com/azure/azure-cache-for-redis/)
- [Azure Entra ID Authentication for Redis](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-azure-active-directory-for-authentication)
- [redis-py Documentation](https://redis-py.readthedocs.io/)

## License

MIT License

