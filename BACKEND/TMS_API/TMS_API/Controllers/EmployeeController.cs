using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS_API.DBContext;

namespace TMS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _dbContext;

        public EmployeeController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Employee Related API's

        [HttpGet("GetEmployeeDashboardDataByUserId")]
        public async Task<IActionResult> GetEmployeeDashboardDataByUserId(string UserId)
        {
            try
            {
                if (string.IsNullOrEmpty(UserId))
                {
                    return Ok(new { success = false, message = "UserId is required." });
                }

                var userExists = await _dbContext.Users
                    .AnyAsync(x => x.UserID.ToString().ToLower() == UserId.ToLower());

                if (!userExists)
                {
                    return Ok(new { success = false, message = "UserId not found." });
                }

                var project = await _dbContext.Project.ToListAsync();
                var ProjectEmployees = await _dbContext.ProjectEmployees.ToListAsync();

                var TotalProjects = (from pe in ProjectEmployees
                                     join p in project on pe.ProjectId equals p.Id
                                     where pe.EmployeeId.ToLower() == UserId.ToString().ToLower()
                                     orderby p.CreatedOn descending
                                     select new
                                     {
                                         p.Id,
                                         p.ProjectName,
                                         p.ProjectType,
                                         p.ManagerName,
                                         p.StartDate,
                                         p.EndDate,
                                         p.Status
                                     }).ToList();
                var TotalTasks = (from t in _dbContext.Tasks
                                  where t.EmployeeUserId.ToLower() == UserId.ToLower() && t.IsActive == true
                                  select new{ t.Id}).Count();
                var CompletedTasks = (from t in _dbContext.Tasks
                                      where t.EmployeeUserId.ToLower() == UserId.ToLower() && t.IsActive == true && t.ManagerCompleteStatus == true
                                      select new{t.Id}).Count();

                var PendingTasks = (from t in _dbContext.Tasks
                                    where t.EmployeeUserId.ToLower() == UserId.ToLower() && t.IsActive == true && t.ManagerCompleteStatus == false
                                    select new {t.Id}).Count();

                var summary = new
                {
                    TotalProjectsCount = TotalProjects.Count(),
                    TotalTasksCount = TotalTasks,
                    CompletedTasksCount = CompletedTasks,   
                    PendingTasksCount = PendingTasks,
                    TotalProjects = TotalProjects,
                };

                return Ok(new {success = true, message = "Employee dashboard details extracted successfully", data = summary });

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("GetAllLatestTasksByEmployeeId")]
        public async Task<IActionResult> GetAllLatestTasksByEmployeeId(string UserId)
        {
            try
            {
                if (string.IsNullOrEmpty(UserId))
                {
                    return Ok(new { success = false, message = "UserId is required." });
                }

                var userExists = await _dbContext.Users.AnyAsync(x => x.UserID.ToString().ToLower() == UserId.ToLower());

                if (!userExists)
                {
                    return Ok(new { success = false, message = "UserId not found." });
                }

                //var user = await _dbContext.Users.ToListAsync();
                var Task = await _dbContext.Tasks.ToListAsync();

                var AllTasks = (from t in Task
                                where t.EmployeeUserId.ToLower() == UserId.ToLower() && t.IsActive == true
                                orderby t.CreatedOn descending
                                select new
                                {
                                    t.Id,
                                    t.EmployeeUserId,
                                    t.ProjectName,
                                    t.ManagerNames,
                                    t.TaskName,
                                    t.TaskDesc,
                                    t.CreatedOn
                                }).ToList();

                var CompletedTasks = (from t in Task
                                      where t.EmployeeUserId.ToLower() == UserId.ToLower() && t.IsActive == true && t.ManagerCompleteStatus == true
                                      orderby t.CreatedOn descending
                                      select new
                                      {
                                          t.Id,
                                          t.EmployeeUserId,
                                          t.ProjectName,
                                          t.ManagerNames,
                                          t.TaskName,
                                          t.TaskDesc,
                                          t.CreatedOn
                                      }).ToList();

                var PendingTasks = (from t in Task
                                    where t.EmployeeUserId.ToLower() == UserId.ToLower() && t.IsActive == true && t.ManagerCompleteStatus == false
                                    orderby t.CreatedOn descending
                                    select new
                                    {
                                        t.Id,
                                        t.EmployeeUserId,
                                        t.ProjectName,
                                        t.ManagerNames,
                                        t.TaskName,
                                        t.TaskDesc,
                                        t.CreatedOn
                                    }).ToList();


                var summary = new
                {
                    TotalAllTasks = AllTasks.Count(),
                    TotalCompletedTasks = CompletedTasks.Count(),
                    TotalPendingTasks = PendingTasks.Count(),
                    AllTasks = AllTasks,
                    CompletedTasks = CompletedTasks,
                    PendingTasks = PendingTasks
                };

                return Ok(new { success = true, message = "Employee task details extracted successfully", data = summary});

            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion
    }
}
