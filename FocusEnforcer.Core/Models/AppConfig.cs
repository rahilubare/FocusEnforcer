namespace FocusEnforcer.Core.Models;

public class AppConfig
{
    public int Id { get; set; }
    public string PasswordHash { get; set; } = string.Empty; // For accessing UI settings
    public bool IsFrozenTurkeyEnabled { get; set; }
    public int UnlockDifficultyLevel { get; set; } // 0-10, size of random text to type
    public bool RequireRestartToUnlock { get; set; }
}
