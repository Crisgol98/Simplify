namespace Simplify.Models
{
	public class WeeklyTask
	{
        public DayOfWeek DayOfWeek { get; set; }
        public List<UserTask> Tasks { get; set; }
    }
}
