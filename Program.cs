using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/", () => Results.Ok("Bot uyanık!"));
        _ = app.RunAsync();

        new Program().RunBotAsync().GetAwaiter().GetResult();
    }

    public async Task RunBotAsync()
    {
        // 🚨 EN GARANTİ İZİN AYARLARI
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMessages
        };

        _client = new DiscordSocketClient(config);
        _client.MessageReceived += MessageReceivedAsync;

        string? token = Environment.GetEnvironmentVariable("GIF_BOT_TOKEN");
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        // 🚨 Sadece !intro ile başlayanları yakala
        if (message.Content.ToLower().StartsWith("!intro "))
        {
            string tur = message.Content.Replace("!intro ", "").Trim().ToLower();
            
            var msg = await message.Channel.SendMessageAsync($"🎬 {tur.ToUpper()} hazırlanıyor...");

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
        string path = Path.Combine(AppContext.BaseDirectory, "intro.gif");
        using (var gif = new Image<Rgba32>(1920, 1080))
        {
            gif.Metadata.GetGifMetadata().RepeatCount = 0;
            for (int i = 0; i < 30; i++) 
            {
                using (var frame = new Image<Rgba32>(1920, 1080))
                {
                    // Efekt renkleri
                    var renk = tur == "neon" ? SixLabors.ImageSharp.Color.ParseHex("#000022") : SixLabors.ImageSharp.Color.ParseHex("#000000");
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
