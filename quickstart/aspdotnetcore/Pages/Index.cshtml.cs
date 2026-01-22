using Azure.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace aspnetcore_razor_pages_quickstart.Pages   
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        private ConnectionMultiplexer? connectionMultiplexer;
        
        private string RedisEndpoint => _configuration["Redis:Endpoint"] ?? throw new InvalidOperationException("Redis:Endpoint not configured");
        
        public string ConnectionStatus { get; set; } = string.Empty;
        public string PingResult { get; set; } = string.Empty;
        public string[] KeyResults { get; set; } = new string[3];
        public string ErrorMessage { get; set; } = string.Empty;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            try
            {
                // --- Section 1: Connection Section ---
                // Using Microsoft.Azure.StackExchangeRedis for Azure Entra ID authentication
                var credential = new DefaultAzureCredential();

                var configurationOptions = ConfigurationOptions.Parse(RedisEndpoint);

                configurationOptions = await configurationOptions.ConfigureForAzureWithTokenCredentialAsync(credential);
                
                connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(configurationOptions);
                
                ConnectionStatus = $"Connected to {RedisEndpoint} using Entra ID";

                // --- Section 2: Ping Section ---
                var db = connectionMultiplexer?.GetDatabase();
                if (db != null)
                {
                    var ping = await db.PingAsync();
                    PingResult = $"Ping: {ping.TotalMilliseconds} ms";

                    // --- Section 3: Set/Get Section ---
                    await db.StringSetAsync("Key1", "Alpha");
                    await db.StringSetAsync("Key2", "Bravo");
                    await db.StringSetAsync("Key3", "Charlie");
                    KeyResults[0] = $"Key1: {await db.StringGetAsync("Key1")}";
                    KeyResults[1] = $"Key2: {await db.StringGetAsync("Key2")}";
                    KeyResults[2] = $"Key3: {await db.StringGetAsync("Key3")}";
                }
                else
                {
                    ErrorMessage = "Failed to get database instance";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                ConnectionStatus = "Failed to connect.";
            }
        }
    }
}
