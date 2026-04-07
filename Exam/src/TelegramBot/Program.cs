using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TelegramBot.Entities;
using TelegramBot.Repository;

class Program
{
    private static string botToken = "8636718651:AAFvcKnWja7Kcp_cMY7ATXHDK8LqxgFG9nI";
    private static string unsplashKey = "Client-ID YOUR_ACCESS_KEY";

    private static TelegramBotClient botClient;

    private static Repository<BotUser> userRepo = new();
    private static Repository<PhotoLog> photoRepo = new();

    static async Task Main(string[] args)
    {
        botClient = new TelegramBotClient(botToken);

        using var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }
        };

        botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: cts.Token
        );

        Console.WriteLine("ishga tushdi");
        Console.ReadLine();
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Type != UpdateType.Message || update.Message!.Text == null)
            return;

        var message = update.Message;

        // ✅ Save user
        var users = await userRepo.GetAllAsync();
        if (!users.Any(u => u.chatId == message.Chat.Id))
        {
            users.Add(new BotUser
            {
                chatId = message.Chat.Id,
                Username = message.Chat.Username
            });

            await userRepo.SaveAllAsync(users);
        }

        string query = message.Text;

        // ✅ Get photos from Unsplash
        var photoUrls = await GetPhotosFromUnsplash(query);

        if (photoUrls.Count == 0)
        {
            await bot.SendMessage(message.Chat.Id, "No photos found.");
            return;
        }

        // ✅ Send photos
        foreach (var url in photoUrls)
        {
            await bot.SendPhoto(message.Chat.Id, url);
        }

        // ✅ Save log
        var logs = await photoRepo.GetAllAsync();
        logs.Add(new PhotoLog
        {
            UserId = message.Chat.Id,
            Query = query,
            PhotoUrls = photoUrls
        });

        await photoRepo.SaveAllAsync(logs);
    }

    private static async Task<List<string>> GetPhotosFromUnsplash(string query)
    {
        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Client-ID {unsplashKey}");

        var url = $"https://api.unsplash.com/search/photos?query={query}&per_page=3";

        var response = await httpClient.GetStringAsync(url);

        var json = JsonDocument.Parse(response);

        var results = json.RootElement.GetProperty("results");

        var urls = new List<string>();

        foreach (var item in results.EnumerateArray())
        {
            var imageUrl = item.GetProperty("urls").GetProperty("regular").GetString();
            urls.Add(imageUrl);
        }

        return urls;
    }

    private static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        Console.WriteLine(exception.Message);
        return Task.CompletedTask;
    }
}