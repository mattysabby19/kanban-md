using Kanban.Md.App.Models;

namespace Kanban.Md.App.Services;

public sealed class TaskRepository
{
    private readonly MarkdownFrontMatterParser _parser;
    private readonly TaskRepositoryOptions _options;

    public TaskRepository(MarkdownFrontMatterParser parser, TaskRepositoryOptions options)
    {
        _parser = parser;
        _options = options;
    }

    public IReadOnlyList<KanbanTask> LoadAll()
    {
        if (!Directory.Exists(_options.Directory))
        {
            return Array.Empty<KanbanTask>();
        }

        return Directory
            .EnumerateFiles(_options.Directory, "*.md", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => _parser.Parse(File.ReadAllText(path)))
            .ToList();
    }
}
