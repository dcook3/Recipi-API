using System;
using System.Collections.Generic;

namespace Recipi_API.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int SendingUserId { get; set; }

    public int ConversationId { get; set; }

    public string MessageContents { get; set; } = null!;

    public DateTime SentDatetime { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User SendingUser { get; set; } = null!;
}
