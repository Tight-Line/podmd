namespace PodMD.Api.Dtos;

public class AnalysisResponseDto
{
    public bool Success { get; set; }
    public object? Data { get; set; }
    public string? Message { get; set; }
    public string ResultFormat { get; set; } = "Default";
}

public class AnalysisDataDto
{
    public List<LogErrorDto> Errors { get; set; } = new();
}

public class LogErrorDto
{
    public string Description { get; set; } = string.Empty;
    public List<string> Occurrences { get; set; } = new();
    public List<SolutionDto> Solutions { get; set; } = new();
}

public class SolutionDto
{
    public string Description { get; set; } = string.Empty;
    public List<StepDto> Steps { get; set; } = new();
}

public class StepDto
{
    public string Title { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
}
