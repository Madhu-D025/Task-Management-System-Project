//using AuthApplication.Helpers;
using AuthApplication.Models;
using DMSAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LearningAPI.DBContext;
//using LearningAPI.Models;

namespace AuthApplication.Services
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;

        public AuthService(IConfiguration configuration, AppDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public Client FindClient(string clientId)
        {
            try
            {
                var client = _dbContext.Clients.Find(clientId);
                return client;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<AuthenticationResult> GetToken(LoginModel loginModel)
        {
            try
            {
                Client client = FindClient(loginModel.clientId);
                if (client == null)
                {
                    //ErrorLog($"{loginModel.EmailId}, Invalid client id.");
                    throw new Exception("Invalid client id");
                }

                var authenticationResult = await AuthenticateUser(loginModel.EmailId, loginModel.Password, loginModel.clientId);
                if (authenticationResult == null)
                {
                    var errorMessage = "The user name or password is incorrect.";
                    throw new Exception(errorMessage);
                }

                IConfiguration JWTSecurityConfig = _configuration.GetSection("Jwt");
                string securityKey = JWTSecurityConfig.GetValue<string>("Key");
                string issuer = JWTSecurityConfig.GetValue<string>("Issuer");
                string audience = JWTSecurityConfig.GetValue<string>("Audience");

                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
                var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, authenticationResult.EmailAddress),
                    new Claim(ClaimTypes.Role, authenticationResult.UserRole),
                    new Claim("UserID", authenticationResult.UserID.ToString()),
                    new Claim("FullName", authenticationResult.FullName ?? ""),
                    new Claim("Department", authenticationResult.Department ?? "")
                };

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    expires: DateTime.Now.AddMinutes(24),
                    signingCredentials: signingCredentials,
                    claims: claims
                );

                authenticationResult.Token = new JwtSecurityTokenHandler().WriteToken(token);

                //WriteLog.AddWriteLog($"{loginModel.EmailId} User Logged in Successfully", loginModel.EmailId);
                Log.DataLog(loginModel.EmailId, $"{loginModel.EmailId} User Loggedin Successfully", "Login Log");


                var userLoginHistory = new UserLoginHistory
                {
                    UserID = authenticationResult.UserID,
                    UserName = authenticationResult.EmployeeId,
                    Email = authenticationResult.EmailAddress,
                    LoginTime = DateTime.Now,
                };

                _dbContext.UserLoginHistory.Add(userLoginHistory);
                await _dbContext.SaveChangesAsync();

                return authenticationResult;
            }
            catch (Exception ex)
            {
                //ErrorLog($"Exception occurred in GetToken method for username '{loginModel.EmailId}': {ex.Message}");
                Log.Error(loginModel.EmailId,$"Exception occurred in GetToken method for username '{loginModel.EmailId}': {ex.Message}","Login Log");
                throw new Exception(ex.Message ?? "Network Error");
            }
        }
       

        public async Task<AuthenticationResult> AuthenticateUser(string UserName, string Password, string ClientId)
        {
            try
            {
                string userId = string.Empty, role = string.Empty;
                List<App> MenuItemList = new List<App>();
                string MenuItemNames = "";
                User user = null;
                var Success = true;
                string isChangePasswordRequired = "No";
                string DefaultPassword = _configuration.GetValue<string>("DefaultPassword");

                if (UserName.Contains('@') && UserName.Contains('.'))
                {
                    user = _dbContext.Users.FirstOrDefault(tb => tb.Email == UserName && tb.ClientId == ClientId);
                }
                else
                {
                    user = _dbContext.Users.FirstOrDefault(tb => tb.UserName == UserName && tb.ClientId == ClientId);
                }

                if (user == null)
                {
                    var errorMessage = "The user name or password is incorrect.";
                    throw new Exception(errorMessage);
                }

                if (!user.IsActive || !string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Account is Disabled please contact the admin");
                }

                bool isValidUser = false;
                var authenticationResult = new AuthenticationResult();
                string DecryptedPassword = Decrypt(user.Password, true);
                isValidUser = DecryptedPassword == Password;

                if (isValidUser)
                {
                    if (!user.IsLocked || (user.IsLocked && DateTime.Now >= user.IsLockDuration))
                    {
                        user.IsLocked = false;
                        user.Attempts = 0;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        if (user.Pass1 == null)
                        {
                            isChangePasswordRequired = "Yes";
                            authenticationResult.ReasonForReset = "Please Enter new Password to login";
                        }

                        Role userRole = (from tb1 in _dbContext.Roles
                                         join tb2 in _dbContext.UserRoleMaps on tb1.RoleID equals tb2.RoleID
                                         join tb3 in _dbContext.Users on tb2.UserID equals tb3.UserID
                                         where tb3.UserID == user.UserID && tb1.IsActive && tb2.IsActive && tb3.IsActive && tb3.ClientId == ClientId
                                         select tb1).FirstOrDefault();
                        //var MenuItemList1 = new List<AllApps>();

                        if (userRole != null)
                        {
                            MenuItemList = (from tb1 in _dbContext.Apps
                                            join tb2 in _dbContext.RoleAppMaps on tb1.AppID equals tb2.AppID
                                            where tb2.RoleID == userRole.RoleID && tb1.IsActive && tb2.IsActive
                                            where tb1.ClientId.Contains(user.ClientId)
                                            select tb1).OrderBy(x => x.CreatedOn).ToList();
                        }
                        authenticationResult.UserID = user.UserID;
                        authenticationResult.EmployeeId = user.UserName;
                        authenticationResult.DisplayName = user.FullName;
                        authenticationResult.UserName = user.UserName;
                        authenticationResult.EmailAddress = user.Email;
                        authenticationResult.FullName = user.FullName;
                        authenticationResult.ProfilePic = user.PicDbPath;
                        authenticationResult.MenuItemNames = MenuItemList;
                        authenticationResult.IsSuccess = Success;
                        authenticationResult.Message = "Login Success";
                        authenticationResult.UserRole = userRole != null ? userRole.RoleName : "";
                        authenticationResult.IsChangePasswordRequired = isChangePasswordRequired;
                        //authenticationResult.Department = user.Department;

                        await _dbContext.SaveChangesAsync();
                        //LogLoginAttempt(UserName, "Logged in successfully.");
                        return authenticationResult;
                    }
                    else
                    {
                        //ErrorLog($"{UserName}, Your Account has been locked due to multiple incorrect password login attempts. Please try again after 15 minutes.");
                        throw new Exception("Your Account Has Been Locked Due To Incorrect Password. Please Login After 15 minutes");
                    }
                }
                else
                {
                    user.Attempts++;
                    var reason = "The user name or password is incorrect.";
                    if (user.Attempts == 5)
                    {
                        user.IsLocked = true;
                        reason = "Your Account Has Been Locked Due To Incorrect Password. Please Login After 15 minutes";
                        user.IsLockDuration = DateTime.Now.AddMinutes(15);
                    }
                    await _dbContext.SaveChangesAsync();
                    //ErrorLog($"{UserName} is Failed to login : {reason}");
                    throw new Exception(reason);
                }
            }
            catch (Exception ex)
            {
                //ErrorLog($"Exception occurred in AuthenticateUser method for username '{UserName}': {ex.Message}");
                Log.Error(UserName, $"Exception occurred in Authenticate method for username '{UserName}': {ex.Message}", "Login Log");

                throw new Exception(ex.Message ?? "Network Error");
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

        private LoginLogEntry ParseLogEntry(string logLine)
        {
            string[] parts = logLine.Split('-');
            if (parts.Length != 5)
            {
                return null;
            }

            int year = int.Parse(parts[0].Trim());
            int month = int.Parse(parts[1].Trim());
            int day = int.Parse(parts[2].Trim().Split(' ')[0]);
            string time = parts[2].Trim().Split(' ')[1];
            string username = parts[3].Trim();
            string status = parts[4].Trim();

            DateTime timestamp = DateTime.Parse($"{year}-{month}-{day} {time}");
            return new LoginLogEntry(timestamp, username, status);
        }

        public class LoginLogEntry
        {
            public DateTime Timestamp { get; set; }
            public string Username { get; set; }
            public string Status { get; set; }

            public LoginLogEntry(DateTime timestamp, string username, string status)
            {
                Timestamp = timestamp;
                Username = username;
                Status = status;
            }
        }

        //private void LogLoginAttempt(string username, string status)
        //{
        //    string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {username} - {status}";
        //    string logFilePath = Path.Combine("Logs", "Login Log","Auth.log.txt");
        //    if (!Directory.Exists(Path.GetDirectoryName(logFilePath)))
        //    {
        //        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
        //    }

        //    try
        //    {
        //        using (StreamWriter writer = System.IO.File.AppendText("Logs/Login Log/Auth.log.txt"))
        //        {
        //            writer.WriteLine(logEntry);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle logging exception silently
        //    }
        //}

        //private void ErrorLog(string status)
        //{
        //    string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {status}";
        //    try
        //    {
        //        string logFilePath = Path.Combine("Logs", "Error.log.txt");
        //        if (!Directory.Exists(Path.GetDirectoryName(logFilePath)))
        //        {
        //            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
        //        }

        //        using (StreamWriter writer = System.IO.File.AppendText("Logs/Error.log.txt"))
        //        {
        //            writer.WriteLine(logEntry);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle logging exception silently
        //    }
        //}

        //public List<ErrorLogEntry> GetErrorLog()
        //{
        //    List<ErrorLogEntry> loginLogs = new List<ErrorLogEntry>();
        //    try
        //    {
        //        string logFilePath = Path.Combine("Logs", "Error.log.txt");
        //        if (!Directory.Exists(Path.GetDirectoryName(logFilePath)))
        //        {
        //            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
        //        }

        //        using (StreamReader reader = System.IO.File.OpenText("Logs/Error.log.txt"))
        //        {
        //            string line;
        //            while ((line = reader.ReadLine()) != null)
        //            {
        //                var logEntry = ParseErrorEntry(line);
        //                if (logEntry != null)
        //                {
        //                    loginLogs.Add(logEntry);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error reading error log: {ex.Message}");
        //    }
        //    return loginLogs;
        //}

        private ErrorLogEntry ParseErrorEntry(string logLine)
        {
            string[] parts = logLine.Split('-');
            if (parts.Length != 4)
            {
                return null;
            }

            int year = int.Parse(parts[0].Trim());
            int month = int.Parse(parts[1].Trim());
            int day = int.Parse(parts[2].Trim().Split(' ')[0]);
            string time = parts[2].Trim().Split(' ')[1];
            string error = parts[3].Trim();

            DateTime timestamp = DateTime.Parse($"{year}-{month}-{day} {time}");
            return new ErrorLogEntry(timestamp, error);
        }

        public class ErrorLogEntry
        {
            public DateTime Timestamp { get; set; }
            public string Error { get; set; }

            public ErrorLogEntry(DateTime timestamp, string error)
            {
                Timestamp = timestamp;
                Error = error;
            }
        }
    }
}
