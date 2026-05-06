# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0] - in progress

### Added

- Repo scaffold: solution, Blazor Server app, CLI tool wrapper, xUnit tests.
- MIT license, README, .editorconfig, Directory.Build.props.
- Multi-stage `Dockerfile` (KMD-0030): non-root `app` user, `/healthz`
  probe, mounts the consuming project's tasks directory at `/data`.
- `.github/workflows/release.yml` (KMD-0032): on `v*.*.*` tag, builds
  and pushes a multi-arch (amd64 + arm64) image to GHCR.

### Known issues

- **Global-tool packaging deferred (KMD-0031-followup).** The CLI runs
  fine as a normal .NET binary and inside the Docker image (KMD-0030),
  but `dotnet pack` does not include the App's wwwroot static assets
  (`app.css`, `kanban-dnd.js`, `Sortable.min.js`) nor Blazor Server's
  `_framework/blazor.web.js`, because those are only fully materialized
  by `dotnet publish`. The `<PackAsTool>` properties are gated behind
  `-p:EnableToolPack=true` until the publish-output → nupkg pipeline
  is wired. Docker is the supported distribution route for 0.1.0.
