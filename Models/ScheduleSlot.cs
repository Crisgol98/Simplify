namespace Simplify.Models
{
    public class ScheduleSlot
    {
        public DateTime Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public string? Period { get; set; }
        public List<UserTask> Tasks { get; set; }
        public bool? IsBreak { get; set; }
    }
}
