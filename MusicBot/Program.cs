using System;
using System.Threading.Tasks;
using Discord;
using System.IO;
using Newtonsoft.Json;

namespace MusicBot
{
	public class Program
	{
		public static async Task Main(string[] args) => await new Bot().InitializeAsync();
	}
}