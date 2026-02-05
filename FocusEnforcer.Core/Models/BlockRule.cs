namespace FocusEnforcer.Core.Models;

public enum BlockType
{
    Process,
    Website,
    Keyword
}

public enum RuleBehavior
{
    Block,
    Allow
}

public class BlockRule
{
    public int Id { get; set; }
    public BlockType Type { get; set; }
    public RuleBehavior Behavior { get; set; } = RuleBehavior.Block; // New Property
    public string Value { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
