using Kanban.Md.App.Models;

namespace Kanban.Md.App.Services;

public sealed class TaskRepository
{
    private readonly MarkdownFrontMatterParser _parser;
    private readonly TaskRepositoryOptions _options;
    private readonly Dictionary<string, string> _idToPath = new(StringComparer.Ordinal);

    public TaskRepository(MarkdownFrontMatterParser parser, TaskRepositoryOptions options)
    {
        _parser = parser;
        _options = options;
    }

    public IReadOnlyList<KanbanTask> LoadAll()
    {
        _idToPath.Clear();

        if (!Directory.Exists(_options.Directory))
        {
            return Array.Empty<KanbanTask>();
        }

        var tasks = new List<KanbanTask>();
        foreach (var path in Directory
            .EnumerateFiles(_options.Directory, "*.md", SearchOption.TopDirectoryOnly)
            .OrderBy(p => p, StringComparer.Ordinal))
        {
            var task = _parser.Parse(File.ReadAllText(path));
            tasks.Add(task);
            _idToPath[task.Id] = path;
        }
        return tasks;
    }

    /// Persists <paramref name="task"/> back to the file it was loaded from.
    /// Atomic: writes to a sibling .tmp file then renames over the original
    /// so a crash mid-write cannot leave a partial file.
    public void Save(KanbanTask task)
    {
        if (!_idToPath.TryGetValue(task.Id, out var path))
        {
            throw new InvalidOperationException(
                $"Task '{task.Id}' has no known file path. Call LoadAll first " +
                "and only Save tasks that came from this repository.");
        }

        var content = _parser.Serialize(task);
        var tmp = path + ".tmp";

        try
        {
            File.WriteAllText(tmp, content);
            File.Move(tmp, path, overwrite: true);
        }
        catch
        {
            if (File.Exists(tmp))
            {
                try { File.Delete(tmp); } catch { /* best-effort cleanup */ }
            }
            throw;
        }
    }
}
