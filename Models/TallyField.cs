namespace TallyTelegramVchd.Models;
using System.Text.Json.Serialization;

public class TallyField
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TallyFieldType Type { get; set; }

    [JsonPropertyName("value")]
    public object? Value { get; set; }

    // Удалено свойство Options, так как оно не используется в данном JSON
}

public class TallyResponse
{
    [JsonPropertyName("eventId")]
    public string? EventId { get; set; }

    [JsonPropertyName("eventType")]
    public string? EventType { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("data")]
    public TallyResponseData? Data { get; set; }
}

public class TallyResponseData
{
    [JsonPropertyName("responseId")]
    public string? ResponseId { get; set; }

    [JsonPropertyName("submissionId")]
    public string? SubmissionId { get; set; }

    [JsonPropertyName("respondentId")]
    public string? RespondentId { get; set; }

    [JsonPropertyName("formId")]
    public string? FormId { get; set; }

    [JsonPropertyName("formName")]
    public string? FormName { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("fields")]
    public List<TallyField>? Fields { get; set; }
}

public enum TallyFieldType
{
    INPUT_TEXT,
    TEXTAREA
    // Добавьте другие типы полей по мере необходимости
}
