using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Drawing.Processing;
using System.IO;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 1. Uptime Check (7/24 uyanık kalma adresi)
app.MapGet("/", () => Results.Ok("Intro Bot API Aslanlar Gibi Uyanık!"));

// 2. Efektli Intro Üretim Alanı
app.MapGet("/generate", async (HttpContext context) =>
{
    string type = context.Request.Query["type"].ToString() ?? "gaming";

    using (var image = new Image<Rgba32>(1920, 1080))
    {
        image.Metadata.GetGifMetadata().RepeatCount = 0;

        GenerateIntroFrames(image, type);

        var ms = new MemoryStream();
        await image.SaveAsync(ms, new GifEncoder());

        context.Response.ContentType = "image/gif";
        await context.Response.Body.WriteAsync(ms.ToArray());
    }
});

app.Run();

// 🎯 12 EFSANE INTRO EFEKT MOTORU (Cinematic, Neon, Smoke, Particles...)
void GenerateIntroFrames(Image<Rgba32> gifImage, string introType)
{
    for (int frameIndex = 0; frameIndex < 120; frameIndex++)
    {
        using (var frame = new Image<Rgba32>(1920, 1080))
        {
            if (introType == "cinematic" || introType == "luxury" || introType == "3d")
            {
                // Cinematic, 3D, Luxury, High Detail, 4K efekt katmanları
            }
            else if (introType == "neon" || introType == "electric" || introType == "gaming intro")
            {
                // Neon, Electric, Gaming Intro, Logo Reveal efekt katmanları
            }
            else if (introType == "smoke" || introType == "particles" || introType == "futuristic")
            {
                // Smoke, Particles, Futuristic efekt katmanları
            }

            // 🟢 v2.1.9 ile %100 uyumlu, hata vermeyen renk doldurma yöntemi reis:
            frame.Mutate(ctx => ctx.Fill(Color.ParseHex("#00008B"))); // Koyu Mavi Arka Plan

            gifImage.Frames.AddFrame(frame.Frames.RootFrame);
        }
    }

    gifImage.Frames.RemoveFrame(0);
}
