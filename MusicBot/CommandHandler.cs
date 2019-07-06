using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace MusicBot
{
	public class CommandHandler
	{
		private readonly DiscordSocketClient client;
		private readonly CommandService cmdService;
		private readonly IServiceProvider services;
		private readonly Program program;

		public CommandHandler(DiscordSocketClient client, CommandService cmdService, IServiceProvider services, Program program)
		{
			this.program = program;
			this.client = client;
			this.cmdService = cmdService;
			this.services = services;
		}

		public async Task InitializeAsync()
		{
			await cmdService.AddModulesAsync(Assembly.GetEntryAssembly(), services);
			cmdService.Log += LogAsync;
			client.MessageReceived += HandleMessageAsync;
		}

		private async Task HandleMessageAsync(SocketMessage socketMessage)
		{
			int argPos = 0;
			if (socketMessage.Author.IsBot) return;

			if (((IGuildUser) socketMessage.Author).RoleIds.All(r => r != program.Config.AdvancedCommandId) &&
			    (socketMessage.Content.Contains("volume") || program.Config.RestrictAllCmds))
				return;

			SocketUserMessage userMessage = socketMessage as SocketUserMessage;
			if (userMessage is null)
				return;

			if (!userMessage.HasMentionPrefix(client.CurrentUser, ref argPos))
				return;

			SocketCommandContext context = new SocketCommandContext(client, userMessage);
			IResult result = await cmdService.ExecuteAsync(context, argPos, services);
		}

		private static Task LogAsync(LogMessage logMessage)
		{
			Console.WriteLine(logMessage.Message);
			return Task.CompletedTask;
		}
	}
}