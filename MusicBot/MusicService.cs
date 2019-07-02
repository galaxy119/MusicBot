using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.XPath;
using Discord;
using Discord.WebSocket;
using Victoria;
using Victoria.Entities;
using Victoria.Queue;

namespace MusicBot
{
	public class MusicService
	{
		private LavaRestClient _lavaRestClient;
		private LavaSocketClient _lavaSocketClient;
		private DiscordSocketClient _client;
		private LavaPlayer _player;

		public MusicService(LavaRestClient lavaRestClient, DiscordSocketClient discordSocketClient,
			LavaSocketClient lavaSocketClient)
		{
			_client = discordSocketClient;
			_lavaRestClient = lavaRestClient;
			_lavaSocketClient = lavaSocketClient;
		}

		public Task InitializeAsync()
		{
			_client.Ready += ClientReadyAsync;
			_lavaSocketClient.Log += LogAsync;
			_lavaSocketClient.OnTrackFinished += TrackFinished;
			return Task.CompletedTask;
		}

		public async Task ConnectAsync(SocketVoiceChannel voiceChannel, ITextChannel textChannel) =>
			await _lavaSocketClient.ConnectAsync(voiceChannel, textChannel);

		public async Task LeaveAsync(SocketVoiceChannel voiceChannel, ITextChannel textChannel) =>
			await _lavaSocketClient.DisconnectAsync(voiceChannel);

		public async Task<string> PlayAsync(string query, ulong guildId)
		{
			_player = _lavaSocketClient.GetPlayer(guildId);
			SearchResult results = await _lavaRestClient.SearchYouTubeAsync(query);

			if (results.LoadType == LoadType.NoMatches || results.LoadType == LoadType.LoadFailed)
				return "No matches found.";
			LavaTrack track = results.Tracks.FirstOrDefault();

			if (_player.IsPlaying)
			{
				_player.Queue.Enqueue(track);
				return $"{track.Title} has been added to the queue.";
			}

			await _player.PlayAsync(track);
			return $"Now player: {track.Title}";
		}

		public async Task StopAsync()
		{
			if (_player is null)
				return;
			await _player.StopAsync();
		}

		public async Task<string> SkipAsync()
		{
			if (_player is null || _player.Queue.Items.Count() is 0)
				return "Player isn't playing anything.";

			LavaTrack oldTrack = _player.CurrentTrack;
			await _player.SkipAsync();
			return $"Skipped: {oldTrack.Title} \nNow Playing: {_player.CurrentTrack.Title}";
		}

		public async Task<string> SetVolumeAsync(int vol)
		{
			if (_player is null)
				return "Player isn't playing anything.";

			if (vol > 100 || vol < 2)
				return "Please set a number between 2 and 100";

			await _player.SetVolumeAsync(vol);
			return $"Volume set to: {vol}";
		}

		public async Task<string> PauseOrResumeAsync()
		{
			if (_player is null)
				return "Player isn't playing anything!";

			if (!_player.IsPaused)
			{
				await _player.PauseAsync();
				return "Player is paused.";
			}

			await _player.ResumeAsync();
			return "Playback resumed.";
		}

		public async Task<string> ResumeAsync()
		{
			if (_player is null)
				return "Player isn't playing anything";

			if (!_player.IsPaused) 
				return "Player is not paused.";
			
			await _player.ResumeAsync();
			return "Playback resumed.";
		}
		private async Task ClientReadyAsync()
		{
			await _lavaSocketClient.StartAsync(_client);
		}

		private async Task TrackFinished(LavaPlayer player, LavaTrack track, TrackEndReason reason)
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