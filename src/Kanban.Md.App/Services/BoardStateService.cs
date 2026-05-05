using Kanban.Md.App.Models;
using Microsoft.Extensions.Logging;

namespace Kanban.Md.App.Services;

/// Single source of truth for the board's current task list.
/// Wraps <see cref="TaskRepository"/> with cached state and a Changed event
/// so subscribers (Blazor components, file watcher) can stay in sync.
public sealed class BoardStateService
{
    private readonly TaskRepository _repository;
    private readonly ILogger<BoardStateService>? _logger;
    private readonly object _lock = new();
    private IReadOnlyList<KanbanTask> _current;

    public BoardStateService(TaskRepository repository, ILogger<BoardStateService>? logger = null)
    {
        _repository = repository;
        _logger = logger;
        _current = _repository.LoadAll();
    }

    /// Snapshot of the current board state. Cheap to read; safe to call on
    /// any thread.
    public IReadOnlyList<KanbanTask> Current => _current;

    /// Raised after Current has been updated. Handlers must not throw — any
    /// exception they raise is logged and swallowed so we never poison the
    /// caller (which may be the file watcher running on a thread-pool thread).
    public event Action? Changed;

    /// Persist <paramref name="task"/> via the repository, then refresh state.
    public void Save(KanbanTask task)
    {
        lock (_lock)
        {
            _repository.Save(task);
            _current = _repository.LoadAll();
        }
        RaiseChanged();
    }

    /// Re-read the tasks directory from disk. Called by the file watcher when
    /// the directory contents change externally.
    public void Refresh()
    {
        lock (_lock)
        {
            _current = _repository.LoadAll();
        }
        RaiseChanged();
    }

    private void RaiseChanged()
    {
        var handler = Changed;
        if (handler is null)
        {
            return;
        }

        // Invoke each delegate individually so one bad subscriber can't
        // prevent the others from running.
        foreach (var subscriber in handler.GetInvocationList())
        {
            try
            {
                ((Action)subscriber)();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "BoardStateService.Changed subscriber threw.");
            }
        }
    }
}
