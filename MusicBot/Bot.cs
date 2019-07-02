using System;
using System.ComponentModel.Design;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Victoria;

namespace MusicBot
{
	public class Bot
	{
		private DiscordSocketClient _client;
		private CommandService _cmdService;
		private IServiceProvider _services;

		public Bot(DiscordSocketClient client = null, CommandService cmdService = null)
		{
			_client = client ?? new DiscordSocketClient(new DiscordSocketConfig {
				AlwaysDownloadUsers = true,
				MessageCacheSize = 50,
				LogLevel = LogSeverity.Debug
			});

			_cmdService = cmdService ?? new CommandService(new CommandServiceConfig {
				LogLevel = LogSeverity.Verbose,
				CaseSensitiveCommands = false
			});
		}

		public async Task InitializeAsync()
		{
			await _client.LoginAsync(TokenType.Bot, "NTk1NDQ4OTYzMjQ3NjM2NDkw.XRrJ4w.0ZVtaDR4IOvOb1k1vwu9FfYjDhs");
			await _client.StartAsync();
			_client.Log += LogAsync;
			_services = SetupServices();

			var cmdHandler = new CommandHandler(_client, _cmdService, _services);
			await cmdHandler.InitializeAsync();

			await _services.GetRequiredService<MusicService>().InitializeAsync();

			await Task.Delay(-1);
		}

		private Task LogAsync(LogMessage logMessage)
		{
			Console.WriteLine(logMessage.Message);
			return Task.CompletedTask;
		}

		private IServiceProvider SetupServices()
			=> new ServiceCollection()
				.AddSingleton(_client)
				.AddSingleton(_cmdService)
				.AddSingleton<LavaRestClient>()
				.AddSingleton<LavaSocketClient>()
				.AddSingleton<MusicService>()
				.BuildServiceProvider();
	}
}