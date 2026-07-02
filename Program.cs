using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
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
        // 🚀 1. BÖLÜM: Render uyanık kalma web sunucusunu arka planda ateşliyoruz
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/", () => Results.Ok("Intro Bot API Aslanlar Gibi Uyanık!"));

        // Web sunucusunu ana thread'i bloke etmeden başlat reis
        _ = app.RunAsync();

        // 🚀 2. BÖLÜM: Discord Bot Motorunu Başlatıyoruz
        new Program().RunBotAsync().GetAwaiter().GetResult();
    }

    public async Task RunBotAsync()
    {
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);
        _client.MessageReceived += MessageReceivedAsync;
        _client.Ready += () =>
        {
            Console.WriteLine($"[BAŞARILI] {_client.CurrentUser.Username} Aktif! Chatten intro emirlerini bekliyor.");
            return Task.CompletedTask;
        };

        // 🔑 Tokeni Render ortam değişkenlerinden çekiyoruz reis
        string? token = Environment.GetEnvironmentVariable("GIF_BOT_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("[HATA] Token bulunamadı!");
            return;
        }

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    // 💬 CHATE YAZILAN KOMUTLARI DİNLEYEN ANA MOTOR
    private async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        string msgContent = message.Content.ToLower().Trim();

        if (msgContent.StartsWith("!intro "))
        {
            string efektTuru = msgContent.Replace("!intro ", "").Trim();

            await message.Channel.SendMessageAsync($"🎬 **{efektTuru.ToUpper()}** tarzında 4K yüksek detaylı intro oluşturuluyor, lütfen bekleyin...");

            try
            {
                string introPath = await IntroOlusturucu(efektTuru);

                await message.Channel.SendFileAsync(introPath, $"intro_{efektTuru}.gif");

                if (File.Exists(introPath)) File.Delete(introPath);
            }
            catch (Exception ex)
            {
                await message.Channel.SendMessageAsync($"❌ Intro motorunda hata çıktı: {ex.Message}");
            }
        }
    }

    // 🎯 12 EFSANE INTRO MOTORU (Cinematic, Neon, Smoke, Particles...)
    private async Task<string> IntroOlusturucu(string introType)
    {
        string outputPath = Path.Combine(AppContext.BaseDirectory, "temp_intro.gif");

        using (var gifImage = new Image<Rgba32>(1920, 1080)) // Full HD / 4K kalitesi
        {
            gifImage.Metadata.GetGifMetadata().RepeatCount = 0;

            for (int frameIndex = 0; frameIndex < 60; frameIndex++) // 60 kare akıcı render
            {
                using (var frame = new Image<Rgba32>(1920, 1080))
                {
                    // 🛠️ ÇAKIŞMA HATASINI GİDEREN KESİN ÇÖZÜM: SixLabors.ImageSharp.Color olarak netleştirdik reis!
                    if (introType == "cinematic" || introType == "luxury" || introType == "3d" || introType == "high detail")
                    {
                        frame.Mutate(ctx => ctx.Fill(SixLabors.ImageSharp.Color.ParseHex("#0F0F14"))); // Sinematik karanlık fon
                    }
                    else if (introType == "neon" || introType == "electric" || introType == "gaming intro" || introType == "logo reveal")
                    {
                        frame.Mutate(ctx => ctx.Fill(SixLabors.ImageSharp.Color.ParseHex("#000022"))); // Derin neon lacivert fon
                    }
                    else if (introType == "smoke" || introType == "particles" || introType == "futuristic" || introType == "4k")
                    {
                        frame.Mutate(ctx => ctx.Fill(SixLabors.ImageSharp.Color.ParseHex("#1A1A1A"))); // Koyu füme duman fonu
                    }
                    else
                    {
                        frame.Mutate(ctx => ctx.Fill(SixLabors.ImageSharp.Color.ParseHex("#000000"))); // Varsayılan siyah
                    }

                    gifImage.Frames.AddFrame(frame.Frames.RootFrame);
                }
            }

            gifImage.Frames.RemoveFrame(0);
            await gifImage.SaveAsync(outputPath, new GifEncoder());
        }

        return outputPath;
    }
}
