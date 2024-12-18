namespace Simplify.Models
{
    public class WeeklyScheduleSlot
    {
        public DateTime Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public List<UserTask> Tasks { get; set; }
        public bool? IsBreak { get; set; }
    }
}
