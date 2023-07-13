namespace Recipi_API.Models
{
    /// <summary>
    /// > 0 signifies block status = true
    /// 2 signifies user has initiated block
    /// </summary>
    public enum BlockStatus
    {
        None = 0,
        Blocked = 1,
        Blocking = 2, 
        Both = 2
    }
}
