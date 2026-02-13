//using AuthApplication.Helpers;
using AuthApplication.Models;
using AuthApplication.Services;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;   
using System.Linq;
using System.Security.Claims;   
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using LearningAPI.DBContext;
using static AuthApplication.Services.AuthService;

namespace AuthApplication.Controllers
{
    [ApiController]
    [Route("api/AuthController")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;
        private readonly AuthService _authService;


        public AuthController(IConfiguration configuration, AppDbContext dbContext, AuthService authService)
        {
            _configuration = configuration;
            _authService = authService;
            _dbContext = dbContext;

        }

        [HttpPost("token")]
        public async Task<IActionResult> GetToken(LoginModel loginModel)
        {
            try
            {
                var authenticationResult = await _authService.GetToken(loginModel);
                if (authenticationResult != null)
                {
                    return Json(new { success = "success", message = "You have successfully logged in", data = authenticationResult });
                }
                else
                {
                    return Json(new { success = "error", message = "The user name or password is incorrect" });
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return Json(new { success = "error", message = ex.Message });
            }
        }
    }
}

