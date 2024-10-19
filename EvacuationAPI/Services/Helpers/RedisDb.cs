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
            var redisConnectionString = Configuration.GetValue<string>("Redis:ConnectionString");

            if (string.IsNullOrWhiteSpace(redisConnectionString))
            {
                throw new ArgumentException("The Redis connection string cannot be null or empty.", nameof(redisConnectionString));
            }

            Multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
        }
    }
}
