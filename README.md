# kanban-md

A Kanban board for projects whose tasks live as markdown files in git.

- **Source of truth**: `tasks/*.md` files with YAML front-matter. No database.
- **Stack**: .NET 8, Blazor Server, distributed as a Docker image.
- **Designed for**: solo founders and small teams who want their work tracker to live next to their code, version-controlled, offline-capable, and reusable across projects.

> Status: **0.1.0 in progress** — early bootstrap. See `CHANGELOG.md`.

## Run it

The board reads `*.md` task files from a directory you mount into the
container at `/data`. A working sample lives in `samples/minimal/tasks/`.

```bash
docker run --rm -p 8090:8090 \
  -v "$(pwd)/samples/minimal/tasks:/data" \
  ghcr.io/mattysabby19/kanban-md:latest
```

Then open http://localhost:8090.

PowerShell equivalent:

```powershell
docker run --rm -p 8090:8090 `
  -v "${PWD}/samples/minimal/tasks:/data" `
  ghcr.io/mattysabby19/kanban-md:latest
```

### Drag-drop persistence

Dragging a card across columns rewrites the corresponding `.md` file's
`status:` front-matter atomically and updates `updated:` to today's date.
The change is visible to git immediately.

### Configuration

The container accepts these settings (env var or command-line flag):

| Setting | Env var | Flag | Default |
|---|---|---|---|
| Tasks directory | `KanbanMd__TasksPath` | `--KanbanMd:TasksPath` | `/data` |
| HTTP URLs | `ASPNETCORE_URLS` | `--urls` | `http://0.0.0.0:8090` |

The container exposes a health probe at `/healthz` and runs as a non-root
user (uid `app`).

## Run from source

```bash
dotnet run --project src/Kanban.Md.App -- \
  --KanbanMd:TasksPath ./samples/minimal/tasks
```

A CLI wrapper (`Kanban.Md.Cli`) is also in the repo and exposes
`kanban serve --tasks-path <dir> --port <n>`. It is **not** currently
published as a `dotnet tool` — see the "Known issues" section of
`CHANGELOG.md` for the static-web-asset packaging gap.

## Quick links

- Getting started: *coming soon* (`docs/getting-started.md`)
- Task schema: *coming soon* (`docs/task-schema.md`)
- Configuration: *coming soon* (`docs/configuration.md`)

## License

MIT — see `LICENSE`.
