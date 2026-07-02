using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Sonsuz hata korumalı döngü: Bot bir şekilde düşerse 5 sn sonra yeniden başlatır
        while (true)
        {
            try
            {
                await StartBotAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HATA: {ex.Message}");
                await Task.Delay(5000); 
            }
        }
    }

    static async Task StartBotAsync()
    {
        var client = new DiscordSocketClient(new DiscordSocketConfig 
        { 
            GatewayIntents = GatewayIntents.All | GatewayIntents.MessageContent 
        });

        client.Ready += async () => {
            // Yayında modu - Mor İkon İçin
            await client.SetGameAsync("Rozue Shop Aktif!", "https://www.twitch.tv/monstercat", ActivityType.Streaming);
            Console.WriteLine("✅ BOT 7/24 AKTİF!");
        };

        client.MessageReceived += async (message) =>
        {
            if (message.Author.IsBot || !message.Content.ToLower().StartsWith("!intro ")) return;

            string tur = message.Content.ToLower().Replace("!intro ", "").Trim();
            await message.Channel.SendMessageAsync($"🎬 **{tur.ToUpper()}** stili için talebin alındı reis!");
        };

        // Render'daki ortam değişkeninden token'ı çek
        string? token = Environment.GetEnvironmentVariable("GIF_BOT_TOKEN");
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        // Sonsuz döngü (Botun kapanmaması için)
        await Task.Delay(-1);
    }
}    
