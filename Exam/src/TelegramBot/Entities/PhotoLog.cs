namespace TelegramBot.Entities;

internal class PhotoLog
{
    public long UserId { get; set; }
    public string Query { get; set; }
    public List<string> PhotoUrls { get; set; }
}
