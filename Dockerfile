# .NET 10.0 SDK imajını kullanıyoruz derleme için
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Proje dosyalarını kopyalayıp bağımlılıkları çözüyoruz
COPY ["DashGifBot.csproj", "./"]
RUN dotnet restore

# Kalan tüm kodları kopyalayıp Web API projesi olarak yayınlıyoruz
COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Çalışma zamanı (Runtime) olarak Web/ASP.NET imajını kullanıyoruz reis
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render'ın portunu dışarı açıyoruz
EXPOSE 10000

ENTRYPOINT ["dotnet", "DashGifBot.dll"]
