#Python Quickstart using Azure Entra ID authentication
# Azure Managed Redis cache that you created using the Azure portal, or CLI
# This script demonstrates secure connection using Microsoft Entra ID authentication
# This script demonstrates secure connection using the default Azure credential provider
# You should be a user on the cache and logged in to Azure CLI with the same account using `az login`

import os
import redis
from azure.identity import DefaultAzureCredential
from redis_entraid.cred_provider import create_from_default_azure_credential

# Connection details for your cache
# Get the connection details from environment variable
# Set REDIS_ENDPOINT environment variable in format: host:port
redis_endpoint = os.environ.get("REDIS_ENDPOINT")

# Validate configuration
if not redis_endpoint or ":" not in redis_endpoint:
    print("Error: REDIS_ENDPOINT environment variable must be set in host:port format")
    exit(1)

# Parse host and port from endpoint
redis_host, redis_port = redis_endpoint.split(":")
redis_port = int(redis_port)

# Mask endpoint for logging (show only first few characters)
masked_endpoint = redis_host[:8] + "***" + ":" + str(redis_port)

print("Starting Azure Managed Redis connection test...")
print(f"Connecting to: {masked_endpoint}")

print()  # Add a new line

try:
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

    # Test connection 
    result = r.ping()
    print("Ping returned : " + str(result))
    print()  # Add a new line

    # Create a simple set and get operation
    result = r.set("Message", "Hello, The cache is working.")
    print("SET Message succeeded: " + str(result))
    print()  # Add a new line

    value = r.get("Message")

    if value is not None:
        print("GET Message returned : " + str(value))
        print()  # Add a new line
    else:
        print("GET Message returned None")
        print()  # Add a new line
    
    print("All Redis operations completed successfully!")
    print()  # Add a new line

except redis.ConnectionError as e:
    print(f"Connection error: {e}")
    print("Check if Redis host and port are correct, and ensure network connectivity")
    print()  # Add a new line
except redis.AuthenticationError as e:
    print(f"Authentication error: {e}")
    print("Check if Azure Entra ID authentication is properly configured")
    print()  # Add a new line
except redis.TimeoutError as e:
    print(f"Timeout error: {e}")
    print("Check network latency and Redis server performance")
    print()  # Add a new line
except Exception as e:
    print(f"Unexpected error: {e}")
    if "999" in str(e):
        print("Error 999 typically indicates a network connectivity issue or firewall restriction")
        print("   - Verify the Redis hostname is correct")
        print("   - Check if your IP is whitelisted in Redis firewall settings")
        print("   - Ensure the Redis cache is running and accessible")
    print()  # Add a new line
finally:
    # Clean up connection if it exists
    if 'r' in locals():
        try:
            r.close()
            print("Redis connection closed")
        except Exception as e:
            print(f"Error closing connection: {e}")
