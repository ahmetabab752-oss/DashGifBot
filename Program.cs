using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

// İki kütüphanedeki Image kelimesi çakışmasın diye ImageSharp olanı netleştiriyoruz
using SharpImage = SixLabors.ImageSharp.Image;

class Program
{
    private DiscordSocketClient _client = null!;

    static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

    public async Task RunBotAsync()
    {
        // 🚀 Render uyanık kalma web sunucusunu bot başlarken burada ateşliyoruz reis:
        StartHttpServer();

        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);
        _client.Log += Log;
        _client.MessageReceived += MessageReceivedAsync;
        _client.Ready += BotHazir;

        // 🔑 Tokeni güvenli bir şekilde Render ortam değişkenlerinden alıyoruz reis:
        string? token = Environment.GetEnvironmentVariable("GIF_BOT_TOKEN");

        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("[HATA] GIF_BOT_TOKEN bulunamadı! Lütfen Render panelini kontrol edin.");
            return;
        }

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private Task BotHazir()
    {
        Console.WriteLine($"[BAŞARILI] {_client.CurrentUser.Username} Aktif! Gif üretmeye hazır.");
        return Task.CompletedTask;
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        if (message.Content.ToLower() == "!gif profil")
        {
            var user = message.Author;
            // Discord.Net v3+'te GetAvatarUrl boyutu int alır, burayı düzelttim reis
            string avatarUrl = user.GetAvatarUrl(size: 256) ?? user.GetDefaultAvatarUrl();

            await message.Channel.SendMessageAsync("🔄 Hareketli avatarınız oluşturuluyor, lütfen bekleyin...");

            try
            {
                // Boşluğu kaldırdık: ProfilResminiGifeCevir yaptık
                string gifPath = await ProfilResminiGifeCevir(avatarUrl);

                await message.Channel.SendFileAsync(gifPath, $"{user.Username}_avatar.gif");

                if (File.Exists(gifPath)) File.Delete(gifPath);
            }
            catch (Exception ex)
            {
                await message.Channel.SendMessageAsync($"❌ Gif oluşturulurken hata çıktı: {ex.Message}");
            }
        }
    }

    // 🚀 Boşluk hatası ve Image çakışması tamamen çözülmüş ana motorumuz:
    private async Task<string> ProfilResminiGifeCevir(string userAvatarUrl)
    {
        using var httpClient = new HttpClient();
        var imageBytes = await httpClient.GetByteArrayAsync(userAvatarUrl);
        using var userImage = SharpImage.Load<Rgba32>(imageBytes);

        using var gifImage = new Image<Rgba32>(400, 400);
        gifImage.Metadata.GetGifMetadata().RepeatCount = 0;

        userImage.Mutate(x => x.Resize(400, 400));

        for (int i = 0; i < 360; i += 30)
        {
            var frame = userImage.Clone(ctx => ctx.Rotate(i));
            gifImage.Frames.AddFrame(frame.Frames.RootFrame);
        }

        gifImage.Frames.RemoveFrame(0);

        string outputPath = Path.Combine(AppContext.BaseDirectory, "temp_avatar.gif");
        gifImage.Save(outputPath, new GifEncoder());

        return outputPath;
    }

    // 🟢 BOTUN RENDER'DA 7/24 UYANIK KALMASINI SAĞLAYAN WEB SUNUCUSU:
    public static void StartHttpServer()
    {
        Task.Run(async () =>
        {
            try
            {
                var listener = new HttpListener();
                // Render'ın bota otomatik atadığı PORT numarasını çekiyoruz, yoksa varsayılan 8080
                string port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
                listener.Prefixes.Add($"http://*:{port}/");
                listener.Start();
                Console.WriteLine($"[Web Sunucu] Uyanık kalma portu dinleniyor: {port}");

                while (true)
                {
                    var context = await listener.GetContextAsync();
                    var response = context.Response;
                    string responseString = "Bot aslanlar gibi uyanık!";
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                    
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    response.OutputStream.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Web Sunucu Hatası] {ex.Message}");
            }
        });
    }
}
