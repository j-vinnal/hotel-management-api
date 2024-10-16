using System.Text;
using System.Text.Json;

namespace WebApp.Middleware;

public class DateTimeMiddleware
{
    private readonly RequestDelegate _next;

    public DateTimeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Process the request body to remove time from DateTime
        if (context.Request.ContentType != null && context.Request.ContentType.Contains("application/json"))
        {
            context.Request.EnableBuffering();
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            // Parse JSON and remove time from DateTime fields
            var jsonObject = JsonDocument.Parse(body);
            var modifiedJson = RemoveTimeFromDateTime(jsonObject.RootElement);

            // Replace the request body with the modified JSON
            var modifiedBody = new MemoryStream(Encoding.UTF8.GetBytes(modifiedJson));
            context.Request.Body = modifiedBody;
            context.Request.Body.Position = 0;
        }

        await _next(context);
    }

    private string RemoveTimeFromDateTime(JsonElement element)
    {
        // If the element is not an object, return the original JSON
        if (element.ValueKind != JsonValueKind.Object)
        {
            return element.GetRawText();
        }

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();

            foreach (var property in element.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String && DateTime.TryParse(property.Value.GetString(), out var dateTime))
                {
                    // Convert to date only
                    var dateOnly = dateTime.Date;
                    writer.WriteString(property.Name, dateOnly.ToString("yyyy-MM-dd"));
                }
                else
                {
                    property.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
