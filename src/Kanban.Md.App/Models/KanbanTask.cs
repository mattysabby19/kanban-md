namespace Kanban.Md.App.Models;

/// One task as parsed from a markdown file with YAML front-matter.
/// Round-trippable: <see cref="ExtraFields"/> captures unknown front-matter
/// keys so they survive a parse → write cycle.
public sealed record KanbanTask
{
    public required int Schema { get; init; }
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required KanbanStatus Status { get; init; }
    public required string Epic { get; init; }
    public required Priority Priority { get; init; }
    public required Effort Effort { get; init; }
    public string? Assignee { get; init; }
    public IReadOnlyList<string> Labels { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();
    public DateOnly Created { get; init; }
    public DateOnly Updated { get; init; }

    /// Markdown body after the closing `---` delimiter, emitted verbatim on write.
    public string Body { get; init; } = string.Empty;

    /// Any front-matter key not mapped above, preserved for forward compatibility.
    public IReadOnlyDictionary<string, string> ExtraFields { get; init; }
        = new Dictionary<string, string>();
}
