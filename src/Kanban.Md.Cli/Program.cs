using System.Globalization;

namespace Kanban.Md.Cli;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
            return 1;
        }

        return args[0] switch
        {
            "--help" or "-h" or "help" => RunHelp(),
            "--version" or "-v" or "version" => RunVersion(),
            "serve" => Serve(args[1..]),
            _ => Unknown(args[0]),
        };
    }

    private static int RunHelp()
    {
        PrintHelp();
        return 0;
    }

    private static int RunVersion()
    {
        var version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0";
        Console.WriteLine($"kanban {version}");
        return 0;
    }

    private static int Unknown(string token)
    {
        Console.Error.WriteLine($"kanban: unknown command '{token}'.");
        Console.Error.WriteLine();
        PrintHelp();
        return 2;
    }

    private static int Serve(string[] args)
    {
        string? tasksPath = null;
        int? port = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--tasks-path" or "-p":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("kanban serve: --tasks-path requires a value.");
                        return 2;
                    }
                    tasksPath = args[++i];
                    break;

                case "--port":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("kanban serve: --port requires a value.");
                        return 2;
                    }
                    if (!int.TryParse(args[i + 1], CultureInfo.InvariantCulture, out var parsed))
                    {
                        Console.Error.WriteLine("kanban serve: --port must be an integer.");
                        return 2;
                    }
                    port = parsed;
                    i++;
                    break;

                case "--help" or "-h":
                    PrintServeHelp();
                    return 0;

                default:
                    Console.Error.WriteLine($"kanban serve: unknown option '{args[i]}'.");
                    return 2;
            }
        }

        var hostArgs = new List<string>();
        if (tasksPath is not null)
        {
            hostArgs.Add("--KanbanMd:TasksPath");
            hostArgs.Add(tasksPath);
        }
        if (port is not null)
        {
            hostArgs.Add("--urls");
            hostArgs.Add($"http://0.0.0.0:{port.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        Kanban.Md.App.Program.Run(hostArgs.ToArray());
        return 0;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("kanban — markdown-driven Kanban board");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  kanban serve [--tasks-path <dir>] [--port <n>]");
        Console.WriteLine("  kanban version");
        Console.WriteLine("  kanban help");
        Console.WriteLine();
        Console.WriteLine("Run 'kanban serve --help' for serve options.");
    }

    private static void PrintServeHelp()
    {
        Console.WriteLine("Usage: kanban serve [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -p, --tasks-path <dir>   Directory containing *.md task files.");
        Console.WriteLine("                           Default: ./tasks");
        Console.WriteLine("      --port <n>           HTTP port to listen on. Default: 8090");
        Console.WriteLine("  -h, --help               Show this help.");
    }
}
