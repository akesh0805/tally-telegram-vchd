using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TallyTelegramVchd.Models;
using Telegram.Bot;

namespace TallyTelegramVchd;

public class TallyHookVchd(ILogger<TallyHookVchd> logger,
    ITelegramBotClient botClient,
    IConfiguration configuration,
    IWebHostEnvironment hostingEnvironment)
{

    [Function("TallyHookVchd")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("C# HTTP trigger function processing a request from Tally.so");

        string requestBody = await new StreamReader(request.Body).ReadToEndAsync(cancellationToken);
        logger.LogInformation("Request body: {RequestBody}", requestBody);

        TallyResponse? tallyResponse;
        try
        {
            tallyResponse = JsonSerializer.Deserialize<TallyResponse>(requestBody);
            if (tallyResponse == null)
            {
                logger.LogWarning("Deserialization failed, result is null");
                return new BadRequestObjectResult("Invalid request body");
            }

            var bot = await botClient.GetMeAsync(cancellationToken);
            logger.LogInformation("Bot name: {BotName} trying to send message", bot.Username);

            var chatId = configuration.GetValue<long>("Bot:VchdReports:GroupId");
            if (chatId == 0)
            {
                logger.LogError("ChatId is not configured correctly");
                return new StatusCodeResult(500);
            }

            var message = await botClient.SendTextMessageAsync(
                chatId,
                FormatReport(tallyResponse.Data),
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken);
            
            // if (hostingEnvironment.IsDevelopment())
            // {
            //     await Task.Delay(100000, cancellationToken);
            //     await botClient.DeleteMessageAsync(chatId, message.MessageId, cancellationToken: cancellationToken);
            // }
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error deserializing Tally request: {Message}", ex.Message);
            return new BadRequestObjectResult("Invalid request body");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Tally request: {Message}", ex.Message);
            return new StatusCodeResult(500);
        }
        return new OkObjectResult("So'rovnoma yuborildi!");
    }

    private string FormatReport(TallyResponseData data)
    {
        return $@"
🚂 <b>Kunlik Ta'mirlangan Vagonlar Hisoboti</b> 🚂

🏭 <b>Korxona:</b> {GetFieldValue(data, "Korxona nomi")}

📥 <b>Ta'mirga olingan vagonlar:</b>
    • Jami: {GetFieldValue(data, "Nechta yuk vagoni ta'mirga olindi")} ta
    • DR: {GetFieldValue(data, "Ta'mirga olinganlardan nechtasi DR")} ta
    • KR: {GetFieldValue(data, "Ta'mirga olinganlardan nechtasi KR")} ta
    • KRP: {GetFieldValue(data, "Ta'mirga olinganlardan nechtasi KRP")} ta

📤 <b>Ta'mirlangan vagonlar:</b>
    • Jami: {GetFieldValue(data, "Nechta yuk vagoni ta'mirlandi")} ta
    • DR: {GetFieldValue(data, "Ta'mirlanganlardan nechtasi DR")} ta
    • KR: {GetFieldValue(data, "Ta'mirlanganlardan nechtasi KR")} ta
    • KRP: {GetFieldValue(data, "Ta'mirlanganlardan nechtasi KRP")} ta

💬 <b>Qo'shimcha izohlar:</b>
{GetFieldValue(data, "Qo'shimcha izoh")}

🕒 <i>Hisobot vaqti: {DateTime.Now:dd.MM.yyyy HH:mm}</i>";

    string GetFieldValue(TallyResponseData data, string label) =>
        data.Fields.FirstOrDefault(f => f.Label?.Equals(label, StringComparison.InvariantCultureIgnoreCase) == true)?.Value?.ToString() ?? "N/A";
    }
}
