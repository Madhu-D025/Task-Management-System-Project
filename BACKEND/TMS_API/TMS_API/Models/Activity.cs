using System.ComponentModel.DataAnnotations;

namespace TMS_API.Models
{
    public class Activity
    {
    }

    public class UserActivityLog
    {
        [Key]
        public int SNID { get; set; }
        public string? SNType { get; set; }
        public string? SNTital { get; set; }
        public string? SNReviewUserId { get; set; }
        public string? SNActionUserId { get; set; }
        public DateTime? SNDate { get; set; }
        public string? SNDescription { get; set; }
        public bool? IsRead { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }


}
