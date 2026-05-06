using Kanban.Md.App.Services;

// Alias avoids the namespace/type clash between `Kanban.Md.App` (this namespace)
// and `Kanban.Md.App.Components.App` (the root Razor component).
using AppComponent = Kanban.Md.App.Components.App;

namespace Kanban.Md.App;

public static class Program
{
    public static void Main(string[] args) => Run(args);

    /// Boots the Blazor host. Exposed as public so the CLI tool
    /// (Kanban.Md.Cli) can launch the same binary with parsed arguments.
    public static void Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddHealthChecks();

        var tasksPath = builder.Configuration["KanbanMd:TasksPath"]
            ?? Path.Combine("..", "..", "samples", "minimal", "tasks");

        builder.Services.AddSingleton(new TaskRepositoryOptions(tasksPath));
        builder.Services.AddSingleton<MarkdownFrontMatterParser>();
        builder.Services.AddSingleton<TaskRepository>();
        builder.Services.AddSingleton<BoardStateService>();
        builder.Services.AddHostedService<TaskFileWatcher>();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        // No HTTPS redirect in production — the container expects to be
        // terminated by an upstream proxy. In development the launchSettings
        // profile already binds an HTTPS endpoint.
        if (app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapHealthChecks("/healthz");

        app.MapRazorComponents<AppComponent>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
