namespace Kanban.Md.Tests.Services;

public sealed class TaskRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly TaskRepository _sut;

    public TaskRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "kanban-md-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _sut = new TaskRepository(
            new MarkdownFrontMatterParser(),
            new TaskRepositoryOptions(_tempDir));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public void LoadAll_FromEmptyDirectory_ReturnsEmptyList()
    {
        var tasks = _sut.LoadAll();
        Assert.Empty(tasks);
    }

    [Fact]
    public void LoadAll_FromDirectoryWithTaskFiles_ReturnsAllParsedTasks()
    {
        WriteTask("a.md", "A-1", "First task", KanbanStatus.Todo);
        WriteTask("b.md", "A-2", "Second task", KanbanStatus.InProgress);
        WriteTask("c.md", "A-3", "Third task", KanbanStatus.Done);

        var tasks = _sut.LoadAll();

        Assert.Equal(3, tasks.Count);
        Assert.Contains(tasks, t => t.Id == "A-1");
        Assert.Contains(tasks, t => t.Id == "A-2");
        Assert.Contains(tasks, t => t.Id == "A-3");
    }

    [Fact]
    public void LoadAll_SkipsNonMarkdownFiles()
    {
        WriteTask("real.md", "A-1", "Real task", KanbanStatus.Todo);
        File.WriteAllText(Path.Combine(_tempDir, "readme.txt"), "ignore me");
        File.WriteAllText(Path.Combine(_tempDir, "config.yml"), "also ignore");

        var tasks = _sut.LoadAll();

        Assert.Single(tasks);
        Assert.Equal("A-1", tasks[0].Id);
    }

    [Fact]
    public void LoadAll_WhenDirectoryDoesNotExist_ReturnsEmptyList()
    {
        var missing = Path.Combine(_tempDir, "does-not-exist");
        var sut = new TaskRepository(
            new MarkdownFrontMatterParser(),
            new TaskRepositoryOptions(missing));

        var tasks = sut.LoadAll();

        Assert.Empty(tasks);
    }

    [Fact]
    public void LoadAll_OrderIsStableBetweenCalls()
    {
        WriteTask("c.md", "A-3", "Third", KanbanStatus.Todo);
        WriteTask("a.md", "A-1", "First", KanbanStatus.Todo);
        WriteTask("b.md", "A-2", "Second", KanbanStatus.Todo);

        var first = _sut.LoadAll().Select(t => t.Id).ToList();
        var second = _sut.LoadAll().Select(t => t.Id).ToList();

        Assert.Equal(first, second);
    }

    private void WriteTask(string filename, string id, string title, KanbanStatus status)
    {
        var content = $"""
            ---
            schema: 1
            id: {id}
            title: {title}
            status: {status}
            epic: E1
            priority: P1
            effort: S
            created: 2026-05-04
            updated: 2026-05-04
            ---

            Body for {id}.
            """;
        File.WriteAllText(Path.Combine(_tempDir, filename), content);
    }
}
