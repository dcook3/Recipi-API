using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class BugReport
{
    public int BugReportId { get; set; }

    public int UserId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime ReportedDatetime { get; set; }

    public string? Status { get; set; }

    public virtual User User { get; set; } = null!;
}
