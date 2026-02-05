namespace FocusEnforcer.Core.Models;

public class Schedule
{
    public int Id { get; set; }
    public string Name { get; set; } = "Default Schedule";
    public bool IsEnabled { get; set; } = true;
    
    // Bitmask or List of days? Keeping it simple.
    public DayOfWeek DayOfWeek { get; set; }
    
    // Time range
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    
    // "Deep Work" mode options
    public bool IsStrict { get; set; } // If true, harder to unlock
}
