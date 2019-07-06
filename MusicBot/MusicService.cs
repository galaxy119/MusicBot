using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Victoria;
using Victoria.Entities;
using Victoria.Queue;

namespace MusicBot
{
	public class MusicService
	{
		private readonly LavaRestClient lavaRestClient;
		private readonly LavaSocketClient lavaSocketClient;
		private readonly DiscordSocketClient client;
		internal static LavaPlayer Player;

		public MusicService(LavaRestClient lavaRestClient, DiscordSocketClient discordSocketClient,
			LavaSocketClient lavaSocketClient)
		{
			client = discordSocketClient;
			this.lavaRestClient = lavaRestClient;
			this.lavaSocketClient = lavaSocketClient;
		}

		public Task InitializeAsync()
		{
			client.Ready += ClientReadyAsync;
			lavaSocketClient.Log += LogAsync;
			lavaSocketClient.OnTrackFinished += TrackFinished;
			return Task.CompletedTask;
		}

		public async Task ConnectAsync(SocketVoiceChannel voiceChannel, ITextChannel textChannel) =>
			await lavaSocketClient.ConnectAsync(voiceChannel, textChannel);

		public async Task LeaveAsync(SocketVoiceChannel voiceChannel) =>
			await lavaSocketClient.DisconnectAsync(voiceChannel);

		public async Task<string> PlayAsync(string query, ulong guildId)
		{
			Player = lavaSocketClient.GetPlayer(guildId);
			SearchResult results = await lavaRestClient.SearchYouTubeAsync(query);

			if (results.LoadType == LoadType.NoMatches || results.LoadType == LoadType.LoadFailed)
				return "No matches found.";
			LavaTrack track = results.Tracks.FirstOrDefault();

			if (track is null) 
				return "The possibility of you seeing this is less than 0% but Jetbrains won't shut up about it.";

			if (Player.IsPlaying)
			{
				Player.Queue.Enqueue(track);
				return $"{track.Title} has been added to the queue.";
			}

			await Player.PlayAsync(track);
			return $"Now player: {track.Title}";
		}

		public async Task StopAsync()
		{
			if (Player is null)
				return;
			await Player.StopAsync();
		}

		public async Task<string> SkipAsync()
		{
			if (Player is null)
				return "Player isn't playing anything.";
			if (!Player.Queue.Items.Any())
				return "There is nothing in the queue, to stop the current song use the stop command.";

			LavaTrack oldTrack = Player.CurrentTrack;
			await Player.SkipAsync();
			return $"Skipped: {oldTrack.Title} \nNow Playing: {Player.CurrentTrack.Title}";
		}

		public async Task<string> SetVolumeAsync(int vol)
		{
			if (Player is null)
				return "Player isn't playing anything.";

			if (vol > 100 || vol < 2)
				return "Please set a number between 2 and 100";

			await Player.SetVolumeAsync(vol);
			return $"Volume set to: {vol}";
		}

		public async Task<string> PauseOrResumeAsync()
		{
			if (Player is null)
				return "Player isn't playing anything!";

			if (!Player.IsPaused)
			{
				await Player.PauseAsync();
				return "Player is paused.";
			}

			await Player.ResumeAsync();
			return "Playback resumed.";
		}

		public static async Task<string> ResumeAsync()
		{
			if (Player is null)
				return "Player isn't playing anything";

			if (!Player.IsPaused) 
				return "Player is not paused.";
			
			await Player.ResumeAsync();
			return "Playback resumed.";
		}
		private async Task ClientReadyAsync()
		{
			await lavaSocketClient.StartAsync(client);
		}

		private static async Task TrackFinished(LavaPlayer player, LavaTrack track, TrackEndReason reason)
		{
			if (!reason.ShouldPlayNext())
				return;

			if (!player.Queue.TryDequeue(out var item) || !(item is LavaTrack nextTrack))
			{
				await player.TextChannel.SendMessageAsync("There are no more tracks in the queue.");
				return;
			}

			await player.PlayAsync(nextTrack);
		}

		private Task LogAsync(LogMessage message)
		{
			Console.WriteLine(message.Message);
			return Task.CompletedTask;
		}
	}
}