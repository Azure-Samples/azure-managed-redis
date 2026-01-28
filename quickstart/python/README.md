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
- Appropriate data access policy assignments for your user (for example, Redis Data Contributor)

## Quick Start

### Download Project and install dependencies

Download this repo.

### Install Dependencies

```bash
pip install -r requirements.txt
```

### Configure Connection

Set the `REDIS_ENDPOINT` environment variable with your Azure Managed Redis endpoint:

**Note:** Azure Managed Redis uses port 10000, which differs from Azure Cache for Redis (port 6380). The endpoint is masked in logs for security.

### 4. Run the Script

```bash
python quickstart-amr.py

```

## Project Structure

```
python-quickstart/
├── quickstart-amr.py    # Main connection script
├── requirements.txt     # Python dependencies
└── README.md            # Documentation
```

## Implementation Details

### Establishing Connection

The script establishes a secure connection using Azure Entra ID authentication via `DefaultAzureCredential`:

Configuration parameters:

- `ssl=True`: Enables TLS encryption for all communication
- `decode_responses=True`: Automatically decodes byte responses to strings
- `socket_timeout` / `socket_connect_timeout`: 10-second timeout for reliability

### Connection Verification

The script tests connectivity to the cache by sending a PING command.

### SET and GET Operations

Employs the basic Redis operations for storing and retrieving data: set and get.

## Error Handling

The script includes exception handling for common failure scenarios.

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
| Unexpected Error `999` | Network/firewall restrictions | Verify IP whitelisting and Redis accessibility |

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

## Expected Output

```bash
Starting Azure Redis Cache connection test...
Connecting to: your-cac***:10000

Ping returned : True

SET Message succeeded: True

GET Message returned : <your configured message value>

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

- Install dependencies: `pip install redis>=7.0.0 redis-entraid>=1.1.0`
- Verify virtual environment activation

## Resources

- [Azure Managed Redis Documentation](https://learn.microsoft.com/azure/redis/)
- [Azure Entra ID Authentication for Redis](https://learn.microsoft.com/azure/redis/cache-azure-active-directory-for-authentication)
- [redis-py Documentation](https://redis-py.readthedocs.io/)

## License

MIT License
