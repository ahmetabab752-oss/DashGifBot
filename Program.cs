using Microsoft.AspNetCore.Builder;
using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

class Program
{
    private static DiscordSocketClient _client = null!;

    static async Task Main(string[] args)
    {
        var botTask = RunBotAsync();
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        app.MapGet("/", () => "Bot ayakta!");
        await Task.WhenAll(botTask, app.RunAsync());
    }

    public static async Task RunBotAsync()
    {
        var config = new DiscordSocketConfig { GatewayIntents = GatewayIntents.All | GatewayIntents.MessageContent };
        _client = new DiscordSocketClient(config);

        _client.Ready += async () => {
            string yayınMetni = "Dash Shop Aktif!";
            string twitchLink = "https://www.twitch.tv/monstercat";
            await _client.SetGameAsync(yayınMetni, twitchLink, ActivityType.Streaming);
            Console.WriteLine("✅ BOT BAĞLANDI VE YAYINA GİRDİ!");
        };

        _client.MessageReceived += async (message) =>
        {
            if (message.Author.IsBot || !message.Content.ToLower().StartsWith("!intro ")) return;

            string tur = message.Content.ToLower().Replace("!intro ", "").Trim();
            await message.Channel.SendMessageAsync($"🎬 **{tur.ToUpper()}** stili için intro talebin alındı reis, kısa süre içinde sana dönüş yapacağım!");
        };

        await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("GIF_BOT_TOKEN"));
        await _client.StartAsync();
        await Task.Delay(-1);
    }
}
