namespace Kanban.Md.Tests.Services;

public class MarkdownFrontMatterParserTests
{
    private readonly MarkdownFrontMatterParser _sut = new();

    private const string MinimalRaw = """
        ---
        schema: 1
        id: NGAP-0001
        title: Replace stub README
        status: Todo
        epic: E1-Hygiene
        priority: P0
        effort: S
        created: 2026-05-04
        updated: 2026-05-04
        ---

        Body.
        """;

    [Fact]
    public void Parse_WithCompleteValidTask_ReturnsAllFieldsPopulated()
    {
        var raw = """
            ---
            schema: 1
            id: NGAP-0001
            title: Replace stub README
            status: Todo
            epic: E1-Hygiene
            priority: P0
            effort: S
            assignee: founder
            labels: [docs, onboarding]
            dependencies: []
            created: 2026-05-04
            updated: 2026-05-04
            ---

            ## Description
            Body text.
            """;

        var task = _sut.Parse(raw);

        Assert.Equal(1, task.Schema);
        Assert.Equal("NGAP-0001", task.Id);
        Assert.Equal("Replace stub README", task.Title);
        Assert.Equal(KanbanStatus.Todo, task.Status);
        Assert.Equal("E1-Hygiene", task.Epic);
        Assert.Equal(Priority.P0, task.Priority);
        Assert.Equal(Effort.S, task.Effort);
        Assert.Equal("founder", task.Assignee);
        Assert.Equal(new[] { "docs", "onboarding" }, task.Labels);
        Assert.Empty(task.Dependencies);
        Assert.Equal(new DateOnly(2026, 5, 4), task.Created);
        Assert.Equal(new DateOnly(2026, 5, 4), task.Updated);
    }

    [Fact]
    public void Parse_PreservesBodyVerbatim()
    {
        var raw = """
            ---
            schema: 1
            id: T-1
            title: t
            status: Todo
            epic: E
            priority: P0
            effort: S
            created: 2026-05-04
            updated: 2026-05-04
            ---

            ## Heading

            Paragraph one with **bold** and `code`.

            - bullet 1
            - bullet 2

            ```csharp
            var x = 1;
            ```
            """;

        var task = _sut.Parse(raw);

        var expected = """

            ## Heading

            Paragraph one with **bold** and `code`.

            - bullet 1
            - bullet 2

            ```csharp
            var x = 1;
            ```
            """;
        Assert.Equal(expected, task.Body);
    }

    [Fact]
    public void Parse_WithMissingRequiredField_Throws()
    {
        var raw = """
            ---
            schema: 1
            id: T-1
            status: Todo
            epic: E
            priority: P0
            effort: S
            created: 2026-05-04
            updated: 2026-05-04
            ---

            (no title above)
            """;

        var ex = Assert.Throws<FormatException>(() => _sut.Parse(raw));
        Assert.Contains("title", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_WhenInputDoesNotStartWithDelimiter_Throws()
    {
        var raw = "no front matter\nbody";
        Assert.Throws<FormatException>(() => _sut.Parse(raw));
    }

    [Fact]
    public void Parse_WhenFrontMatterIsNotClosed_Throws()
    {
        var raw = "---\nid: T-1\nbody without closing";
        Assert.Throws<FormatException>(() => _sut.Parse(raw));
    }

    [Fact]
    public void Parse_WithoutOptionalAssignee_LeavesItNull()
    {
        var task = _sut.Parse(MinimalRaw);
        Assert.Null(task.Assignee);
    }

    [Fact]
    public void Parse_WithCrlfLineEndings_ParsesCorrectly()
    {
        var raw = MinimalRaw.Replace("\n", "\r\n", StringComparison.Ordinal);
        var task = _sut.Parse(raw);
        Assert.Equal("NGAP-0001", task.Id);
    }

    [Fact]
    public void Parse_PreservesUnknownFrontMatterKeys()
    {
        var raw = """
            ---
            schema: 1
            id: T-1
            title: t
            status: Todo
            epic: E
            priority: P0
            effort: S
            created: 2026-05-04
            updated: 2026-05-04
            sprint: 2026-Q2-S3
            estimated_hours: 4
            ---

            body
            """;

        var task = _sut.Parse(raw);

        Assert.Equal("2026-Q2-S3", task.ExtraFields["sprint"]);
        Assert.Equal("4", task.ExtraFields["estimated_hours"]);
    }
}
