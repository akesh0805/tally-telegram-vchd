using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Tally.Hooks
{
    public class TallyHookVchd(ILogger<TallyHookVchd> logger)
    {

        [Function("TallyHookVchd")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest request)
        {
            logger.LogInformation("C# HTTP trigger function processing a request from Tally.so");

            // Read the request body
            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();

            // Deserialize the JSON payload
            var tallyResponse = JsonSerializer.Deserialize<TallyResponse>(requestBody);

            if (tallyResponse == null)
            {
                return new BadRequestObjectResult("Invalid request body");
            }

            // Extract and process the data
            var formData = new FormData
            {
                Company = GetFieldValue(tallyResponse, "question_MXq5El"),
                VagonsInRepair = ParseIntSafely(GetFieldValue(tallyResponse, "question_Jqx0Or")),
                VagonsDR = ParseIntSafely(GetFieldValue(tallyResponse, "question_ga149P")),
                VagonsKR = ParseIntSafely(GetFieldValue(tallyResponse, "question_yMRDJ8")),
                VagonsKRP = ParseIntSafely(GetFieldValue(tallyResponse, "question_XLkq4P")),
                VagonsRepaired = ParseIntSafely(GetFieldValue(tallyResponse, "question_8qyeZl")),
                VagonsRepairedDR = ParseIntSafely(GetFieldValue(tallyResponse, "question_0dyPe9")),
                VagonsRepairedKR = ParseIntSafely(GetFieldValue(tallyResponse, "question_zjoe7k")),
                VagonsRepairedKRP = ParseIntSafely(GetFieldValue(tallyResponse, "question_5bD2ZN")),
                AdditionalNotes = GetFieldValue(tallyResponse, "question_dE1P9r")
            };

            // TODO: Process the formData (e.g., save to database, send notifications, etc.)

            return new OkObjectResult("So'rovnoma yuborildi!");
        }

        private string GetFieldValue(TallyResponse response, string key)
        {
            var field = response.Data?.Fields?.FirstOrDefault(f => f.Key == key);
            return field?.Value?.ToString() ?? string.Empty;
        }

        private int ParseIntSafely(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0; // или другое значение по умолчанию
            }

            if (int.TryParse(value, out int result))
            {
                return result;
            }

            return 0; // или другое значение по умолчанию
        }
    }

    // Define classes to deserialize the JSON payload
    public class TallyResponse
    {
        public string? EventId { get; set; }
        public string? EventType { get; set; }
        public DateTime CreatedAt { get; set; }
        public TallyData? Data { get; set; }
    }

    public class TallyData
    {
        public string? ResponseId { get; set; }
        public string? SubmissionId { get; set; }
        public string? RespondentId { get; set; }
        public string? FormId { get; set; }
        public string? FormName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<TallyField>? Fields { get; set; }
    }

    public class TallyField
    {
        public string? Key { get; set; }
        public string? Label { get; set; }
        public string? Type { get; set; }
        public object? Value { get; set; }
        public List<TallyOption>? Options { get; set; }
    }

    public class TallyOption
    {
        public string? Id { get; set; }
        public string? Text { get; set; }
    }

    public class FormData
    {
        public string? Company { get; set; }
        public int VagonsInRepair { get; set; }
        public int VagonsDR { get; set; }
        public int VagonsKR { get; set; }
        public int VagonsKRP { get; set; }
        public int VagonsRepaired { get; set; }
        public int VagonsRepairedDR { get; set; }
        public int VagonsRepairedKR { get; set; }
        public int VagonsRepairedKRP { get; set; }
        public string? AdditionalNotes { get; set; }
    }
}
