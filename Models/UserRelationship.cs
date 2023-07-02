using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class UserRelationship
{
    public int InitiatingUserId { get; set; }

    public int ReceivingUserId { get; set; }

    public string Relationship { get; set; } = null!;

    public DateTime InitiatedDatetime { get; set; }

    public virtual User InitiatingUser { get; set; } = null!;

    public virtual User ReceivingUser { get; set; } = null!;
}
