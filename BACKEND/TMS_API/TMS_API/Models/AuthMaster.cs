
using Microsoft.Data.SqlClient;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//using LearningAPI.Models;

namespace AuthApplication.Models
{
    public class User
    {
        [Key]
        public Guid UserID { get; set; }
        public string? RoleID { get; set; }
        public string? RoleName { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LocationOrBranch { get; set; }
        public string? Department { get; set; }
        public string? EmploymentType { get; set; }
        public string? Status { get; set; }
        public bool? EmailStatus { get; set; }
        public string? Password { get; set; }
        public string? Pass1 { get; set; }
        public string? Pass2 { get; set; }
        public string? Pass3 { get; set; }
        public string? Pass4 { get; set; }
        public string? Pass5 { get; set; }
        public string? LastChangedPassword { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? IsLockDuration { get; set; }
        public int Attempts { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public string? joiningDate { get; set; }
        public string? ClientId { get; set; }
        public string? ProfilePath { get; set; }
        public string? PicDbPath { get; set; }

    }

    public class Role
    {
        [Key]
        public Guid RoleID { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }

        public string? ClientId { get; set; }
    }

    public class UserRoleMap
    {
        [Column(Order = 0), Key, ForeignKey("User")]
        public Guid UserID { get; set; }
        [Column(Order = 1), Key, ForeignKey("Role")]
        public Guid RoleID { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class App
    {
        [Key]
        public int AppID { get; set; }
        public string AppName { get; set; }
        public string? AppRoute { get; set; }
        public string? ClientId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }
 

    public class RoleAppMap
    {

        [Key]
        [Column(Order = 1)]
        public Guid RoleID { get; set; }
        [Key]
        [Column(Order = 2)]
        public int AppID { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class UpdateActiveInactiveModel
    {
        public Guid UserID { get; set; }
        public bool IsActive { get; set; }
    }
    public class UserWithRole
    {
        public Guid? UserID { get; set; }
        public Guid RoleID { get; set; }
        public string? RoleName { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LocationOrBranch { get; set; }
        public string? Department { get; set; }
        public string? EmploymentType { get; set; }
        public string? joiningDate { get; set; }
        public string? Status { get; set; }
        public bool? EmailStatus { get; set; }
        public string? ClientId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public string? ProfilePath { get; set; }
        public string? PicDbPath { get; set; }
        public IFormFile? ProfilePic { get; set; }

    }

    public class UserReffdata
    {
        public Guid? UserID { get; set; }
        public Guid RoleID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Designation { get; set; }
        public string Division { get; set; }
        public string RoleName { get; set; }
        public string? ClientId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; }

    }

    public class ManagerUserMap
    {
        [Key]
        public int MapID { get; set; }
        public Guid ManagerID { get; set; }
        public Guid UserID { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }
    public class ManagerUserMapDTO
    {
        public Guid ManagerID { get; set; }
        public string? UserID { get; set; }
        public string? CreatedBy { get; set; }
    }
    public class ManagerUserMapData
    {
        public Guid ManagerID { get; set; }
        public string? UserID { get; set; }
    }

    public class UserProfileUpdate
    {
        public Guid? UserID { get; set; }
        public string? ModifiedBy { get; set; }
        public IFormFile? ProfilePic { get; set; }
    }
    public class UserView
    {
        public Guid UserID { get; set; }
        public string EmployeeId { get; set; }
    }

    public class MailBodyConfiguration
    {
        [Key]
        public int ID { get; set; }
        public string MailType { get; set; }
        public string MailBody { get; set; }
        public string MailSubject { get; set; }
    }

    public class EmpInfo
    {

        [Key]
        public Guid EmpId { get; set; }
        public int? EmpInfoId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Mobile { get; set; }
        public string? Address { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Department { get; set; }
        public string? JoiningDate { get; set; }
        public string? Plant { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class FromEmployeeEmpInfo
    {
        public Guid EmpId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Mobile { get; set; }
        public string? Address { get; set; }
        public string? DateOfBirth { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class RoleWithApp
    {
        public Guid? RoleID { get; set; }
        public string RoleName { get; set; }
        public int[] AppIDList { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public string? ClientId { get; set; }
    }
    public class RoleWithAppView
    {
        public Guid RoleID { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? ClientId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public int[] AppIDList { get; set; }
        public string AppNames { get; set; }
    }


    public class NewsAndNotification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NID { get; set; }
        public string? NTital { get; set; }
        public DateTime? NDate { get; set; }
        public string? NDescription { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class NewsAndNotificationDTO
    {
        public int? NID { get; set; }
        public string? NTital { get; set; }
        public DateTime? NDate { get; set; }
        public string? NDescription { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class UserLoginHistory
    {
        [Key]
        public int ID { get; set; }
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
    }

    public class ChangePassword
    {
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public string CurrentPassword { get; set; }

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$", ErrorMessage = "Password must contain at least one alphabetic character and one special character.")]
        public string NewPassword { get; set; }
    }

    public class EmailModel
    {
        public string EmailAddress { get; set; }
        public string siteURL { get; set; }
    }

    public class ForgotPassword
    {
        public Guid UserID { get; set; }
        public string EmailAddress { get; set; }

        //[MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
        //[RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{12,}$", ErrorMessage = "Password must contain at least one alphabetic character and one special character.")]
        public string NewPassword { get; set; }
        public string Token { get; set; }
    }

    public class ForgotPasswordTokenCheck
    {
        //public Guid UserID { get; set; }        
        public string Token { get; set; }
    }

    public class AuthTokenHistory
    {
        [Key]
        public int TokenHistoryID { get; set; }
        public Guid UserID { get; set; }
        //public string? UserName { get; set; }
        public string Token { get; set; }
        public string EmailAddress { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ExpireOn { get; set; }
        public DateTime? UsedOn { get; set; }
        public bool IsUsed { get; set; }
        public string? Comment { get; set; }            
    }
    public class LoginModel
    {
        public string EmailId { get; set; }
        public string Password { get; set; }
        public string clientId { get; set; }
    }

    public class ClientIdModel
    {
        public Guid UserId { get; set; }
        public string clientId { get; set; }
    }
    public class AuthenticationResult
    {
        public Guid UserID { get; set; }
        public string EmployeeId { get; set; }
        public string? FullName { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string UserRole { get; set; }
        public string Token { get; set; }
        public List<App> MenuItemNames { get; set; }
        public string IsChangePasswordRequired { get; set; }
        public string ReasonForReset { get; set; }
        public string? ProfilePic { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string? Designation { get; set; }
        public string? Division { get; set; }
        public string? Department { get; set; }
    }

    public class LoginResult
    {
        public Guid UserID { get; set; }
        public string EmployeeId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public string UserRole { get; set; }
        public string Token { get; set; }
        public string IsChangePasswordRequired { get; set; }
        public string ReasonForReset { get; set; }
        public string? ProfilePic { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string? Designation { get; set; }
        public string? Division { get; set; }

    }

    public class VendorUser
    {
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class STMPDetails
    {
        public string Host { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
    }

    public class EmailConfiguration
    {
        [Key]
        public int ID { get; set; }

        public string ServerAddress { get; set; }

        public string MailAddress { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string IncomingIMAPPort { get; set; }

        public string IncomingPOP3Port { get; set; }

        public string OutgoingPort { get; set; }

        public bool IsSSL { get; set; }

        public bool IsActive { get; set; }

    }

    public class EmailConfigurations
    {
        public int ID { get; set; }
        public string MailID { get; set; }
        public string MailPassword { get; set; }

        public string UserName { get; set; }

        public string SmtpServer { get; set; }

        public string SmtpPort { get; set; }
        public bool IsSSL { get; set; }
    }

    [Table("otpConfiguration")]
    public class otpConfiguration
    {
        [Key]
        public int ID { get; set; }
        public string? method { get; set; }
        public string? send_to { get; set; }
        public string? msg { get; set; }
        public string? msg_type { get; set; }
        public string? userid { get; set; }
        public string? auth_scheme { get; set; }
        public string? password { get; set; }
        public string? v { get; set; }
        public string? format { get; set; }

    }


    public class PasswordResetOtpHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? MobileNo { get; set; }
        public string OTP { get; set; }
        public bool OTPIsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime ExpiryOn { get; set; }
    }

    public class PasswordChangeRequest
    {
        public string EmailorMobileNo { get; set; }
        public string Otp { get; set; }

        //[MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
        //[RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{12,}$", ErrorMessage = "Password must contain at least one alphabetic character and one special character.")]
        public string NewPassword { get; set; }
    }

    public class SignUpUserRequest
    {
        public string UserId { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class Documentdto
    {
        public string? Id { get; set; }
        public List<IFormFile>? Documents { get; set; }
        public string? DocumentId { get; set; }
        public List<int>? DocumentNameId { get; set; }
        public string? documentType { get; set; }
        public string? FolderName { get; set; }
        public string? UserID { get; set; }

        public int? InitiationId { get; set; } = 0;
    }
    public class DocumentMaster
    {
        [Key]

        public int Id { get; set; }
        public string? DocumentId { get; set; }
        public string? DocumentType { get; set; }
        public string? DocumentExtention { get; set; }
        public string? DocumentName { get; set; }
        public string? DocumentPath { get; set; }
        public string? DocumentURL { get; set; }
        public int? DocumentNameId { get; set; }
        public string? DocumentXeroxOrOriginal { get; set; }

        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }

        public int? InitiationId { get; set; } = 0;

        public bool? DocumentReadStatus { get; set; } = false;

    }

    public class DepartmentMaster
    {
        [Key]

        public int Id { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string? DepartmentName { get; set; }
        public string? UserId { get; set; }
    }


    public class SendPaymentReminderRequest
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string? SenderUserId { get; set; }
        public string? ReceiverUserId { get; set; }
        public string? Message { get; set; }
    }

    public class SendMesageToEmail
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string? SenderUserId { get; set; }
        public string? ReceiverUserId { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; }
    }


}

