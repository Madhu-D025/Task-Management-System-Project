using AuthApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Security.AccessControl;
using TMS_API.DBContext;
using TMS_API.Models;

namespace TMS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _dbContext;

        public AdminController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        #region Admin Dashboard Related APIs

        [HttpGet("GetAdminDashboardDetails")]
        public async Task<IActionResult> GetAdminDashboardDetails(string ClientId)
        {
            try
            {
                if (string.IsNullOrEmpty(ClientId))
                {
                    return Ok(new { success = false, message = "ClientId is required." });
                }

                var clientcheck = _dbContext.Clients.FirstOrDefault(e => e.Id == ClientId);
                if (clientcheck == null)
                {
                    throw new Exception("ClientId is Not exists");
                }

                var User = await _dbContext.Users.ToListAsync();
                var role = await _dbContext.Roles.ToListAsync();
                var Projects = await _dbContext.Project.ToListAsync();

                var AllManagers = (from u in User
                                join r in role on u.RoleID.ToLower() equals r.RoleID.ToString().ToLower()
                                where u.RoleName.ToLower() == "manager" && u.IsActive == true && u.ClientId.ToLower() == ClientId.ToLower()
                                select new
                                {
                                    u.UserID,
                                    u.FullName,
                                    u.Email,
                                    u.RoleID,
                                    r.RoleName,
                                    u.Department,
                                    u.LocationOrBranch,
                                    u.IsActive,
                                    u.ClientId,
                                    ProjectCount = (from p in Projects
                                                    where p.CreatedBy.ToLower() == u.UserID.ToString().ToLower() && p.IsActive == true
                                                    select new
                                                    {
                                                        p.Id
                                                    }).Count()
                                }).ToList();

                var AllEmployees = (from u in User
                                join r in role on u.RoleID.ToLower() equals r.RoleID.ToString().ToLower()
                                where u.RoleName.ToLower() == "employee" && u.IsActive == true && u.ClientId.ToLower() == ClientId.ToLower()
                                select new
                                {
                                    u.UserID,
                                    u.FullName,
                                    u.Email,
                                    u.RoleID,
                                    r.RoleName,
                                    u.Department,
                                    u.LocationOrBranch,
                                    u.IsActive,
                                    u.ClientId,
                                    ProjectCount = (from p in _dbContext.ProjectEmployees
                                                    where p.EmployeeId.ToLower() == u.UserID.ToString().ToLower() && p.IsActive == true
                                                    select new
                                                    {
                                                        p.Id
                                                    }).Count()
                                }).ToList();

                var TotalProjectsCount = (from p in Projects
                                          where p.IsActive == true && p.IsCompleted == false && p.IsCancelled == false
                                          select new
                                          {
                                              p.Id
                                          }).Count();

                var CompletedProjectsCount = (from p in Projects
                                          where p.IsActive == true && p.IsCompleted == true && p.IsCancelled == false
                                          select new
                                          {
                                              p.Id
                                          }).Count();

                var summary = new
                {
                    TotalManagersCount = AllManagers.Count(),
                    TotalEmployeesCount = AllEmployees.Count(),
                    TotalProjectsCount = TotalProjectsCount,
                    CompletedProjectsCount = CompletedProjectsCount,
                    AllManagers = AllManagers,
                    AllEmployees = AllEmployees
                };

                return Ok(new { success = true, message = "Admin dashboard details extracted successfully", data = summary });

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region Project Management Related API's

        [HttpGet("GetAllProjectDetailsforAdmin")]
        public async Task<IActionResult> GetAllProjectDetailsforAdmin(string ClientId)
        {
            try
            {
                if (string.IsNullOrEmpty(ClientId))
                {
                    return Ok(new { success = false, message = "ClientId is required." });
                }

                var clientcheck = _dbContext.Clients.FirstOrDefault(e => e.Id == ClientId);
                if (clientcheck == null)
                {
                    throw new Exception("ClientId is Not exists");
                }

                //var User = await _dbContext.Users.ToListAsync();
                var Employees = await _dbContext.ProjectEmployees.ToListAsync();
                var Project = await _dbContext.Project.ToListAsync();
                var Task = await _dbContext.Tasks.ToListAsync();

                var TotalProjects = (from p in Project
                                  where p.IsActive == true
                                  select new
                                  {
                                      p.Id
                                  }).Count();
                var Activeproject = (from p in Project
                                   where p.IsActive == true && p.IsCompleted == false && p.IsCancelled == false 
                                   select new
                                   {
                                       p.Id,
                                   }).Count();

                var Completeproject = (from p in Project
                                       where p.IsActive == true && p.IsCompleted == true && p.IsCancelled == false
                                       select new
                                       {
                                           p.Id,
                                       }).Count();

                var ProjectsDetails = (from p in Project
                                       where p.IsActive == true
                                       orderby p.CreatedOn descending
                                       select new
                                       {
                                           p.Id,
                                           p.ProjectName,
                                           p.ProjectType,
                                           p.Status,
                                           p.CreatedOn,
                                           TotalWorkingEmployees = (from e in Employees
                                                                    where e.ProjectId == p.Id
                                                                    select e).Count(),
                                           TotalTask = (from t in Task
                                                        where t.ProjectId == p.Id && t.IsActive == true
                                                        select new {t.ProjectId}).Count(),
                                           CompletedTask = (from t in Task
                                                            where t.ProjectId == p.Id && t.IsActive == true && t.ManagerCompleteStatus == true
                                                            select new { t.ProjectId }).Count(),

                                       }).ToList();

                var summary = new
                {
                    TotalProjects = TotalProjects,
                    TotalActiveproject = Activeproject,
                    TotalCompleteproject = Completeproject,
                    ProjectsDetails = ProjectsDetails
                };


                return Ok(new { success = true, message = "project details extracted successfully", data = summary });


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        #endregion

        #region Task Management Related API's

        [HttpGet("GetAllEmployeesTasksforAdmin")]
        public async Task<IActionResult> GetAllEmployeesTasksforAdmin(string ClientId)
        {
            try
            {
                if (string.IsNullOrEmpty(ClientId))
                {
                    return Ok(new { success = false, message = "ClientId is required." });
                }

                var clientcheck = _dbContext.Clients.FirstOrDefault(e => e.Id == ClientId);
                if (clientcheck == null)
                {
                    throw new Exception("ClientId is Not exists");
                }

                var User = await _dbContext.Users.ToListAsync();
                var role = await _dbContext.Roles.ToListAsync();
                var Project = await _dbContext.ProjectEmployees.ToListAsync();
                var Task = await _dbContext.Tasks.ToListAsync();

                var TotalTasks = (from t in Task
                                  where t.IsActive == true
                                  select new
                                  {
                                      t.Id
                                  }).Count();
                var ActiveTasks = (from t in Task
                                   where t.ManagerCompleteStatus == false
                                   select new
                                   {
                                       t.Id,
                                   }).Count();

                var CompleteTasks = (from t in Task
                                     where t.ManagerCompleteStatus == true
                                     select new
                                     {
                                         t.Id,
                                     }).Count();

                var EmployeesDetails = (from u in User
                                        join r in role on u.RoleID.ToLower() equals r.RoleID.ToString().ToLower()
                                        orderby u.CreatedOn descending
                                        where r.RoleName.ToLower() == "employee" 
                                        select new
                                        {
                                            u.UserID,
                                            u.FullName,
                                            u.Department,
                                            u.Email,
                                            ProjectCount = (from pe in Project
                                                            where u.UserID.ToString().ToLower() == pe.EmployeeId.ToLower() 
                                                            && pe.IsActive == true
                                                            select new {pe.ProjectId}).Count(),
                                           TotalTasks = (from t in Task
                                                        where t.EmployeeUserId.ToString() == u.UserID.ToString().ToLower() 
                                                        && t.IsActive == true
                                                        select new {t.Id}).Count(),
                                            CompletedTasks = (from t in Task
                                                              where t.EmployeeUserId.ToString() == u.UserID.ToString().ToLower()
                                                              && t.IsActive == true && t.ManagerCompleteStatus == true
                                                              select new {t.Id}).Count(),
                                        }).ToList();

                var summary = new
                {
                    TotalTasks = TotalTasks,
                    TotalActiveTasks = ActiveTasks,
                    TotalCompleteTasks = CompleteTasks,
                    EmployeesDetails = EmployeesDetails,
                };

                return Ok(new { success = true, message = "Employees Tasks details extracted successfully", data = summary });


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        #endregion

    }
}
