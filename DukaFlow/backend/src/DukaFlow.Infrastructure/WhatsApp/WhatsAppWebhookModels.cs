using System.Text.Json.Serialization;

namespace DukaFlow.Infrastructure.WhatsApp;

// Minimal subset of Meta's WhatsApp Cloud API webhook payload - only the
// fields DukaFlow actually reads. Extend as needed (media, interactive
// replies, status callbacks) rather than modeling the whole schema
// upfront. Verify field names against developers.facebook.com/docs/whatsapp
// before relying on anything not already here - Meta's payload schema is
// out of DukaFlow's control. See Phase 4 CLAUDE.md #4.
public class WhatsAppWebhookPayload
{
    [JsonPropertyName("entry")]
    public List<WhatsAppEntry>? Entry { get; set; }
}

public class WhatsAppEntry
{
    [JsonPropertyName("changes")]
    public List<WhatsAppChange>? Changes { get; set; }
}

public class WhatsAppChange
{
    [JsonPropertyName("value")]
    public WhatsAppChangeValue? Value { get; set; }
}

public class WhatsAppChangeValue
{
    [JsonPropertyName("metadata")]
    public WhatsAppMetadata? Metadata { get; set; }

    [JsonPropertyName("messages")]
    public List<WhatsAppInboundMessage>? Messages { get; set; }

    // Present on delivery/read status callbacks instead of "messages" -
    // checked for but not processed in Phase 4 (see CLAUDE.md #22, which
    // is about outbound status updates, not inbound ones).
    [JsonPropertyName("statuses")]
    public List<object>? Statuses { get; set; }
}

public class WhatsAppMetadata
{
    [JsonPropertyName("phone_number_id")]
    public string? PhoneNumberId { get; set; }

    [JsonPropertyName("display_phone_number")]
    public string? DisplayPhoneNumber { get; set; }
}

public class WhatsAppInboundMessage
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("text")]
    public WhatsAppTextBody? Text { get; set; }

    [JsonPropertyName("interactive")]
    public WhatsAppInteractiveReply? Interactive { get; set; }

    [JsonPropertyName("button")]
    public WhatsAppButtonReply? Button { get; set; }
}

public class WhatsAppTextBody
{
    [JsonPropertyName("body")]
    public string? Body { get; set; }
}

public class WhatsAppButtonReply
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class WhatsAppInteractiveReply
{
    [JsonPropertyName("button_reply")]
    public WhatsAppButtonReplyDetail? ButtonReply { get; set; }

    [JsonPropertyName("list_reply")]
    public WhatsAppButtonReplyDetail? ListReply { get; set; }
}

public class WhatsAppButtonReplyDetail
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}
