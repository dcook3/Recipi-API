using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class UserRole
{
    public int RoleId { get; set; }

    public int UserId { get; set; }

    public DateTime GrantedDatetime { get; set; }

    public int ExpirationDays { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
