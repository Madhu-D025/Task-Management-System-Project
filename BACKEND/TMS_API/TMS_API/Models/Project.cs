using System.ComponentModel.DataAnnotations;

namespace TMS_API.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        public string? ProjectName { get; set; }
        public string? ProjectType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsCompleted { get; set; }
        public bool? IsCancelled { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }
    public class ProjectDto
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? ProjectName { get; set; }
        public string? ProjectType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public bool? IsActive { get; set; } = true;
        public bool? IsCompleted { get; set; } = false;
        public bool? IsCancelled { get; set; } = false;
        public string? Status { get; set; }
        public List<ProjectEmployeesDto> ProjectEmployeesDto { get; set; }


    }
    public class ProjectEmployees
    {
        [Key]
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public string? EmployeeId { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }
    public class ProjectEmployeesDto
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? ProjectId { get; set; }
        public string? EmployeeId { get; set; }
        public bool? IsActive { get; set; }

    }

    public class ProjectUpdateStatusDto
    {
        public int ProjectId { get; set; }
        public string? UserId { get; set; }
        public string? Status { get; set; }
        public bool? IsCompleted { get; set; }
        public bool? IsCancelled { get; set; }

    }

}
