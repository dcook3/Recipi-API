using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class PostReport
{
    public int PostReportId { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    public string Message { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime ReportedDatetime { get; set; }

    public virtual Post Post { get; set; } = null!;
}
