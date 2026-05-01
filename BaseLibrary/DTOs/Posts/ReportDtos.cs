using System;

namespace BaseLibrary.DTOs.Posts;

public class ReportReasonDto
{
    public Guid Id { get; set; }
    public string ReasonTextAr { get; set; } = string.Empty;
    public string ReasonTextFr { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class CreateReportReasonDto
{
    public string ReasonTextAr { get; set; } = string.Empty;
    public string ReasonTextFr { get; set; } = string.Empty;
}

public class SubmitReportDto
{
    public Guid ReasonId { get; set; }
    public string? AdditionalComments { get; set; }
}
