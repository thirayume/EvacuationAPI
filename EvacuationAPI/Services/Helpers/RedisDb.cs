using StackExchange.Redis;

namespace EvacuationAPI.Services.Helpers
{
    public class RedisDb
    {
        public IConfiguration Configuration { get; }
        public ConnectionMultiplexer Multiplexer { get; }

        public RedisDb(IConfiguration configuration)
        {
            Configuration = configuration;

            // Get the connection string from configuration
            var redisConnectionString = Configuration.GetValue<string>("Redis:ConnectionString");

            // Check if the connection string is null or empty
            if (string.IsNullOrWhiteSpace(redisConnectionString))
            {
                // Provide a meaningful message for the ArgumentException
                throw new ArgumentException("The Redis connection string cannot be null or empty.", nameof(redisConnectionString));
            }

            Multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
        }
    }
}
