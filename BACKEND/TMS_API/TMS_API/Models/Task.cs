using System.ComponentModel.DataAnnotations;

namespace TMS_API.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? ManagerNames { get; set; }
        public string? TaskName { get; set; }
        public string? TaskDesc { get; set; }
        public DateTime? TaskStartDate { get; set; }
        public DateTime? TaskEndDate { get; set; }
        public bool? IsActive { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }

    }

    public class TaskDto
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? ManagerNames { get; set; }
        public string? TaskName { get; set; }
        public string? TaskDesc { get; set; }
        public DateTime? TaskStartDate { get; set; }
        public DateTime? TaskEndDate { get; set; }
        public bool? IsActive { get; set; }
        public string? Status { get; set; }
    }
}
