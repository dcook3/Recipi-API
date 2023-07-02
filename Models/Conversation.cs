using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class Conversation
{
    public int ConversationId { get; set; }

    public int UserId1 { get; set; }

    public int UserId2 { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User UserId1Navigation { get; set; } = null!;

    public virtual User UserId2Navigation { get; set; } = null!;
}
