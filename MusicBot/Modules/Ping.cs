using System.Threading.Tasks;
using Discord.Commands;

namespace MusicBot.Modules
{
	public class Ping : ModuleBase<SocketCommandContext>
	{
		[Command("Ping")]
		public async Task Pong()
		{
			await ReplyAsync("Pong!");
		}
	}
}