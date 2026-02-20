using AuthApplication.Models;
using DMSAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlTypes;
using TMS_API.DBContext;
using TMS_API.Models;
using TMS_API.Services;

namespace TMS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : Controller
    {
        //private readonly IProjectServices _projectServices;
        private readonly AppDbContext _dbContext;

        public ProjectController( AppDbContext dbContext)
        {
            //_projectServices = projectServices;
            _dbContext = dbContext;
        }


        #region Project API's

        [HttpGet("GetAllProjectsDetails")]
        public async Task<IActionResult> GetAllProjectsDetails()
        {
            try
            {
                var result = (from p in _dbContext.Project
                              where p.IsActive == true
                              orderby p.CreatedOn descending
                              select new
                              {
                                  p.Id,
                                  p.ProjectName,
                                  p.ProjectType,
                                  p.StartDate,
                                  p.EndDate,
                                  p.ManagerId,
                                  p.ManagerName,
                                  p.IsActive,
                                  p.IsCompleted,
                                  p.IsCancelled,
                                  p.Status,
                                  p.CreatedOn,
                                  p.CreatedBy,
                                  p.ModifiedOn,
                                  p.ModifiedBy,
                                  Employees = (from pe in _dbContext.ProjectEmployees
                                               where pe.ProjectId == p.Id
                                               select new
                                               {
                                                   pe.ProjectId,
                                                   pe.EmployeeId,
                                                   pe.IsActive
                                               }).ToList()

                              }).ToList();
                if(result == null || result.Count == 0)
                {
                    return Ok(new { success = false, message = "No active projects found.", data = result });
                }

                return Ok(new { success = true, message = "Projects retrieved successfully.", data = result });

            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"An error occurred while retrieving projects: {ex.Message}" });
            }
        }

        [HttpGet("GetProjectById")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Ok(new { success = false, message = "Project Id is required." });
                }

                var result = await (from p in _dbContext.Project
                                    where p.Id == id && p.IsActive == true
                                    select new
                                    {
                                        p.Id,
                                        p.ProjectName,
                                        p.ProjectType,
                                        p.StartDate,
                                        p.EndDate,
                                        p.ManagerId,
                                        p.ManagerName,
                                        p.IsActive,
                                        p.IsCompleted,
                                        p.IsCancelled,
                                        p.Status,
                                        p.CreatedOn,
                                        p.CreatedBy,
                                        p.ModifiedOn,
                                        p.ModifiedBy,
                                        Employees = (from pe in _dbContext.ProjectEmployees
                                                     where pe.ProjectId == p.Id
                                                     select new
                                                     {
                                                         pe.ProjectId,
                                                         pe.EmployeeId,
                                                         pe.IsActive
                                                     }).ToList()
                                    }).FirstOrDefaultAsync();

                if (result == null)
                {
                    return Ok(new { success = false, message = "Project not found.", data = result });
                }

                return Ok(new { success = true, message = "Project retrieved successfully.", data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"An error occurred while retrieving project: {ex.Message}" });
            }
        }

        [HttpGet("GetProjectsByManagerId")]
        public async Task<IActionResult> GetProjectsByManagerId(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return Ok(new { success = false, message = "UserId is required." });
                }

                var userExists = await _dbContext.Users
                    .AnyAsync(x => x.UserID.ToString().ToLower() == userId.ToLower());

                if (!userExists)
                {
                    return Ok(new { success = false, message = "UserId not found." });
                }

                var result = await (from p in _dbContext.Project
                                    where p.CreatedBy == userId && p.IsActive == true
                                    orderby p.CreatedOn descending
                                    select new
                                    {
                                        p.Id,
                                        p.ProjectName,
                                        p.ProjectType,
                                        p.StartDate,
                                        p.EndDate,
                                        p.ManagerId,
                                        p.ManagerName,
                                        p.IsActive,
                                        p.IsCompleted,
                                        p.IsCancelled,
                                        p.Status,
                                        p.CreatedOn,
                                        p.CreatedBy,
                                        p.ModifiedOn,
                                        p.ModifiedBy,
                                        Employees = (from pe in _dbContext.ProjectEmployees
                                                     where pe.ProjectId == p.Id
                                                     select new
                                                     {
                                                         pe.ProjectId,
                                                         pe.EmployeeId,
                                                         pe.IsActive
                                                     }).ToList()
                                    }).ToListAsync();

                if (result == null || result.Count == 0)
                {
                    return Ok(new { success = false, message = "No projects found for this user.", data = result });
                }

                return Ok(new { success = true, message = "Projects retrieved successfully.", data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"An error occurred while retrieving projects: {ex.Message}" });
            }
        }

        [HttpPost("CreateProject")]
        public async Task<IActionResult> CreateProject([FromForm] ProjectDto data)
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(data.ProjectName))
                {
                    return Ok(new { success = false, message = "ProjectName is required." });
                }

                if (string.IsNullOrWhiteSpace(data.UserId))
                {
                    return Ok(new { success = false, message = "UserId is required." });
                }

                if (data.StartDate == null || data.EndDate == null)
                {
                    return Ok(new { success = false, message = "StartDate & EndDate is required." });
                }

                // Date validation
                if (data.EndDate < data.StartDate)
                {
                    return Ok(new { success = false, message = "EndDate must be greater than StartDate." });
                }



                // Duplicate check based on ProjectName and ProjectType
                var existingProject = _dbContext.Project.FirstOrDefault(p =>
                    p.ProjectName.ToLower().Trim() == data.ProjectName.ToLower().Trim()
                    && p.ProjectType.ToLower().Trim() == data.ProjectType.ToLower().Trim());

                if (existingProject != null)
                {
                    return Ok(new { success = false, message = "A project with the same ProjectName and ProjectType already exists. Please enter different details." });
                }

                // Get Manager Name from UserId
                var manager = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserID.ToString().ToLower() == data.UserId.ToString().ToLower());

                if (manager == null)
                {
                    return Ok(new { success = false, message = "User not found. Please provide a valid UserId." });
                }

                var managerName = manager.FullName ?? "Unknown Manager";

                // Validate ProjectEmployees list
                if (data.ProjectEmployeesDto == null || data.ProjectEmployeesDto.Count <= 0)
                {
                    return Ok(new { success = false, message = "At least one employee must be assigned to the project." });
                }

                // Filter unique employees based on EmployeeId
                var uniqueEmployees = data.ProjectEmployeesDto
                                        .GroupBy(e => e.EmployeeId?.ToLower().Trim())
                                        .Select(g => g.First())
                                        .Where(e => !string.IsNullOrWhiteSpace(e.EmployeeId))
                                        .ToList();

                if (uniqueEmployees.Count == 0)
                {
                    return Ok(new { success = false, message = "No valid employees found. Please provide valid EmployeeId(s)." });
                }

                // Create new Project
                var newProject = new Project
                {
                    ProjectName = data.ProjectName,
                    ProjectType = data.ProjectType,
                    StartDate = data.StartDate,
                    EndDate = data.EndDate,
                    ManagerId = data.UserId,
                    ManagerName = managerName,
                    IsActive = data.IsActive ?? true,
                    IsCompleted = false,
                    IsCancelled = false,
                    Status = data.Status ?? "In Progress",
                    CreatedBy = data.UserId,
                    CreatedOn = DateTime.Now
                };

                await _dbContext.Project.AddAsync(newProject);
                await _dbContext.SaveChangesAsync();
                int projectId = newProject.Id;

                // Save unique ProjectEmployees records
                var projectEmployeesList = uniqueEmployees.Select(empDto => new ProjectEmployees
                {
                    ProjectId = projectId,
                    EmployeeId = empDto.EmployeeId,
                    IsActive = empDto.IsActive ?? true,
                    CreatedBy = data.UserId,
                    CreatedOn = DateTime.Now
                }).ToList();

                await _dbContext.ProjectEmployees.AddRangeAsync(projectEmployeesList);
                await _dbContext.SaveChangesAsync();

                // Log creation
                Log.DataLog($"{data.UserId}",
                    $"Project Id {newProject.Id} created with name '{newProject.ProjectName}' and type '{newProject.ProjectType}' by {data.UserId}. {uniqueEmployees.Count} employee(s) assigned.",
                    "Project");

                // Create UserActivityLog
                var userActivityLog = new UserActivityLog
                {
                    SNType = "Project Creation",
                    SNTital = "Project Created",
                    SNDescription = $"Project '{data.ProjectName}' Type: '{data.ProjectType}' was successfully created by user '{managerName}' with {uniqueEmployees.Count} employee(s) assigned.",
                    SNActionUserId = data.UserId,
                    CreatedOn = DateTime.Now,
                    IsActive = true,
                    IsRead = false
                };

                _dbContext.UserActivityLog.Add(userActivityLog);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Project created successfully with {uniqueEmployees.Count} employee(s) assigned.",
                    //data = new
                    //{
                    //    ProjectId = newProject.Id,
                    //    EmployeesAssigned = uniqueEmployees.Count,
                    //    DuplicatesRemoved = data.ProjectEmployeesDto.Count - uniqueEmployees.Count
                    //}
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error($"{data?.UserId ?? "Unknown"}", $"Error creating project: {ex.Message}", "Project");
                return Ok(new { success = false, message = $"An error occurred while creating the project: {ex.Message}" });
            }
        }

        [HttpPost("UpdateProject")]
        public async Task<IActionResult> UpdateProject([FromForm] ProjectDto data)
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Required field validations
                if (data.Id <= 0)
                {
                    return Ok(new { success = false, message = "Project Id is required." });
                }

                if (string.IsNullOrWhiteSpace(data.UserId))
                {
                    return Ok(new { success = false, message = "UserId is required." });
                }

                var existingProject = await _dbContext.Project.FirstOrDefaultAsync(p => p.Id == data.Id);
                if (existingProject == null)
                {
                    return Ok(new { success = false, message = "Project not found. Please provide a valid Project Id." });
                }


                if (data.StartDate != null && data.EndDate != null)
                {
                    if (data.EndDate < data.StartDate)
                    {
                        return Ok(new { success = false, message = "EndDate must be greater than StartDate." });
                    }
                }

                // Duplicate check based on ProjectName and ProjectType (excluding current project)
                if (!string.IsNullOrWhiteSpace(data.ProjectName) && !string.IsNullOrWhiteSpace(data.ProjectType))
                {
                    var duplicateProject = _dbContext.Project.FirstOrDefault(p =>
                        p.Id != data.Id &&
                        p.ProjectName.ToLower().Trim() == data.ProjectName.ToLower().Trim() &&
                        p.ProjectType.ToLower().Trim() == data.ProjectType.ToLower().Trim());

                    if (duplicateProject != null)
                    {
                        return Ok(new { success = false, message = "A project with the same ProjectName and ProjectType already exists. Please enter different details." });
                    }
                }

                // Track changes for logging
                var changes = new List<string>();

                // Update only the fields that are provided/changed
                if (!string.IsNullOrWhiteSpace(data.ProjectName) && data.ProjectName != existingProject.ProjectName)
                {
                    changes.Add($"ProjectName changed from '{existingProject.ProjectName}' to '{data.ProjectName}'");
                    existingProject.ProjectName = data.ProjectName;
                }

                if (!string.IsNullOrWhiteSpace(data.ProjectType) && data.ProjectType != existingProject.ProjectType)
                {
                    changes.Add($"ProjectType changed from '{existingProject.ProjectType}' to '{data.ProjectType}'");
                    existingProject.ProjectType = data.ProjectType;
                }

                if (data.StartDate != null && data.StartDate != existingProject.StartDate)
                {
                    changes.Add($"StartDate changed from '{existingProject.StartDate?.ToString("yyyy-MM-dd")}' to '{data.StartDate?.ToString("yyyy-MM-dd")}'");
                    existingProject.StartDate = data.StartDate;
                }

                if (data.EndDate != null && data.EndDate != existingProject.EndDate)
                {
                    changes.Add($"EndDate changed from '{existingProject.EndDate?.ToString("yyyy-MM-dd")}' to '{data.EndDate?.ToString("yyyy-MM-dd")}'");
                    existingProject.EndDate = data.EndDate;
                }

                if (!string.IsNullOrWhiteSpace(data.ManagerId) && data.ManagerId != existingProject.ManagerId)
                {
                    // Get new Manager Name from ManagerId
                    var newManager = await _dbContext.Users.FirstOrDefaultAsync(u =>
                        u.UserID.ToString().ToLower() == data.ManagerId.ToString().ToLower());

                    if (newManager == null)
                    {
                        return Ok(new { success = false, message = "Manager not found. Please provide a valid ManagerId." });
                    }

                    var newManagerName = newManager.FullName ?? "Unknown Manager";
                    changes.Add($"Manager changed from '{existingProject.ManagerName}' to '{newManagerName}'");
                    existingProject.ManagerId = data.ManagerId;
                    existingProject.ManagerName = newManagerName;
                }

                if (data.IsActive != null && data.IsActive != existingProject.IsActive)
                {
                    changes.Add($"IsActive changed from '{existingProject.IsActive}' to '{data.IsActive}'");
                    existingProject.IsActive = data.IsActive;
                }

                if (data.IsCompleted != null && data.IsCompleted != existingProject.IsCompleted)
                {
                    changes.Add($"IsCompleted changed from '{existingProject.IsCompleted}' to '{data.IsCompleted}'");
                    existingProject.IsCompleted = data.IsCompleted;
                }

                if (data.IsCancelled != null && data.IsCancelled != existingProject.IsCancelled)
                {
                    changes.Add($"IsCancelled changed from '{existingProject.IsCancelled}' to '{data.IsCancelled}'");
                    existingProject.IsCancelled = data.IsCancelled;
                }

                if (!string.IsNullOrWhiteSpace(data.Status) && data.Status != existingProject.Status)
                {
                    changes.Add($"Status changed from '{existingProject.Status}' to '{data.Status}'");
                    existingProject.Status = data.Status;
                }

                // Set ModifiedBy and ModifiedOn
                existingProject.ModifiedBy = data.UserId;
                existingProject.ModifiedOn = DateTime.Now;

                _dbContext.Project.Update(existingProject);
                await _dbContext.SaveChangesAsync();

                // Handle ProjectEmployees update
                int employeesAddedCount = 0;
                int employeesRemovedCount = 0;

                if (data.ProjectEmployeesDto != null && data.ProjectEmployeesDto.Count > 0)
                {
                    // Remove all existing ProjectEmployees for this ProjectId
                    var existingEmployees = _dbContext.ProjectEmployees
                        .Where(pe => pe.ProjectId == data.Id)
                        .ToList();

                    employeesRemovedCount = existingEmployees.Count;

                    if (existingEmployees.Any())
                    {
                        _dbContext.ProjectEmployees.RemoveRange(existingEmployees);
                        await _dbContext.SaveChangesAsync();
                    }

                    // Filter unique employees based on EmployeeId
                    var uniqueEmployees = data.ProjectEmployeesDto
                        .GroupBy(e => e.EmployeeId?.ToLower().Trim())
                        .Select(g => g.First())
                        .Where(e => !string.IsNullOrWhiteSpace(e.EmployeeId))
                        .ToList();

                    if (uniqueEmployees.Count == 0)
                    {
                        return Ok(new { success = false, message = "No valid employees found. Please provide valid EmployeeId(s)." });
                    }

                    // Save unique ProjectEmployees records
                    var projectEmployeesList = uniqueEmployees.Select(empDto => new ProjectEmployees
                    {
                        ProjectId = data.Id,
                        EmployeeId = empDto.EmployeeId,
                        IsActive = empDto.IsActive ?? true,
                        CreatedBy = data.UserId,
                        CreatedOn = DateTime.Now
                    }).ToList();

                    await _dbContext.ProjectEmployees.AddRangeAsync(projectEmployeesList);
                    await _dbContext.SaveChangesAsync();

                    employeesAddedCount = uniqueEmployees.Count;
                    changes.Add($"ProjectEmployees updated: {employeesRemovedCount} removed, {employeesAddedCount} added");
                }

                // Get user details for logging
                var user = await _dbContext.Users.FirstOrDefaultAsync(u =>
                    u.UserID.ToString().ToLower() == data.UserId.ToString().ToLower());
                var modifiedByName = user?.FullName ?? "Unknown User";

                // Log updates
                if (changes.Any())
                {
                    var changesSummary = string.Join("; ", changes);
                    Log.DataLog($"{data.UserId}",
                        $"Project Id {existingProject.Id} ('{existingProject.ProjectName}') updated by {data.UserId}. Changes: {changesSummary}",
                        "Project");

                    // Create UserActivityLog
                    var userActivityLog = new UserActivityLog
                    {
                        SNType = "Project Update",
                        SNTital = "Project Updated",
                        SNDescription = $"Project '{existingProject.ProjectName}' was successfully updated by user '{modifiedByName}'. Changes: {changesSummary}",
                        SNActionUserId = data.UserId,
                        CreatedOn = DateTime.Now,
                        IsActive = true,
                        IsRead = false
                    };

                    _dbContext.UserActivityLog.Add(userActivityLog);
                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                var responseMessage = changes.Any()
                    ? $"Project updated successfully. {changes.Count} change(s) applied."
                    : "No changes detected. Project remains unchanged.";

                return Ok(new
                {
                    success = true,
                    message = responseMessage,
                    //data = new
                    //{
                    //    ProjectId = existingProject.Id,
                    //    ChangesApplied = changes.Count,
                    //    EmployeesUpdated = data.ProjectEmployeesDto != null && data.ProjectEmployeesDto.Count > 0,
                    //    EmployeesRemoved = employeesRemovedCount,
                    //    EmployeesAdded = employeesAddedCount,
                    //    DuplicatesRemoved = data.ProjectEmployeesDto != null
                    //        ? data.ProjectEmployeesDto.Count - employeesAddedCount
                    //        : 0
                    //}
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error($"{data?.UserId ?? "Unknown"}",
                    $"Error updating project Id {data?.Id}: {ex.Message}",
                    "Project");
                return Ok(new { success = false, message = $"An error occurred while updating the project: {ex.Message}" });
            }
        }

        [HttpPost("UpdateProjectCompleteOrCancelledStatusById")]
        public async Task<IActionResult> UpdateProjectCompleteOrCancelledStatusById([FromBody] ProjectUpdateStatusDto data)
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Required field validations
                if (data.ProjectId <= 0)
                {
                    return Ok(new { success = false, message = "ProjectId is required." });
                }

                if (string.IsNullOrWhiteSpace(data.UserId))
                {
                    return Ok(new { success = false, message = "UserId is required." });
                }

                // Check if project exists
                var existingProject = await _dbContext.Project.FirstOrDefaultAsync(p => p.Id == data.ProjectId);
                if (existingProject == null)
                {
                    return Ok(new { success = false, message = $"Project with Id {data.ProjectId} not found. Please provide a valid ProjectId." });
                }

                // Validation: Cannot set both IsCompleted and IsCancelled to true at the same time
                if (data.IsCompleted == true && data.IsCancelled == true)
                {
                    return Ok(new { success = false, message = "A project cannot be marked as both Completed and Cancelled at the same time. Please select only one status." });
                }

                // Validation: At least one status field must be provided
                if (data.IsCompleted == null && data.IsCancelled == null)
                {
                    return Ok(new { success = false, message = "Please provide at least one status update (IsCompleted or IsCancelled)." });
                }

                // Track changes for logging
                var changes = new List<string>();
                string statusMessage = "";
                string oldStatus = existingProject.Status;
                bool? oldIsCompleted = existingProject.IsCompleted;
                bool? oldIsCancelled = existingProject.IsCancelled;

                // Update IsCompleted status
                if (data.IsCompleted != null && data.IsCompleted != existingProject.IsCompleted)
                {
                    existingProject.IsCompleted = data.IsCompleted;
                    changes.Add($"IsCompleted changed from '{oldIsCompleted}' to '{data.IsCompleted}'");

                    if (data.IsCompleted == true)
                    {
                        existingProject.Status = "Completed";
                        existingProject.IsActive = true;
                        statusMessage = "Project marked as Completed successfully.";
                    }
                    else
                    {
                        existingProject.Status = "In Progress";
                        existingProject.IsActive = true;
                        statusMessage = "Project completion status removed. Status set to In Progress.";
                    }
                }

                // Update IsCancelled status
                if (data.IsCancelled != null && data.IsCancelled != existingProject.IsCancelled)
                {
                    existingProject.IsCancelled = data.IsCancelled;
                    changes.Add($"IsCancelled changed from '{oldIsCancelled}' to '{data.IsCancelled}'");

                    if (data.IsCancelled == true)
                    {
                        existingProject.Status = "Cancelled";
                        existingProject.IsActive = true;
                        statusMessage = "Project marked as Cancelled successfully.";
                    }
                    else
                    {
                        existingProject.Status = "In Progress";
                        existingProject.IsActive = true;
                        statusMessage = "Project cancellation status removed. Status set to In Progress.";
                    }
                }

                // If no changes detected
                if (!changes.Any())
                {
                    return Ok(new
                    {
                        success = false,
                        message = "No status changes detected. Project status remains unchanged."
                    });
                }

                // Update ModifiedBy and ModifiedOn
                existingProject.ModifiedBy = data.UserId;
                existingProject.ModifiedOn = DateTime.Now;

                _dbContext.Project.Update(existingProject);
                await _dbContext.SaveChangesAsync();

                // Get user details for logging
                var user = await _dbContext.Users.FirstOrDefaultAsync(u =>
                    u.UserID.ToString().ToLower() == data.UserId.ToString().ToLower());
                var modifiedByName = user?.FullName ?? "Unknown User";

                // Log status update
                var changesSummary = string.Join("; ", changes);
                Log.DataLog($"{data.UserId}",
                    $"Project Id {existingProject.Id} ('{existingProject.ProjectName}') status updated by {data.UserId}. Changes: {changesSummary}. New Status: '{existingProject.Status}'",
                    "Project");

                // Create UserActivityLog
                var userActivityLog = new UserActivityLog
                {
                    SNType = "Project Status Update",
                    SNTital = "Project Status Changed",
                    SNDescription = $"Project '{existingProject.ProjectName}' (Id: {existingProject.Id}) status was updated by '{modifiedByName}'. {changesSummary}. Current Status: '{existingProject.Status}'",
                    SNActionUserId = data.UserId,
                    CreatedOn = DateTime.Now,
                    IsActive = true,
                    IsRead = false
                };

                _dbContext.UserActivityLog.Add(userActivityLog);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = statusMessage,
                    //data = new
                    //{
                    //    ProjectId = existingProject.Id,
                    //    ProjectName = existingProject.ProjectName,
                    //    PreviousStatus = oldStatus,
                    //    CurrentStatus = existingProject.Status,
                    //    IsCompleted = existingProject.IsCompleted,
                    //    IsCancelled = existingProject.IsCancelled,
                    //    IsActive = existingProject.IsActive,
                    //    UpdatedBy = modifiedByName,
                    //    UpdatedOn = existingProject.ModifiedOn
                    //}
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error($"{data?.UserId ?? "Unknown"}",
                    $"Error updating status for project Id {data?.ProjectId}: {ex.Message}",
                    "Project");
                return Ok(new { success = false, message = $"An error occurred while updating project status: {ex.Message}" });
            }
        }

        [HttpPost("DeleteProjectById")]
        public async Task<IActionResult> DeleteProjectById(int projectId, string? userId)
        {
            if (projectId <= 0)
            {
                return Ok(new { success = false, message = "Invalid Project Id." });
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Ok(new { success = false, message = "UserId is required." });
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserID.ToString().ToLower() == userId.ToLower());

            if (user == null)
            {
                return Ok(new { success = false, message = "User not found." });
            }

            var project = await _dbContext.Project.FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return Ok(new { success = false, message = "Project not found." });
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // 🔹 Delete Project Employees
                var projectEmployees = await _dbContext.ProjectEmployees
                                            .Where(pe => pe.ProjectId == projectId)
                                            .ToListAsync();

                if (projectEmployees.Any())
                    _dbContext.ProjectEmployees.RemoveRange(projectEmployees);

                // 🔹 Delete Project Documents
                var documents = await _dbContext.DocumentMaster
                    .Where(d => d.DocumentId == projectId.ToString()
                                && d.DocumentType == "ProjectDocuments")
                    .ToListAsync();

                if (documents.Any())
                    _dbContext.DocumentMaster.RemoveRange(documents);

                _dbContext.Project.Remove(project);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                Log.DataLog(userId, $"Project Id {projectId} ('{project.ProjectName}') deleted successfully by {user.FullName}.", "Project");

                var userActivityLog = new UserActivityLog
                {
                    SNType = "Project Deletion",
                    SNTital = "Project Deleted",
                    SNDescription = $"Project '{project.ProjectName}' (Id: {projectId}) was deleted by '{user.FullName}'.",
                    SNActionUserId = userId,
                    CreatedOn = DateTime.Now,
                    IsActive = true,
                    IsRead = false
                };

                _dbContext.UserActivityLog.Add(userActivityLog);
                await _dbContext.SaveChangesAsync();
                return Ok(new
                {
                    success = true,
                    message = "Project and related records deleted successfully."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error(userId, $"Error deleting project {projectId}: {ex.Message}", "Project");

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the project."
                });
            }
        }


        #endregion
    }
}
