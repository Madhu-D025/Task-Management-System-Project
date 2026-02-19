using System.ComponentModel.DataAnnotations;

namespace TMS_API.Models
{
    public class Tasks
    {
        [Key]
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? ManagerNames { get; set; }
        public string? EmployeeUserId { get; set; }
        public string? TaskName { get; set; }
        public string? TaskDesc { get; set; }
        public DateTime? TaskStartDate { get; set; }
        public DateTime? TaskEndDate { get; set; }
        public bool? IsActive { get; set; }
        public bool? ManagerCompleteStatus { get; set; }
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
        public bool? ManagerCompleteStatus { get; set; }
        public List<TaskListDto> TaskListDto { get; set; }


    }


    public class TaskListDto
    {
        public string? EmployeeUserId { get; set; }
        public string? TaskName { get; set; }
        public string? TaskDesc { get; set; }
        public DateTime? TaskStartDate { get; set; }
        public DateTime? TaskEndDate { get; set; }
        public bool? IsActive { get; set; }
        public string? Status { get; set; }
        public List<IFormFile>? Documents { get; set; }
    }



    public class TaskUpdateDto
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? ManagerNames { get; set; }
        public bool? ManagerCompleteStatus { get; set; }
        public string? EmployeeUserId { get; set; }
        public string? TaskName { get; set; }
        public string? TaskDesc { get; set; }
        public DateTime? TaskStartDate { get; set; }
        public DateTime? TaskEndDate { get; set; }
        public bool? IsActive { get; set; }
        public string? Status { get; set; }
        public List<IFormFile>? Documents { get; set; }
    }



    public class TaskRelatedInfo
    {
        [Key]
        public int Id { get; set; }
        public int? TaskId { get; set; }
        public int? ProjectId { get; set; }
        public string? Comments { get; set; }
        public bool? IsCompleted { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }


    public class TaskRelatedInfoDto
    {
        [Key]
        public int Id { get; set; }
        public int? TaskId { get; set; }
        public string? UserId { get; set; }
        public int? ProjectId { get; set; }
        public string? Comments { get; set; }
        public bool? IsCompleted { get; set; }
        public bool? IsActive { get; set; }
        public List<IFormFile>? Documents { get; set; }
    }

}
