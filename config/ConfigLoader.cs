using Newtonsoft.Json;

using Newtonsoft.Json;

namespace Townsward.config
{
    public class BotConfig
    {
        public string token { get; set; }
        public string prefix { get; set; }
    }

    public class ConfigLoader
    {
        public async Task<BotConfig> ReadAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync("config.json");
                return JsonConvert.DeserializeObject<BotConfig>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to load config: {ex.Message}");
                throw;
            }
        }
    }
}
