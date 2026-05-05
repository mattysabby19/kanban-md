namespace Kanban.Md.App.Models;

public sealed record Epic
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? Color { get; init; }
}
