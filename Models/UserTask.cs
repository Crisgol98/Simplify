namespace Simplify.Models
{
	public class UserTask
	{
		public int Id { get; set; }
        public int UserId { get; set; }
        public string? Name { get; set; }
		public string? Description { get; set; }
        public string? Priority { get; set; }
        public int? EstimatedTime { get; set; }
        public int? RemainingTime { get; set; }
        public string? State { get; set; }
		public DateTime? CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
