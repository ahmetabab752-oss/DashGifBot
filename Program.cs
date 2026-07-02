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
using System.Collections.Generic;

class Program
{
    private static DiscordSocketClient _client = null!;

    static async Task Main(string[] args)
    {
        var botTask = RunBotAsync();
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        app.MapGet("/", () => "Bot Intro Motoru Aktif!");
        await Task.WhenAll(botTask, app.RunAsync());
    }

    public static async Task RunBotAsync()
    {
        var config = new DiscordSocketConfig { GatewayIntents = GatewayIntents.All | GatewayIntents.MessageContent };
        _client = new DiscordSocketClient(config);

        _client.MessageReceived += async (message) =>
        {
            if (message.Author.IsBot || !message.Content.ToLower().StartsWith("!intro ")) return;

            string tur = message.Content.ToLower().Replace("!intro ", "").Trim();
            var msg = await message.Channel.SendMessageAsync($"🎬 **{tur.ToUpper()}** stili yükleniyor, render ediliyor...");

            try
            {
                string path = await IntroOlusturucu(tur);
                await message.Channel.SendFileAsync(path, $"intro_{tur}.gif");
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception ex)
            {
                await msg.ModifyAsync(m => m.Content = $"❌ Render Hatası: {ex.Message}");
            }
        };

        await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("GIF_BOT_TOKEN"));
        await _client.StartAsync();
        await Task.Delay(-1);
    }

    private static async Task<string> IntroOlusturucu(string tur)
    {
        string path = Path.Combine(Path.GetTempPath(), "intro.gif");

        // 🎨 EFEKT PROFİL SİSTEMİ
        var renkler = new Dictionary<string, string> {
            { "cinematic", "#0F0F14" }, { "neon", "#0000FF" }, { "smoke", "#2F2F2F" },
            { "particles", "#FFFFFF" }, { "electric", "#00FFFF" }, { "gaming", "#FF00FF" },
            { "futuristic", "#00FF00" }, { "luxury", "#DAA520" }, { "3d", "#333333" },
            { "high detail", "#000000" }, { "4k", "#1A1A1A" }, { "logo reveal", "#FF4500" }
        };

        string secilenRenk = renkler.ContainsKey(tur) ? renkler[tur] : "#000000";

        using (var gif = new Image<Rgba32>(1920, 1080))
        {
            gif.Metadata.GetGifMetadata().RepeatCount = 0;
            for (int i = 0; i < 30; i++)
            {
                using (var frame = new Image<Rgba32>(1920, 1080))
                {
                    // Efekti uyguluyoruz
                    frame.Mutate(ctx => ctx.Fill(SixLabors.ImageSharp.Color.ParseHex(secilenRenk)));
                    gif.Frames.AddFrame(frame.Frames.RootFrame);
                }
            }
            gif.Frames.RemoveFrame(0);
            await gif.SaveAsync(path, new GifEncoder());
        }
        return path;
    }
}
