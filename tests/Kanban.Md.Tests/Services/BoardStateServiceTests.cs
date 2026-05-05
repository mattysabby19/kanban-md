namespace Kanban.Md.Tests.Services;

public sealed class BoardStateServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly TaskRepository _repo;

    public BoardStateServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "kanban-md-state-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _repo = new TaskRepository(
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
    public void Current_AfterConstruction_ReflectsDisk()
    {
        WriteTask("a.md", "A-1", KanbanStatus.Todo);

        var sut = new BoardStateService(_repo);

        Assert.Single(sut.Current);
        Assert.Equal("A-1", sut.Current[0].Id);
    }

    [Fact]
    public void Save_PersistsAndUpdatesCurrent()
    {
        WriteTask("a.md", "A-1", KanbanStatus.Todo);
        var sut = new BoardStateService(_repo);
        var task = sut.Current[0];

        sut.Save(task with { Status = KanbanStatus.Done });

        Assert.Equal(KanbanStatus.Done, sut.Current[0].Status);
    }

    [Fact]
    public void Save_RaisesChangedEvent()
    {
        WriteTask("a.md", "A-1", KanbanStatus.Todo);
        var sut = new BoardStateService(_repo);
        var raised = 0;
        sut.Changed += () => raised++;

        sut.Save(sut.Current[0] with { Status = KanbanStatus.InProgress });

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Refresh_RereadsDisk_AndRaisesChangedEvent()
    {
        WriteTask("a.md", "A-1", KanbanStatus.Todo);
        var sut = new BoardStateService(_repo);
        var raised = 0;
        sut.Changed += () => raised++;

        // Mutate disk under the service's feet, then ask it to refresh.
        WriteTask("b.md", "A-2", KanbanStatus.Done);
        sut.Refresh();

        Assert.Equal(2, sut.Current.Count);
        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_HandlerThatThrows_DoesNotPropagateToCaller()
    {
        WriteTask("a.md", "A-1", KanbanStatus.Todo);
        var sut = new BoardStateService(_repo);
        sut.Changed += () => throw new InvalidOperationException("subscriber blew up");

        // Caller (e.g. file watcher) should not have to deal with subscriber faults.
        Action act = () => sut.Refresh();
        var ex = Record.Exception(act);
        Assert.Null(ex);
    }

    private void WriteTask(string filename, string id, KanbanStatus status)
    {
        var content = $"""
            ---
            schema: 1
            id: {id}
            title: t
            status: {status}
            epic: E
            priority: P0
            effort: S
            created: 2026-05-04
            updated: 2026-05-04
            ---

            body
            """;
        File.WriteAllText(Path.Combine(_tempDir, filename), content);
    }
}
