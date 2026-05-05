using Microsoft.Extensions.Logging.Abstractions;

namespace Kanban.Md.Tests.Services;

/// Integration-style tests for the file watcher. They use the real FileSystemWatcher
/// against a temp directory, so they're inherently a little slow (FSW startup +
/// debounce). Each waits up to ~3s for the expected change to propagate.
[Trait("Category", "Integration")]
public sealed class TaskFileWatcherTests : IAsyncLifetime
{
    private string _tempDir = string.Empty;
    private TaskRepository _repo = null!;
    private BoardStateService _state = null!;
    private TaskFileWatcher _watcher = null!;

    public async Task InitializeAsync()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "kanban-md-watcher-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        var options = new TaskRepositoryOptions(_tempDir);
        _repo = new TaskRepository(new MarkdownFrontMatterParser(), options);
        _state = new BoardStateService(_repo);
        _watcher = new TaskFileWatcher(_state, options, NullLogger<TaskFileWatcher>.Instance);

        await _watcher.StartAsync(CancellationToken.None);
    }

    public async Task DisposeAsync()
    {
        await _watcher.StopAsync(CancellationToken.None);
        _watcher.Dispose();
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task NewlyDroppedTaskFile_TriggersStateRefresh()
    {
        var changedSignal = new TaskCompletionSource();
        _state.Changed += () => changedSignal.TrySetResult();

        WriteTask("a.md", "A-1");

        await WaitForOrTimeout(changedSignal.Task, TimeSpan.FromSeconds(3));

        Assert.Single(_state.Current);
        Assert.Equal("A-1", _state.Current[0].Id);
    }

    [Fact]
    public async Task DeletedTaskFile_TriggersStateRefresh()
    {
        WriteTask("a.md", "A-1");
        WriteTask("b.md", "A-2");
        _state.Refresh();
        Assert.Equal(2, _state.Current.Count);

        var changedSignal = new TaskCompletionSource();
        _state.Changed += () => changedSignal.TrySetResult();

        File.Delete(Path.Combine(_tempDir, "b.md"));

        await WaitForOrTimeout(changedSignal.Task, TimeSpan.FromSeconds(3));

        Assert.Single(_state.Current);
        Assert.Equal("A-1", _state.Current[0].Id);
    }

    private static async Task WaitForOrTimeout(Task task, TimeSpan timeout)
    {
        var completed = await Task.WhenAny(task, Task.Delay(timeout));
        if (completed != task)
        {
            throw new TimeoutException($"Expected event did not arrive within {timeout.TotalMilliseconds}ms.");
        }
    }

    private void WriteTask(string filename, string id)
    {
        var content = $"""
            ---
            schema: 1
            id: {id}
            title: t
            status: Todo
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
