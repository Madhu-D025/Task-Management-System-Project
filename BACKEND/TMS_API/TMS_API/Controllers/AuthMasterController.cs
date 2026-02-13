using AuthApplication.Models;
using AuthApplication.Models;
using AuthApplication.Services;
using DMSAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TMS_API.DBContext;
using TMS_API.Models;
using static AuthApplication.Services.AuthService;


namespace AuthApplication.Controllers
{
    [ApiController]
    [Route("api/AuthMasterController")]
    public class AuthMasterController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly AuthMasterServices _authMasterService;
        private readonly int _tokenTimespan;

        public AuthMasterController(AppDbContext dbContext, IConfiguration configuration, AuthMasterServices authMasterService)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _authMasterService = authMasterService;
            try
            {
                var span = "30";
                if (span != "")
                    _tokenTimespan = Convert.ToInt32(span.ToString());
                if (_tokenTimespan <= 0)
                {
                    _tokenTimespan = 30;
                }
            }
            catch
            {
                _tokenTimespan = 30;
            }
        }

        #region Authentication

        [HttpGet("FindClient")]
        public IActionResult FindClient(string clientId)
        {
            try
            {
                var client = _authMasterService.FindClient(clientId);
                if (client != null)
                {
                    return Json(new { success = "success", message = "You have successfully get data", data = client });
                }
                else
                {
                    return Json(new { success = "error", message = "Data Not Found" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        #endregion

        #region UserControll

        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers(string ClientId)
        {
            try
            {
                var users = _authMasterService.GetAllUsers(ClientId);
                return Json(new { success = "success", message = "You have successfully get user data", data = users });

            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        [HttpGet("GetUsersById")]
        public async Task<IActionResult> GetUsersById(string UserId)
        {
            try
            {
                var users = await _authMasterService.GetUsersById(UserId);

                return Json(new
                {
                    success = "success",
                    message = "You have successfully get user data",
                    data = users
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser(UserWithRole userWithRole)
        {
            try
            {
                var userResult = await _authMasterService.CreateUser(userWithRole);
                return Json(new { success = "success", message = "You have successfully created the user."});
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        [HttpPost("ProfileUpdateUser")]
        public async Task<ActionResult> ProfileUpdateUser([FromForm] UserProfileUpdate userProfileUpdate)
        {
            try
            {
                var userResult = await _authMasterService.ProfileUpdateUser(userProfileUpdate);
                return Json(new { success = "success", message = "Profile image updated successfully...", data = userResult });
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        [HttpPost("UpdateUser")]
        public async Task<ActionResult<UserWithRole>> UpdateUser(UserWithRole userWithRole)
        {
            try
            {
                var userResult = await _authMasterService.UpdateUser(userWithRole);
                return Json(new { success = "success", message = "Data updated successfully", data = userResult });
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        [HttpPost("DeleteUser")]
        public async Task<ActionResult<UserWithRole>> DeleteUser(string ClientId, Guid UserID)
        {
            try
            {
                var userResult = await _authMasterService.DeleteUser(ClientId, UserID);
                return Json(new { success = "success", message = "You have successfully Deleted User", data = userResult });
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        #endregion

        #region RoleControll

        [HttpGet("GetAllRoles")]
        public IActionResult GetAllRoles(string ClientId)
        {
            try
            {
                var roles = _authMasterService.GetAllRoles(ClientId);
                return Json(new { success = "success", message = "You have successfully get the  Data", data = roles });
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        [HttpGet("GetAllRolesWithApp")]
        public IActionResult GetAllRolesWithApp(string ClientId)
        {
            try
            {
                var rolesWithApp = _authMasterService.GetAllRolesWithApp(ClientId);
                return Json(new { success = "success", message = "You have successfully get the Data", data = rolesWithApp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole(RoleWithApp roleWithApp)
        {
            try
            {
                var createdRole = await _authMasterService.CreateRole(roleWithApp);
                return Json(new { success = "success", message = "You have successfully Created Data", data = createdRole });
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        [HttpPost("UpdateRole")]
        public async Task<IActionResult> UpdateRole(RoleWithApp roleWithApp)
        {
            try
            {
                var updatedRole = await _authMasterService.UpdateRole(roleWithApp);
                return Json(new { success = "success", message = "You have successfully Update Data", data = updatedRole });
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        [HttpPost("DeleteRole")]
        public async Task<IActionResult> DeleteRole(Guid roleId)
        {
            try
            {
                var result = await _authMasterService.DeleteRole(roleId);
                return Json(new { success = "success", message = "You have successfully Deleted Data", data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        #endregion

        #region AppControll

        [HttpGet("GetAllApps")]
        public IActionResult GetAllApps()
        {
            try
            {
                var apps = _authMasterService.GetAllApps();
                return Json(new { success = "success", message = "You have successfully Update Data", data = apps });
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        [HttpPost("CreateApp")]
        public async Task<IActionResult> CreateApp(App app)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException("Model state is not valid");
                }

                var createdApp = await _authMasterService.CreateApp(app);
                return Json(new { success = "success", message = "You have successfully Created Data", data = createdApp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        [HttpPut("UpdateApp")]
        public async Task<IActionResult> UpdateApp(App app)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException("Model state is not valid");
                }

                var updatedApp = await _authMasterService.UpdateApp(app);
                return Json(new { success = "success", message = "You have successfully Update Data", data = updatedApp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        [HttpPost("DeleteApp")]
        public async Task<IActionResult> DeleteApp(App app)
        {
            try
            {
                var deletedApp = await _authMasterService.DeleteApp(app);
                return Json(new { success = "success", message = "You have successfully Deleted App", data = deletedApp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        #endregion

        #region LoginHistoryControll

        //[HttpPost("LoginHistory")]
        //public async Task<IActionResult> LoginHistory(Guid userID, string username)
        //{
        //    try
        //    {
        //        var loginData = await _authMasterService.AddLoginHistory(userID, username);
        //        //return Ok(loginData);
        //        return Json(new { success = "success", message = "You have successfully get user data", data = loginData });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = "error", message = ex.Message });
        //    }
        //}

        [HttpGet("GetAllUsersLoginHistory")]
        public IActionResult GetAllUsersLoginHistory()
        {
            try
            {
                var userLoginHistoryList = _authMasterService.GetAllUsersLoginHistory();
                //return Ok(userLoginHistoryList);
                return Json(new { success = "success", message = "You have successfully get user data", data = userLoginHistoryList });
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        [HttpGet("GetCurrentUserLoginHistory")]
        public IActionResult GetCurrentUserLoginHistory(Guid userID)
        {
            try
            {
                var userLoginHistoryList = _authMasterService.GetCurrentUserLoginHistory(userID);
                //return Ok(userLoginHistoryList);
                return Json(new { success = "success", message = "You have successfully get user data", data = userLoginHistoryList });
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        [HttpPost("SignOut")]
        public async Task<IActionResult> SignOut(Guid userID)
        {
            try
            {
                var result = await _authMasterService.SignOut(userID);
                //return Ok(result);
                return Json(new { success = "success", message = "You have successfully get user data", data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        #endregion

        #region ChangePasswordControl

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePassword changePassword)
        {
            try
            {
                var result = await _authMasterService.ChangePassword(changePassword);
                //return Ok(result);
                return Json(new { success = "success", message = "You have successfully Reseted the Password", data = result });

            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        [HttpPost("SendResetLinkToMail")]
        public async Task<ActionResult<AuthTokenHistory>> SendResetLinkToMail(EmailModel emailModel)
        {
            try
            {
                var result = await _authMasterService.SendResetLinkToMail(emailModel);
                //return Ok(result);
                return Json(new { success = "success", message = "You have successfully Sent Mail", data = result });

            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        [HttpPost("ForgotPassword")]
        public async Task<ActionResult> ForgotPassword(ForgotPassword forgotPassword)
        {
            try
            {
                var result = await _authMasterService.ForgotPassword(forgotPassword);
                //return Ok();
                return Json(new { success = "success", message = "You have successfully Reseted password", data = result });

            }
            catch (Exception ex)
            {
                return Json(new { success = "error", message = ex.Message });
            }
        }

        #endregion

        #region LogGetApi's

        //[HttpGet("GetUsersCLog")]
        //public ActionResult<List<ErrorLogEntry>> GetUsersCLog()
        //{
        //    var errorLogs = _authMasterService.GetUsersCLog();
        //    return Ok(errorLogs);
        //}

        //[HttpGet("GetAuthMasterErrorLog")]
        //public ActionResult<List<LoginLogEntry>> GetAuthMasterErrorLog()
        //{
        //    var loginLogs = _authMasterService.GetAuthMasterErrorLog();
        //    return Ok(loginLogs);
        //}

        #endregion

        [HttpPost("UpdateUserStatus")]
        public async Task<IActionResult> UpdateUserStatus(string userID, bool isActive, string modifiedBy, string? department)
        {
            try
            {
                var checkuser = await _dbContext.Users.Where(x => x.UserID.ToString().ToLower() == userID.ToLower()).FirstOrDefaultAsync();
                if (checkuser != null)
                {
                    checkuser.IsActive = isActive;
                    checkuser.ModifiedBy = modifiedBy;
                    checkuser.IsActive = isActive;
                }
                else
                {
                    return Ok(new { success = false, message = "User data not found", data = checkuser });
                }
                await _dbContext.SaveChangesAsync();
                return Ok(new { success = false, message = "User data updated successfully", data = checkuser });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
        private string Decrypt(string Password, bool UseHashing)
        {
            try
            {
                string EncryptionKey = "Iteos";
                byte[] KeyArray;
                byte[] ToEncryptArray = Convert.FromBase64String(Password);
                if (UseHashing)
                {
                    using (MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider())
                    {
                        KeyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(EncryptionKey));
                    }
                }
                else
                {
                    KeyArray = Encoding.UTF8.GetBytes(EncryptionKey);
                }

                using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = KeyArray;
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;
                    ICryptoTransform cTransform = tdes.CreateDecryptor();
                    byte[] resultArray = cTransform.TransformFinalBlock(ToEncryptArray, 0, ToEncryptArray.Length);
                    return Encoding.UTF8.GetString(resultArray);
                }
            }
            catch (Exception ex)
            {

                return null;
            }
        }


        [HttpPost("CreateMasterData")]
        public async Task<IActionResult> CreateMasterData(MasterDto data)
        {
            var tranaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrEmpty(data.MasterName) || (data.MasterName) == "string")
                {
                    return Ok(new { success = false, message = "Mastername is required" });
                }

                if(string.IsNullOrEmpty(data.MasterName))
                {
                    var exist = await _dbContext.Masters
                        .AnyAsync(x => x.MasterName.ToLower() == data.MasterName.ToLower() && x.MasterValue.ToLower() == data.MasterValue.ToLower());
                    if(exist)
                    {
                        return Ok(new { success = false, message = "Data is already Exist" });
                    }
                }

                var master = new MasterDto
                {
                    MasterName = data.MasterName,
                    MasterValue = data.MasterValue,
                    CreatedBy = data.CreatedBy,
                    CreatedOn = DateTime.Now,
                    IsActive = true,

                };

                await tranaction.CommitAsync();
                return Ok(new { success = true, message = "Data Created Successfully", data = master });

            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetAllMasterData")]
        public async Task<IActionResult> GetAllMasterData()
        {
            try
            {
                var data = await _dbContext.Masters.ToListAsync();
                return Ok(new { success = true, message = "Data extarced successfully", data = data });

            }

            catch (Exception ex)
            {
                return Ok(new { sucess = false, message = ex.Message });
            }
        }
    }

}

