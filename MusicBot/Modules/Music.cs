using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Victoria.Entities;

namespace MusicBot.Modules
{
	public class Music : ModuleBase<SocketCommandContext>
    {
        private readonly MusicService musicService;

        public Music(MusicService musicService) => this.musicService = musicService;

        [Command("Join")]
        public async Task Join()
        {
            SocketGuildUser user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await ReplyAsync("You need to connect to a voice channel.");
                return;
            }

            await musicService.ConnectAsync(user.VoiceChannel, Context.Channel as ITextChannel);
            await ReplyAsync($"now connected to {user.VoiceChannel.Name}");
        }

        [Command("Leave")]
        public async Task Leave()
        {
            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await ReplyAsync("Please join the channel the bot is in to make it leave.");
            }
            else
            {
                await musicService.LeaveAsync(user.VoiceChannel);
                await ReplyAsync($"Bot has now left {user.VoiceChannel.Name}");
            }
        }
        
        [Command("Fuckoff")]
        public async Task Fuckoff()
        {
            SocketGuildUser user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await ReplyAsync("Please join the channel the bot is in to make it leave.");
            }
            else
            {
                await musicService.LeaveAsync(user.VoiceChannel);
                await ReplyAsync($"Bot has now left {user.VoiceChannel.Name}");
            }
        }

        [Command("Play")]
        public async Task Play([Remainder]string query)
        {
            string result = await musicService.PlayAsync(query, Context.Guild.Id);
            await ReplyAsync(result);
        }

        [Command("Stop")]
        public async Task Stop()
        {
            await musicService.StopAsync();
            await ReplyAsync("Music Playback Stopped.");
        }

        [Command("Skip")]
        public async Task Skip()
        {
            string result = await musicService.SkipAsync();
            await ReplyAsync(result);
        }

        [Command("Volume")]
        public async Task Volume(int vol)
            => await ReplyAsync(await musicService.SetVolumeAsync(vol));

        [Command("Pause")]
        public async Task Pause()
            => await ReplyAsync(await musicService.PauseOrResumeAsync());

        [Command("Resume")]
        public async Task Resume()
            => await ReplyAsync(await MusicService.ResumeAsync());

    }
}