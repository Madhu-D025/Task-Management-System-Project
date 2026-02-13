using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace Wm.Models
{
    public class Master
    {
    }

    public class Masters
    {
        [Key]
        public int Id { get; set; }
        public string? MasterName { get; set; }
        public string? MasterValue { get; set; }
        public bool? IsActive { get; set; } = true;
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class MasterDto
    {
        [Key]
        public int Id { get; set; }
        public string? MasterName { get; set; }
        public string? MasterValue { get; set; }
        public bool? IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }


    public class DocumentExtensions
    {
        [Key]
        public int Id { get; set; }
        public string? Extensions { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

    }


    public class DocumentExtensionsDto
    {
        [Key]
        public int Id { get; set; }
        public string? Extensions { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

    }




}
