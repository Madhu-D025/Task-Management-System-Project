using AuthApplication.Models;
using DMSAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Security.AccessControl;
using TMS_API.DBContext;

namespace TMS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManagerController : Controller
    {
        private readonly AppDbContext _dbContext;

        public ManagerController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Manager Dashboard Related APIs

        [HttpGet("GetManagerDashboardDataByUserId")]
        public async Task<IActionResult> GetManagerDashboardDataByUserId(string UserId)
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
                var Employees = await _dbContext.ProjectEmployees.ToListAsync();

                var AllProjects = (from p in project
                                   where p.CreatedBy.ToLower() == UserId.ToString().ToLower() && p.IsActive == true
                                   orderby p.CreatedOn descending
                                   select new
                                   {
                                       p.Id,
                                       p.ProjectName,
                                       p.ProjectType,
                                       p.ManagerName,
                                       p.StartDate,
                                       p.EndDate,
                                       p.Status,
                                       TotalWorkingEmployees = (from e in Employees
                                                                where e.ProjectId == p.Id
                                                                select new { e.Id }).Count(),
                                   }).ToList();

                var ActiveProjects = (from p in project
                                   where p.CreatedBy.ToLower() == UserId.ToLower() && p.IsActive == true && p.IsCompleted == false && p.IsCancelled == false
                                   orderby p.CreatedOn descending
                                   select new
                                   {
                                       p.Id,
                                       p.ProjectName,
                                       p.ProjectType,
                                       p.ManagerName,
                                       p.StartDate,
                                       p.EndDate,
                                       p.Status,
                                       TotalWorkingEmployees = (from e in Employees
                                                                where e.ProjectId == p.Id
                                                                select new { e.Id }).Count(),
                                   }).ToList();

                var CompletedProjects = (from p in project
                                      where p.CreatedBy.ToLower() == UserId.ToLower() && p.IsActive == true  && p.IsCompleted == true
                                      orderby p.CreatedOn descending
                                      select new
                                      {
                                          p.Id,
                                          p.ProjectName,
                                          p.ProjectType,
                                          p.ManagerName,
                                          p.StartDate,
                                          p.EndDate,
                                          p.Status,
                                          TotalWorkingEmployees = (from e in Employees
                                                                   where e.ProjectId == p.Id
                                                                   select new { e.Id }).Count(),
                                      }).ToList();

                var CancelledProjects = (from p in project
                                         where p.CreatedBy.ToLower() == UserId.ToLower() && p.IsActive == true && p.IsCancelled == true
                                         orderby p.CreatedOn descending
                                         select new
                                         {
                                             p.Id,
                                             p.ProjectName,
                                             p.ProjectType,
                                             p.ManagerName,
                                             p.StartDate,
                                             p.EndDate,
                                             p.Status,
                                             TotalWorkingEmployees = (from e in Employees
                                                                      where e.ProjectId == p.Id
                                                                      select e).Count(),
                                         }).ToList();


                var summary = new
                {
                    TotalProjectCount = AllProjects.Count(),
                    ActiveProjectsCount = ActiveProjects.Count(),
                    CompletedProjectsCount = CompletedProjects.Count(),
                    CancelledProjectsCount = CancelledProjects.Count(),
                    AllProjects = AllProjects,
                    ActiveProjects = ActiveProjects,
                    CompletedProjects = CompletedProjects,
                    CancelledProjects = CancelledProjects,
                };

                return Ok(new { success = true, message = "Manager dashboard data extracted successfully", data = summary });

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("GetProjectDetailsByProjectId")]
        public async Task<IActionResult> GetProjectDetails1ByProjectId(int ProjectId)
        {
            try
            {
                if (ProjectId <= 0)
                {
                    return Ok(new { success = false, message = "Project Id is required." });
                }

                var project = await _dbContext.Project
                    .Where(p => p.Id == ProjectId && p.IsActive == true)
                    .Select(p => new
                    {
                        p.Id,
                        p.ProjectName,
                        p.ProjectType,
                        p.ManagerId,
                        p.ManagerName,
                        p.StartDate,
                        p.EndDate,
                        p.IsActive,
                        p.IsCompleted,
                        p.IsCancelled,
                        p.Status,
                        p.CreatedBy,
                        p.CreatedOn,
                        p.ModifiedBy,
                        p.ModifiedOn,

                        EmployeeDetails = _dbContext.ProjectEmployees
                            .Where(pe => pe.ProjectId == p.Id)
                            .Select(pe => new
                            {
                                pe.EmployeeId,

                                // Employee Name from Users table
                                EmployeeName = _dbContext.Users
                                    .Where(u => u.UserID.ToString() == pe.EmployeeId)
                                    .Select(u => u.FullName)
                                    .FirstOrDefault(),

                                // Total Tasks Assigned
                                TotalTasksAssigned = _dbContext.Tasks
                                    .Count(t => t.ProjectId == p.Id
                                             && t.EmployeeUserId == pe.EmployeeId),

                                // Completed Tasks
                                CompletedTasks = _dbContext.Tasks
                                    .Count(t => t.ProjectId == p.Id
                                             && t.EmployeeUserId == pe.EmployeeId
                                             && t.ManagerCompleteStatus == true),

                                // Delayed Tasks
                                DelayedTasks = _dbContext.Tasks
                                    .Count(t => t.ProjectId == p.Id
                                             && t.EmployeeUserId == pe.EmployeeId
                                             && (t.ManagerCompleteStatus == false
                                                 || t.ManagerCompleteStatus == null)
                                             && t.TaskEndDate < DateTime.Now)
                            }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (project == null)
                {
                    return Ok(new { success = false, message = "Project not found." });
                }

                return Ok(new { success = true, data = project });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        //[HttpGet("GetProjectDetails2ByProjectId")]
        //public async Task<IActionResult> GetProjectDetails2ByProjectId(int ProjectId)
        //{
        //    try
        //    {
        //        if (ProjectId <= 0)
        //        {
        //            return Ok(new { success = false, message = "Project Id is required." });
        //        }

        //        // Fetch project details
        //        var project = await _dbContext.Project
        //            .Where(p => p.Id == ProjectId && p.IsActive == true)
        //            .FirstOrDefaultAsync();

        //        if (project == null)
        //        {
        //            return Ok(new { success = false, message = "Project not found." });
        //        }

        //        // Fetch all project employees in one query
        //        var projectEmployees = await _dbContext.ProjectEmployees
        //            .Where(pe => pe.ProjectId == ProjectId)
        //            .Select(pe => pe.EmployeeId)
        //            .ToListAsync();

        //        // Fetch all users for employees in one query
        //        var employeeIds = projectEmployees.Where(e => !string.IsNullOrEmpty(e)).ToList();
        //        var users = await _dbContext.Users
        //            .Where(u => employeeIds.Contains(u.UserID.ToString()))
        //            .Select(u => new
        //            {
        //                UserId = u.UserID.ToString(),
        //                u.FullName
        //            })
        //            .ToListAsync();

        //        // Fetch all tasks for this project in one query
        //        var tasks = await _dbContext.Tasks
        //            .Where(t => t.ProjectId == ProjectId)
        //            .Select(t => new
        //            {
        //                t.EmployeeUserId,
        //                t.ManagerCompleteStatus,
        //                t.TaskEndDate
        //            })
        //            .ToListAsync();

        //        // Build employee details using in-memory data
        //        var employeeDetails = projectEmployees.Select(employeeId =>
        //        {
        //            var employeeName = users.FirstOrDefault(u => u.UserId == employeeId)?.FullName ?? "Unknown";
        //            var employeeTasks = tasks.Where(t => t.EmployeeUserId == employeeId).ToList();

        //            return new
        //            {
        //                EmployeeId = employeeId,
        //                EmployeeName = employeeName,
        //                TotalTasksAssigned = employeeTasks.Count,
        //                CompletedTasks = employeeTasks.Count(t => t.ManagerCompleteStatus == true),
        //                DelayedTasks = employeeTasks.Count(t =>
        //                    (t.ManagerCompleteStatus == false || t.ManagerCompleteStatus == null)
        //                    && t.TaskEndDate < DateTime.Now)
        //            };
        //        }).ToList();

        //        // Build final result
        //        var result = new
        //        {
        //            project.Id,
        //            project.ProjectName,
        //            project.ProjectType,
        //            project.ManagerId,
        //            project.ManagerName,
        //            project.StartDate,
        //            project.EndDate,
        //            project.IsActive,
        //            project.IsCompleted,
        //            project.IsCancelled,
        //            project.Status,
        //            project.CreatedBy,
        //            project.CreatedOn,
        //            project.ModifiedBy,
        //            project.ModifiedOn,
        //            EmployeeDetails = employeeDetails
        //        };

        //        return Ok(new { success = true, data = result });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpGet("GetProjectDetails3ByProjectId")]
        //public async Task<IActionResult> GetProjectDetails3ByProjectId(int ProjectId)
        //{
        //    try
        //    {
        //        if (ProjectId <= 0)
        //        {
        //            return Ok(new { success = false, message = "Project Id is required." });
        //        }

        //        // Single optimized query with all necessary data
        //        var projectData = await (
        //            from p in _dbContext.Project
        //            where p.Id == ProjectId && p.IsActive == true
        //            select new
        //            {
        //                Project = p,
        //                Employees = (from pe in _dbContext.ProjectEmployees
        //                             where pe.ProjectId == p.Id
        //                             select pe.EmployeeId).ToList(),
        //                Tasks = (from t in _dbContext.Tasks
        //                         where t.ProjectId == p.Id
        //                         select new
        //                         {
        //                             t.EmployeeUserId,
        //                             t.ManagerCompleteStatus,
        //                             t.TaskEndDate
        //                         }).ToList()
        //            }).FirstOrDefaultAsync();

        //        if (projectData == null || projectData.Project == null)
        //        {
        //            return Ok(new { success = false, message = "Project not found." });
        //        }

        //        // Fetch employee names only if there are employees
        //        var employeeDetails = new List<object>();

        //        if (projectData.Employees != null && projectData.Employees.Any())
        //        {
        //            var employeeIds = projectData.Employees.Where(e => !string.IsNullOrEmpty(e)).Distinct().ToList();

        //            // Fetch user details in one query
        //            var userDict = await _dbContext.Users
        //                .Where(u => employeeIds.Contains(u.UserID.ToString()))
        //                .Select(u => new
        //                {
        //                    UserId = u.UserID.ToString(),
        //                    u.FullName
        //                })
        //                .ToDictionaryAsync(u => u.UserId, u => u.FullName);

        //            // Build employee details efficiently
        //            var currentDate = DateTime.Now;

        //            employeeDetails = projectData.Employees.Select(employeeId =>
        //            {
        //                var employeeTasks = projectData.Tasks.Where(t => t.EmployeeUserId == employeeId).ToList();

        //                return new
        //                {
        //                    EmployeeId = employeeId,
        //                    EmployeeName = userDict.TryGetValue(employeeId, out var name) ? name : "Unknown",
        //                    TotalTasksAssigned = employeeTasks.Count,
        //                    CompletedTasks = employeeTasks.Count(t => t.ManagerCompleteStatus == true),
        //                    DelayedTasks = employeeTasks.Count(t =>
        //                        (t.ManagerCompleteStatus == false || t.ManagerCompleteStatus == null)
        //                        && t.TaskEndDate.HasValue
        //                        && t.TaskEndDate.Value < currentDate)
        //                };
        //            }).ToList<object>();
        //        }

        //        // Build final result
        //        var result = new
        //        {
        //            projectData.Project.Id,
        //            projectData.Project.ProjectName,
        //            projectData.Project.ProjectType,
        //            projectData.Project.ManagerId,
        //            projectData.Project.ManagerName,
        //            projectData.Project.StartDate,
        //            projectData.Project.EndDate,
        //            projectData.Project.IsActive,
        //            projectData.Project.IsCompleted,
        //            projectData.Project.IsCancelled,
        //            projectData.Project.Status,
        //            projectData.Project.CreatedBy,
        //            projectData.Project.CreatedOn,
        //            projectData.Project.ModifiedBy,
        //            projectData.Project.ModifiedOn,
        //            EmployeeDetails = employeeDetails
        //        };

        //        return Ok(new { success = true, data = result });
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("System", $"Error fetching project details for ProjectId {ProjectId}: {ex.Message}", "Project");
        //        return StatusCode(500, new { success = false, message = "An error occurred while retrieving project details." });
        //    }
        //}

        [HttpGet("GetAllLatestTasksByManagerId")]
        public async Task<IActionResult> GetAllLatestTasksByManagerId(string UserId)
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

                var LatestTasks = (from t in _dbContext.Tasks
                                   join u in _dbContext.Users on t.EmployeeUserId equals u.UserID.ToString().ToLower()
                                   where t.CreatedBy.ToLower() == UserId.ToString().ToLower()
                                   orderby t.CreatedOn descending
                                   select new
                                   {
                                       t.Id,
                                       t.ProjectId,
                                       t.ProjectName,
                                       t.TaskName,
                                       t.TaskDesc,
                                       u.FullName,
                                       t.CreatedOn
                                   }).ToList();
                    

                //if(LatestTasks == null)
                //{
                //    return Ok(new { succcess = false, message = "Task Data Not Found" });
                //}

                return Ok(new { success = true, message = "Task data extracted successfully", data = LatestTasks });


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message); 
            }
        }

        [HttpGet("GetAllLatestProjectsByManagerId")]
        public async Task<IActionResult> GetAllLatestProjectsByManagerId(string UserId)
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

                var LatestProjects = (from p in _dbContext.Project
                                      where p.CreatedBy.ToLower() == UserId.ToLower()
                                      orderby p.CreatedOn descending
                                      select new
                                      {
                                          p.Id,
                                          p.ProjectName,
                                          p.ProjectType,
                                          p.StartDate,
                                          p.EndDate,
                                          p.CreatedOn
                                      }).ToList();


                //if (LatestProjects == null)
                //{
                //    return Ok(new { succcess = false, message = "Project Data Not Found" });
                //}

                return Ok(new { success = true, message = "Project data extracted successfully", data = LatestProjects });


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

    }
}
