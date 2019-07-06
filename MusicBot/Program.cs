using System.IO;
using Newtonsoft.Json;

namespace MusicBot
{
	public class Program
	{
		private const string kCfgFile = "MusicBotConfig.json";
		private Config config;
		public Config Config => config ?? (config = GetConfig());
		public static void Main() => new Program();

		private Program() => new Bot(this);

		private static Config GetConfig()
		{
			if (File.Exists(kCfgFile))
				return JsonConvert.DeserializeObject<Config>(File.ReadAllText(kCfgFile));
			File.WriteAllText(kCfgFile, JsonConvert.SerializeObject(Config.Default, Formatting.Indented));
			return Config.Default;
		}
	}

	public class Config
	{
		public string BotToken { get; set; }
		public ulong AdvancedCommandId { get; set; }
		public bool RestrictAllCmds { get; set; }

		public static readonly Config Default = new Config
		{
			BotToken = "", 
			AdvancedCommandId = 0, 
			RestrictAllCmds = false
		};
	}
}