using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public class PostReportData
{
    public int PostId { get; set; }

    public int UserId { get; set; }

    public string Message { get; set; } = null!;
}
