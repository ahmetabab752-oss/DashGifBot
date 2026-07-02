using Microsoft.AspNetCore.Builder;
using Discord;
using Discord.WebSocket;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Drawing.Processing;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    private static DiscordSocketClient _client = null!;

    static void Main(string[] args)
    {
        // 1. Önce botu başlatıyoruz
        new Program().RunBotAsync().GetAwaiter().GetResult();
    }

    public async Task RunBotAsync()
    {
        // 2. İzinleri garanti altına alıyoruz
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMessages
        };

        _client = new DiscordSocketClient(config);
        _client.MessageReceived += MessageReceivedAsync;

        _client.Ready += () => {
            Console.WriteLine("✅ BOT BAŞARIYLA BAĞLANDI!");
            return Task.CompletedTask;
        };

        string? token = Environment.GetEnvironmentVariable("GIF_BOT_TOKEN");
        if (string.IsNullOrEmpty(token)) return;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // 3. Web sunucusunu en son tetikliyoruz
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();
        app.MapGet("/", () => "Bot ayakta!");
        await app.RunAsync();
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        // 4. Komut dinleyici
        if (message.Content.ToLower().StartsWith("!intro "))
        {
            string tur = message.Content.ToLower().Replace("!intro ", "").Trim();

            try
            {
                string path = await IntroOlusturucu(tur);
                await message.Channel.SendFileAsync(path, "intro.gif");
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception ex)
            {
                await message.Channel.SendMessageAsync($"❌ Hata: {ex.Message}");
            }
        }
    }

    private async Task<string> IntroOlusturucu(string tur)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "temp.gif");
        using (var gif = new Image<Rgba32>(1920, 1080))
        {
            gif.Metadata.GetGifMetadata().RepeatCount = 0;
            for (int i = 0; i < 30; i++)
            {
                using (var frame = new Image<Rgba32>(1920, 1080))
                {
                    // Efekt renk motoru
                    var renk = tur == "neon" ? SixLabors.ImageSharp.Color.ParseHex("#000022") : SixLabors.ImageSharp.Color.ParseHex("#1A1A1A");
                    frame.Mutate(ctx => ctx.Fill(renk));
                    gif.Frames.AddFrame(frame.Frames.RootFrame);
                }
            }
            gif.Frames.RemoveFrame(0);
            await gif.SaveAsync(path, new GifEncoder());
        }
        return path;
    }
}
