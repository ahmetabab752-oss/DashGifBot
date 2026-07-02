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
        // 1. Önce Botu Başlatıyoruz (Garanti)
        var botTask = RunBotAsync();

        // 2. Sonra Web Sunucusunu Başlatıyoruz (Render için)
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        app.MapGet("/", () => "Bot ayakta ve çalışıyor!");

        await Task.WhenAll(botTask, app.RunAsync());
    }

    public static async Task RunBotAsync()
    {
        var config = new DiscordSocketConfig
        {
            // BÜTÜN İZİNLERİ BURADA AÇIYORUZ
            GatewayIntents = GatewayIntents.All | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);

        _client.Log += (log) => { Console.WriteLine(log); return Task.CompletedTask; };

        _client.Ready += () => {
            Console.WriteLine("✅ BOT BAĞLANDI VE MESAJLARI DİNLİYOR!");
            return Task.CompletedTask;
        };

        _client.MessageReceived += async (message) =>
        {
            if (message.Author.IsBot) return;
            Console.WriteLine($"Gelen mesaj: {message.Content}"); // LOGLARDAN MESAJI TAKİP ET!

            if (message.Content.ToLower().StartsWith("!intro"))
            {
                await message.Channel.SendMessageAsync("🎬 Komut algılandı, işlem başlıyor...");
            }
        };

        string? token = Environment.GetEnvironmentVariable("GIF_BOT_TOKEN");
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }
}
