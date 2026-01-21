using System;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Azure.StackExchangeRedis;
using StackExchange.Redis;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // Get the endpoint from environment variable
        string? endpoint = Environment.GetEnvironmentVariable("REDIS_ENDPOINT");
        
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            Console.WriteLine("ERROR: REDIS_ENDPOINT environment variable is not set.");
            Console.WriteLine("Please set it to your Azure Redis Cache endpoint in the format: hostname:port\r\n");
            Console.WriteLine("Example: <cachename>.westus3.redis.azure.net:10000\r\n");
            return;
        }

        ConnectionMultiplexer? connection = null;
        
        try
        {
            // --- Section 1: Connection Section ---
            // Using Microsoft.Azure.StackExchangeRedis for Microsoft ID authentication
            // This handles token acquisition and refresh automatically
            Console.WriteLine("Acquiring Azure credentials...\r\n");
            var tokenCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeInteractiveBrowserCredential = false, //set to false to enable user to authenticate interactively
            });

            Console.WriteLine($"Connecting to Azure Managed Redis Cache at {endpoint} using Microsoft Entra ID authentication...\r\n");

            // Parse the endpoint and create connection configuration
            var configurationOptions = ConfigurationOptions.Parse(endpoint);
            if (configurationOptions.EndPoints.Count == 0)
            {
                Console.WriteLine("ERROR: Invalid endpoint format.");
                Console.WriteLine("Press any key to exit...\r\n");
                Console.ReadKey();
                return;
            }
            
            // Configure connection settings for reliability and security
            configurationOptions.Ssl = true;                       // Enable SSL/TLS encryption
            configurationOptions.AbortOnConnectFail = false;       // Allow connection retries
            configurationOptions.ConnectTimeout = 30000;           // Wait up to 30 seconds for connection
            configurationOptions.SyncTimeout = 30000;              // Timeout for synchronous operations
            configurationOptions.AsyncTimeout = 30000;             // Timeout for asynchronous operations
            configurationOptions.ConnectRetry = 3;                 // Retry connection up to 3 times

            // Configure the connection to use Azure Entra ID authentication
            // This method handles token acquisition and automatic token refresh
            Console.WriteLine("Configuring Azure authentication token...\r\n");
            await configurationOptions.ConfigureForAzureWithTokenCredentialAsync(tokenCredential);

            // Establish the connection to Redis
            Console.WriteLine("Establishing connection to Redis (this may take up to 30 seconds)...\r\n");
            connection = await ConnectionMultiplexer.ConnectAsync(configurationOptions);
            
            if (connection == null || !connection.IsConnected)
            {
                Console.WriteLine("ERROR: Failed to establish connection to Redis.\r\n");
                Console.WriteLine("Press any key to exit...\r\n");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine($"Connection established! IsConnected: {connection.IsConnected}\r\n");

            // Get a database reference for executing Redis commands
            IDatabase? redisCache = connection.GetDatabase();
            if (redisCache == null)
            {
                Console.WriteLine("ERROR: Failed to get database reference from Redis connection.\r\n");
                Console.WriteLine("Press any key to exit...\r\n");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine("Successfully acquired database reference from Redis connection.\r\n");

            // --- SECTION 2: Ping Test ---
            try
            {
                // Send a PING command to verify the connection is working
                // This is a lightweight operation that tests round-trip communication
                Console.WriteLine("Attempting to ping Redis server...\r\n");
                TimeSpan pingResult = await redisCache.PingAsync();
                Console.WriteLine($"Redis ping successful! Response time: {pingResult}\r\n");
                
                // Check if ping response time is unusually high
                if (pingResult > TimeSpan.FromSeconds(5))
                {
                    Console.WriteLine($"WARNING: Ping response time is high ({pingResult}). Connection may be slow or unstable.\r\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error performing Ping operations: {ex.Message}\r\n");
                Console.WriteLine($"{ex.StackTrace}\r\n");

                // Display connection status for each endpoint
                foreach (var endpointInfo in connection.GetEndPoints())
                {
                    var server = connection.GetServer(endpointInfo);
                    Console.WriteLine($"Server {endpointInfo}: IsConnected={server.IsConnected}\r\n");
                }
                
                Console.WriteLine("Ping failed. Skipping data operations.\r\n");
                Console.WriteLine("Press any key to exit...\r\n");
                Console.ReadKey();
                return;
            }

            // --- SECTION 3: Data Operations (SET/GET) ---
            try
            {
                // Demonstrate basic Redis string operations
                string key = "test:key";
                string value = "Hello from .NET 10 with Microsoft Entra ID authentication!";

                // SET operation: Store a string value in Redis
                Console.WriteLine($"Setting key '{key}' with value '{value}'...\r\n");
                bool setResult = await redisCache.StringSetAsync(key, value);
                Console.WriteLine($"Set value result: {setResult}\r\n");

                // GET operation: Retrieve the string value from Redis
                Console.WriteLine($"Retrieving value for key '{key}'...\r\n");
                string? getValue = await redisCache.StringGetAsync(key);
                Console.WriteLine($"Retrieved value: {getValue ?? "null"}\r\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error performing Redis data operations: {ex.Message}\r\n");
                Console.WriteLine($"{ex.StackTrace}\r\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to Redis: {ex.Message}\r\n");
            
            // Display inner exception details if available
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}\r\n");
            }
            
            Console.WriteLine($"{ex.StackTrace}\r\n");
            
            // Provide helpful troubleshooting guidance
            Console.WriteLine("\r\nTroubleshooting tips:");
            Console.WriteLine("1. Verify the Redis cache endpoint is correct");
            Console.WriteLine("2. Check if your IP address is allowed in the Redis firewall rules");
            Console.WriteLine("3. Ensure you have 'Redis Cache Data Owner' or 'Redis Cache Data Contributor' role on the Redis cache");
            Console.WriteLine("4. Verify you're authenticated with Azure: az account show\r\n");
            
            Console.WriteLine("Press any key to exit...\r\n");
            Console.ReadKey();
        }
        finally
        {
            // Clean up: Dispose the connection to release resources
            if (connection != null)
            {
                Console.WriteLine("\r\nClosing Redis connection...\r\n");
                connection.Dispose();
            }
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}