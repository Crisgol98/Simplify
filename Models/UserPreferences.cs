namespace Simplify.Models
{
    public class UserPreferences
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan BreakLength { get; set; }
        public TimeSpan BreakFrequency { get; set; }
        public List<TimeRange>? WorkingHours { get; set; }
    }

    public class TimeRange
    {
        public string? Period { get; set; }
        public int Hours { get; set; }
    }
}
