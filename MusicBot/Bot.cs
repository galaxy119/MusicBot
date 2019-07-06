using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MEC;
using Microsoft.Extensions.DependencyInjection;
using Victoria;
using Victoria.Entities;

namespace MusicBot
{
	public class Bot
	{
		private readonly DiscordSocketClient client;
		private readonly CommandService cmdService;
		private IServiceProvider services;
		private readonly Program program;

		public Bot(Program program, DiscordSocketClient client = null, CommandService cmdService = null)
		{
			this.program = program;
			this.client = client ?? new DiscordSocketClient(new DiscordSocketConfig {
				AlwaysDownloadUsers = true,
				MessageCacheSize = 50,
				LogLevel = LogSeverity.Warning
			});

			this.cmdService = cmdService ?? new CommandService(new CommandServiceConfig {
				LogLevel = LogSeverity.Warning,
				CaseSensitiveCommands = false
			});
			InitializeAsync().GetAwaiter().GetResult();
		}

		private async Task InitializeAsync()
		{
			await client.LoginAsync(TokenType.Bot, program.Config.BotToken);
			await client.StartAsync();
			client.Log += LogAsync;
			services = SetupServices();

			CommandHandler cmdHandler = new CommandHandler(client, cmdService, services, program);
			await cmdHandler.InitializeAsync();

			await services.GetRequiredService<MusicService>().InitializeAsync();

			SetStatus();

			await Task.Delay(-1);
		}

		private async Task SetStatus()
		{
			while (true)
			{
				if (MusicService.Player == null)
				{
					await client.SetStatusAsync(UserStatus.AFK);
					await client.SetActivityAsync(new Game("Idling."));
				}
				else if (MusicService.Player.IsPlaying)
				{
					await client.SetStatusAsync(UserStatus.Online);
					LavaTrack track = MusicService.Player.CurrentTrack;
					await client.SetActivityAsync(new Game(
						$"{track.Title} - {track.Author} ({track.Position.TotalMinutes}/{track.Length.TotalMinutes}"));
				}

				await Task.Delay(1000);
			}
		}

		private Task LogAsync(LogMessage logMessage)
		{
			Console.WriteLine(logMessage.Message);
			return Task.CompletedTask;
		}

		private IServiceProvider SetupServices()
			=> new ServiceCollection()
				.AddSingleton(client)
				.AddSingleton(cmdService)
				.AddSingleton<LavaRestClient>()
				.AddSingleton<LavaSocketClient>()
				.AddSingleton<MusicService>()
				.BuildServiceProvider();
	}
}