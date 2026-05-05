using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kanban.Md.App.Services;

/// Watches the tasks directory for *.md changes and triggers a board refresh.
/// Multiple events that arrive in a short window (e.g. an editor's
/// write-temp-then-rename pattern, or the temp+rename used by Save) are
/// collapsed into a single refresh via a 150 ms debounce timer.
public sealed class TaskFileWatcher : IHostedService, IDisposable
{
    private const int DebounceMilliseconds = 150;

    private readonly BoardStateService _state;
    private readonly TaskRepositoryOptions _options;
    private readonly ILogger<TaskFileWatcher> _logger;
    private readonly object _debounceLock = new();
    private FileSystemWatcher? _watcher;
    private Timer? _debounceTimer;
    private bool _disposed;

    public TaskFileWatcher(
        BoardStateService state,
        TaskRepositoryOptions options,
        ILogger<TaskFileWatcher> logger)
    {
        _state = state;
        _options = options;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_options.Directory))
        {
            _logger.LogWarning(
                "Tasks directory '{Directory}' does not exist; file watcher will not start.",
                _options.Directory);
            return Task.CompletedTask;
        }

        _watcher = new FileSystemWatcher(_options.Directory, "*.md")
        {
            NotifyFilter = NotifyFilters.LastWrite
                | NotifyFilters.FileName
                | NotifyFilters.CreationTime
                | NotifyFilters.Size,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true,
        };

        _watcher.Created += OnFileEvent;
        _watcher.Changed += OnFileEvent;
        _watcher.Deleted += OnFileEvent;
        _watcher.Renamed += OnFileEvent;
        _watcher.Error += OnWatcherError;

        _logger.LogInformation(
            "Watching '{Directory}' for *.md changes.",
            _options.Directory);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        ScheduleRefresh();
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        _logger.LogError(e.GetException(), "FileSystemWatcher reported an error.");
    }

    private void ScheduleRefresh()
    {
        lock (_debounceLock)
        {
            if (_disposed)
            {
                return;
            }
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(
                _ => SafeRefresh(),
                state: null,
                dueTime: DebounceMilliseconds,
                period: Timeout.Infinite);
        }
    }

    private void SafeRefresh()
    {
        try
        {
            _state.Refresh();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh after file change failed.");
        }
    }

    public void Dispose()
    {
        lock (_debounceLock)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            if (_watcher is not null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Created -= OnFileEvent;
                _watcher.Changed -= OnFileEvent;
                _watcher.Deleted -= OnFileEvent;
                _watcher.Renamed -= OnFileEvent;
                _watcher.Error -= OnWatcherError;
                _watcher.Dispose();
                _watcher = null;
            }

            _debounceTimer?.Dispose();
            _debounceTimer = null;
        }
    }
}
