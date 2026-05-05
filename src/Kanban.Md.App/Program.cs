using Kanban.Md.App.Components;
using Kanban.Md.App.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var tasksPath = builder.Configuration["KanbanMd:TasksPath"]
    ?? Path.Combine("..", "..", "samples", "minimal", "tasks");

builder.Services.AddSingleton(new TaskRepositoryOptions(tasksPath));
builder.Services.AddSingleton<MarkdownFrontMatterParser>();
builder.Services.AddSingleton<TaskRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
