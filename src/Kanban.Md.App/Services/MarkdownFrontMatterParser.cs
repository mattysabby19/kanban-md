using System.Globalization;
using Kanban.Md.App.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Kanban.Md.App.Services;

public class MarkdownFrontMatterParser
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly HashSet<string> KnownKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "schema", "id", "title", "status", "epic", "priority", "effort",
        "assignee", "labels", "dependencies", "created", "updated",
    };

    public KanbanTask Parse(string raw)
    {
        var (frontMatterText, body) = SplitFrontMatter(raw);

        var dto = Deserializer.Deserialize<FrontMatterDto>(frontMatterText)
            ?? throw new FormatException("Front-matter is empty.");

        var allKeys = Deserializer.Deserialize<Dictionary<string, object?>>(frontMatterText)
            ?? new Dictionary<string, object?>();

        var extras = allKeys
            .Where(kvp => !KnownKeys.Contains(kvp.Key))
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.ToString() ?? string.Empty,
                StringComparer.Ordinal);

        return new KanbanTask
        {
            Schema = dto.Schema,
            Id = Required(dto.Id, "id"),
            Title = Required(dto.Title, "title"),
            Status = Enum.Parse<KanbanStatus>(Required(dto.Status, "status"), ignoreCase: true),
            Epic = Required(dto.Epic, "epic"),
            Priority = Enum.Parse<Priority>(Required(dto.Priority, "priority"), ignoreCase: true),
            Effort = Enum.Parse<Effort>(Required(dto.Effort, "effort"), ignoreCase: true),
            Assignee = dto.Assignee,
            Labels = dto.Labels ?? new List<string>(),
            Dependencies = dto.Dependencies ?? new List<string>(),
            Created = DateOnly.Parse(Required(dto.Created, "created"), CultureInfo.InvariantCulture),
            Updated = DateOnly.Parse(Required(dto.Updated, "updated"), CultureInfo.InvariantCulture),
            Body = body,
            ExtraFields = extras,
        };
    }

    private static (string FrontMatter, string Body) SplitFrontMatter(string raw)
    {
        var normalized = raw.Replace("\r\n", "\n");
        if (!normalized.StartsWith("---\n", StringComparison.Ordinal))
        {
            throw new FormatException("File must start with '---' on the first line.");
        }

        var endIndex = normalized.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        if (endIndex < 0)
        {
            throw new FormatException("Front-matter is not closed by '---'.");
        }

        var frontMatter = normalized[4..endIndex];
        var bodyStart = endIndex + 5;
        var body = bodyStart < normalized.Length ? normalized[bodyStart..] : string.Empty;
        return (frontMatter, body);
    }

    private static string Required(string? value, string field) =>
        value ?? throw new FormatException($"Missing required field: '{field}'.");

    private sealed class FrontMatterDto
    {
        public int Schema { get; set; }
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Status { get; set; }
        public string? Epic { get; set; }
        public string? Priority { get; set; }
        public string? Effort { get; set; }
        public string? Assignee { get; set; }
        public List<string>? Labels { get; set; }
        public List<string>? Dependencies { get; set; }
        public string? Created { get; set; }
        public string? Updated { get; set; }
    }
}
