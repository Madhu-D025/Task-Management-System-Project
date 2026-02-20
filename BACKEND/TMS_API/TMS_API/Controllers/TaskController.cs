using AuthApplication.Models;
using DMSAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS_API.DBContext;
using TMS_API.Models;
using TMS_API.Services;
using WM.Services;

namespace TMS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly DocumentService _docservice;

        public TaskController(AppDbContext dbContext, DocumentService docservice)
        {
            _dbContext = dbContext;
            _docservice = docservice;
        }


        #region Task Creation Related API's

        [HttpGet("GetAllTasks")]
        public async Task<IActionResult> GetAllTasks()
        {
            try
            {
                var TaskDetails = await (from t in _dbContext.Tasks
                                         where t.IsActive == true
                                         orderby t.CreatedOn descending
                                         select new
                                         {
                                             t.Id,
                                             t.ProjectId,
                                             t.ProjectName,
                                             t.ManagerNames,
                                             t.EmployeeUserId,
                                             t.TaskName,
                                             t.TaskDesc,
                                             t.TaskStartDate,
                                             t.TaskEndDate,
                                             t.IsActive,
                                             t.ManagerCompleteStatus,
                                             t.Status,
                                             t.CreatedOn,
                                             t.CreatedBy,
                                             t.ModifiedOn,
                                             t.ModifiedBy,
                                             Documents = (from doc in _dbContext.DocumentMaster
                                                          where  doc.DocumentId == t.ProjectId.ToString() && doc.InitiationId == t.Id && doc.DocumentType == "TaskDocuments"
                                                          select new
                                                          {
                                                              doc.Id,
                                                              doc.DocumentId,
                                                              doc.DocumentType,
                                                              doc.DocumentPath,
                                                              doc.DocumentURL,
                                                              doc.CreatedBy,
                                                              doc.CreatedOn
                                                          }).ToList(),
                                         }).ToListAsync();

                if (TaskDetails == null || TaskDetails.Count == 0)
                {
                    return Ok(new { success = false, message = "No active tasks found.", data = TaskDetails });
                }

                return Ok(new { success = true, message = "Tasks retrieved successfully.", data = TaskDetails });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"An error occurred while retrieving tasks: {ex.Message}" });
            }
        }

        [HttpGet("GetTaskById")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Ok(new { success = false, message = "Task Id is required." });
                }

                var TaskDetails = await (from t in _dbContext.Tasks
                                         where t.Id == id && t.IsActive == true
                                         select new
                                         {
                                             t.Id,
                                             t.ProjectId,
                                             t.ProjectName,
                                             t.ManagerNames,
                                             t.EmployeeUserId,
                                             t.TaskName,
                                             t.TaskDesc,
                                             t.TaskStartDate,
                                             t.TaskEndDate,
                                             t.IsActive,
                                             t.ManagerCompleteStatus,
                                             t.Status,
                                             t.CreatedOn,
                                             t.CreatedBy,
                                             t.ModifiedOn,
                                             t.ModifiedBy,
                                             Documents = (from doc in _dbContext.DocumentMaster
                                                          where doc.DocumentId == t.ProjectId.ToString() && doc.InitiationId == t.Id && doc.DocumentType == "TaskDocuments"
                                                          select new
                                                          {
                                                              doc.Id,
                                                              doc.DocumentId,
                                                              doc.DocumentType,
                                                              doc.DocumentPath,
                                                              doc.DocumentURL,
                                                              doc.CreatedBy,
                                                              doc.CreatedOn
                                                          }).ToList(),
                                         }).FirstOrDefaultAsync();

                if (TaskDetails == null)
                {
                    return Ok(new { success = false, message = "Task not found.", data = TaskDetails });
                }

                return Ok(new { success = true, message = "Task retrieved successfully.", data = TaskDetails });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"An error occurred while retrieving task: {ex.Message}" });
            }
        }

        [HttpGet("GetAllTasksByProjectId")]
        public async Task<IActionResult> GetAllTasksByProjectId(int projectId)
        {
            try
            {
                if (projectId <= 0)
                {
                    return Ok(new { success = false, message = "Project Id is required." });
                }

                var TaskDetails = await (from t in _dbContext.Tasks
                                         where t.ProjectId == projectId && t.IsActive == true
                                         orderby t.CreatedOn descending
                                         select new
                                         {
                                             t.Id,
                                             t.ProjectId,
                                             t.ProjectName,
                                             t.ManagerNames,
                                             t.EmployeeUserId,
                                             t.TaskName,
                                             t.TaskDesc,
                                             t.TaskStartDate,
                                             t.TaskEndDate,
                                             t.IsActive,
                                             t.ManagerCompleteStatus,
                                             t.Status,
                                             t.CreatedOn,
                                             t.CreatedBy,
                                             t.ModifiedOn,
                                             t.ModifiedBy,
                                             Documents = (from doc in _dbContext.DocumentMaster
                                                          where doc.DocumentId == t.ProjectId.ToString() && doc.InitiationId == t.Id  && doc.DocumentType == "TaskDocuments"
                                                          select new
                                                          {
                                                              doc.Id,
                                                              doc.DocumentId,
                                                              doc.DocumentType,
                                                              doc.DocumentPath,
                                                              doc.DocumentURL,
                                                              doc.CreatedBy,
                                                              doc.CreatedOn
                                                          }).ToList(),
                                         }).ToListAsync();

                if (TaskDetails == null || TaskDetails.Count == 0)
                {
                    return Ok(new { success = false, message = "No tasks found for this project.", data = TaskDetails });
                }

                return Ok(new { success = true, message = "Tasks retrieved successfully.", data = TaskDetails });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"An error occurred while retrieving tasks: {ex.Message}" });
            }
        }

        [HttpPost("CreateTask")]
        public async Task<IActionResult> CreateTask([FromForm] TaskDto data)
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Required field validations
                if (string.IsNullOrWhiteSpace(data.UserId))
                {
                    return Ok(new { success = false, message = "UserId is required." });
                }

                if (data.ProjectId == null || data.ProjectId <= 0)
                {
                    return Ok(new { success = false, message = "ProjectId is required." });
                }

                if (string.IsNullOrWhiteSpace(data.ProjectName))
                {
                    return Ok(new { success = false, message = "ProjectName is required." });
                }

                if (data.TaskListDto == null || data.TaskListDto.Count == 0)
                {
                    return Ok(new { success = false, message = "At least one task must be provided." });
                }

                // Validate ProjectId exists
                var projectExists = await _dbContext.Project.FirstOrDefaultAsync(p => p.Id == data.ProjectId);
                if (projectExists == null)
                {
                    return Ok(new { success = false, message = "Project not found. Please provide a valid ProjectId." });
                }

                // Get Manager Name based on UserId
                var manager = await _dbContext.Users.FirstOrDefaultAsync(u =>
                    u.UserID.ToString().ToLower() == data.UserId.ToString().ToLower());
                var managerName = manager?.FullName ?? "Unknown";

                // Validate each task in the list
                foreach (var task in data.TaskListDto)
                {
                    if (string.IsNullOrWhiteSpace(task.TaskName))
                    {
                        return Ok(new { success = false, message = "TaskName is required for all tasks." });
                    }

                    if (string.IsNullOrWhiteSpace(task.EmployeeUserId))
                    {
                        return Ok(new { success = false, message = "EmployeeUserId is required for all tasks." });
                    }

                    // Validate TaskEndDate >= TaskStartDate
                    if (task.TaskStartDate != null && task.TaskEndDate != null)
                    {
                        if (task.TaskEndDate < task.TaskStartDate)
                        {
                            return Ok(new { success = false, message = $"TaskEndDate must be greater than or equal to TaskStartDate for task '{task.TaskName}'." });
                        }
                    }
                }

                // Check for duplicate tasks (same ProjectId, TaskName, and EmployeeUserId)
                var duplicateTasks = new List<string>();
                foreach (var task in data.TaskListDto)
                {
                    var existingTask = _dbContext.Tasks.FirstOrDefault(t =>
                        t.ProjectId == data.ProjectId &&
                        t.TaskName.ToLower().Trim() == task.TaskName.ToLower().Trim() &&
                        t.EmployeeUserId.ToLower().Trim() == task.EmployeeUserId.ToLower().Trim());

                    if (existingTask != null)
                    {
                        duplicateTasks.Add($"Task '{task.TaskName}' for Employee '{task.EmployeeUserId}'");
                    }
                }

                if (duplicateTasks.Any())
                {
                    return Ok(new
                    {
                        success = false,
                        message = $"Duplicate tasks found: {string.Join(", ", duplicateTasks)}. These tasks already exist for the same project and employees."
                    });
                }

                // Create tasks - CHANGED: List<Task> to List<Tasks>
                var createdTasks = new List<Tasks>();
                var totalDocumentsUploaded = 0;

                foreach (var taskItem in data.TaskListDto)
                {
                    var newTask = new Tasks
                    {
                        ProjectId = data.ProjectId,
                        ProjectName = data.ProjectName,
                        ManagerNames = managerName,
                        EmployeeUserId = taskItem.EmployeeUserId,
                        TaskName = taskItem.TaskName,
                        TaskDesc = taskItem.TaskDesc,
                        TaskStartDate = taskItem.TaskStartDate,
                        TaskEndDate = taskItem.TaskEndDate,
                        IsActive = true, 
                        Status = "In Progress", 
                        ManagerCompleteStatus = false,
                        CreatedBy = data.UserId,
                        CreatedOn = DateTime.Now
                    };

                    await _dbContext.Tasks.AddAsync(newTask);
                    await _dbContext.SaveChangesAsync();

                    createdTasks.Add(newTask);

                    // Upload documents if provided
                    if (taskItem.Documents != null && taskItem.Documents.Any())
                    {
                        var documentDto = new Documentdto
                        {
                            Documents = taskItem.Documents,
                            DocumentId = data.ProjectId.ToString(),
                            InitiationId = data.Id,
                            documentType = "TaskDocuments",
                            FolderName = "TaskDocuments",
                            UserID = data.UserId
                        };

                        await _docservice.DocumentsUpload(documentDto);
                        totalDocumentsUploaded += taskItem.Documents.Count;
                    }
                }

                // Log creation
                Log.DataLog($"{data.UserId}",
                    $"{createdTasks.Count} task(s) created for ProjectId {data.ProjectId} ('{data.ProjectName}') by {data.UserId}. {totalDocumentsUploaded} document(s) uploaded.",
                    "Task");

                // Create UserActivityLog
                var userActivityLog = new UserActivityLog
                {
                    SNType = "Task Creation",
                    SNTital = "Tasks Created",
                    SNDescription = $"{createdTasks.Count} task(s) were successfully created for project '{data.ProjectName}' (ProjectId: {data.ProjectId}) by manager '{managerName}'. {totalDocumentsUploaded} document(s) attached.",
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
                    message = $"{createdTasks.Count} Tasks created successfully with for the Project: {data.ProjectName}",
                    //data = new
                    //{
                    //    TasksCreated = createdTasks.Count,
                    //    ProjectId = data.ProjectId,
                    //    ProjectName = data.ProjectName,
                    //    DocumentsUploaded = totalDocumentsUploaded,
                    //    TaskIds = createdTasks.Select(t => t.Id).ToList()
                    //}
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error($"{data?.UserId ?? "Unknown"}",
                    $"Error creating tasks: {ex.Message}",
                    "Task");
                return Ok(new { success = false, message = $"An error occurred while creating tasks: {ex.Message}" });
            }
        }

        [HttpPost("UpdateTask")]
        public async Task<IActionResult> UpdateTask([FromForm] TaskUpdateDto data)
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Required field validations
                if (data.Id <= 0)
                {
                    return Ok(new { success = false, message = "Task Id is required." });
                }

                if (string.IsNullOrWhiteSpace(data.UserId))
                {
                    return Ok(new { success = false, message = "UserId is required." });
                }

                // Check if task exists
                var existingTask = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == data.Id);
                if (existingTask == null)
                {
                    return Ok(new { success = false, message = "Task not found. Please provide a valid Task Id." });
                }

                // Validate TaskEndDate >= TaskStartDate
                var startDate = data.TaskStartDate ?? existingTask.TaskStartDate;
                var endDate = data.TaskEndDate ?? existingTask.TaskEndDate;

                if (startDate != null && endDate != null && endDate < startDate)
                {
                    return Ok(new { success = false, message = "TaskEndDate must be greater than or equal to TaskStartDate." });
                }

                // Check for duplicate task (excluding current task)
                if (!string.IsNullOrWhiteSpace(data.TaskName) && !string.IsNullOrWhiteSpace(data.EmployeeUserId))
                {
                    var projectId = data.ProjectId ?? existingTask.ProjectId;
                    var duplicateTask = _dbContext.Tasks.FirstOrDefault(t =>
                        t.Id != data.Id &&
                        t.ProjectId == projectId &&
                        t.TaskName.ToLower().Trim() == data.TaskName.ToLower().Trim() &&
                        t.EmployeeUserId.ToLower().Trim() == data.EmployeeUserId.ToLower().Trim());

                    if (duplicateTask != null)
                    {
                        return Ok(new
                        {
                            success = false,
                            message = $"A task with name '{data.TaskName}' already exists for employee '{data.EmployeeUserId}' in this project."
                        });
                    }
                }

                // Track changes for logging
                var changes = new List<string>();

                // Update ProjectId if changed
                if (data.ProjectId != null && data.ProjectId > 0 && data.ProjectId != existingTask.ProjectId)
                {
                    var projectExists = await _dbContext.Project.FirstOrDefaultAsync(p => p.Id == data.ProjectId);
                    if (projectExists == null)
                    {
                        return Ok(new { success = false, message = "Project not found. Please provide a valid ProjectId." });
                    }

                    changes.Add($"ProjectId changed from '{existingTask.ProjectId}' to '{data.ProjectId}'");
                    existingTask.ProjectId = data.ProjectId;
                }

                // Update ProjectName
                if (!string.IsNullOrWhiteSpace(data.ProjectName) && data.ProjectName != existingTask.ProjectName)
                {
                    changes.Add($"ProjectName changed from '{existingTask.ProjectName}' to '{data.ProjectName}'");
                    existingTask.ProjectName = data.ProjectName;
                }

                // Update Manager Name based on UserId
                var manager = await _dbContext.Users.FirstOrDefaultAsync(u =>
                    u.UserID.ToString().ToLower() == data.UserId.ToString().ToLower());
                var managerName = manager?.FullName ?? "Unknown";

                if (managerName != existingTask.ManagerNames)
                {
                    changes.Add($"ManagerNames changed from '{existingTask.ManagerNames}' to '{managerName}'");
                    existingTask.ManagerNames = managerName;
                }

                // Update EmployeeUserId
                if (!string.IsNullOrWhiteSpace(data.EmployeeUserId) && data.EmployeeUserId != existingTask.EmployeeUserId)
                {
                    changes.Add($"EmployeeUserId changed from '{existingTask.EmployeeUserId}' to '{data.EmployeeUserId}'");
                    existingTask.EmployeeUserId = data.EmployeeUserId;
                }

                // Update TaskName
                if (!string.IsNullOrWhiteSpace(data.TaskName) && data.TaskName != existingTask.TaskName)
                {
                    changes.Add($"TaskName changed from '{existingTask.TaskName}' to '{data.TaskName}'");
                    existingTask.TaskName = data.TaskName;
                }

                // Update TaskDesc
                if (!string.IsNullOrWhiteSpace(data.TaskDesc) && data.TaskDesc != existingTask.TaskDesc)
                {
                    changes.Add($"TaskDesc updated");
                    existingTask.TaskDesc = data.TaskDesc;
                }

                // Update TaskStartDate
                if (data.TaskStartDate != null && data.TaskStartDate != existingTask.TaskStartDate)
                {
                    changes.Add($"TaskStartDate changed from '{existingTask.TaskStartDate?.ToString("yyyy-MM-dd")}' to '{data.TaskStartDate?.ToString("yyyy-MM-dd")}'");
                    existingTask.TaskStartDate = data.TaskStartDate;
                }

                // Update TaskEndDate
                if (data.TaskEndDate != null && data.TaskEndDate != existingTask.TaskEndDate)
                {
                    changes.Add($"TaskEndDate changed from '{existingTask.TaskEndDate?.ToString("yyyy-MM-dd")}' to '{data.TaskEndDate?.ToString("yyyy-MM-dd")}'");
                    existingTask.TaskEndDate = data.TaskEndDate;
                }

                // Update IsActive
                if (data.IsActive != null && data.IsActive != existingTask.IsActive)
                {
                    changes.Add($"IsActive changed from '{existingTask.IsActive}' to '{data.IsActive}'");
                    existingTask.IsActive = data.IsActive;
                }

                // Update Status
                if (!string.IsNullOrWhiteSpace(data.Status) && data.Status != existingTask.Status)
                {
                    changes.Add($"Status changed from '{existingTask.Status}' to '{data.Status}'");
                    existingTask.Status = data.Status;
                }

                // Update ManagerCompleteStatus
                if (data.ManagerCompleteStatus != null && data.ManagerCompleteStatus != existingTask.ManagerCompleteStatus)
                {
                    changes.Add($"ManagerCompleteStatus changed from '{existingTask.ManagerCompleteStatus}' to '{data.ManagerCompleteStatus}'");
                    existingTask.ManagerCompleteStatus = data.ManagerCompleteStatus;
                }

                // Set ModifiedBy and ModifiedOn
                existingTask.ModifiedBy = data.UserId;
                existingTask.ModifiedOn = DateTime.Now;

                _dbContext.Tasks.Update(existingTask);
                await _dbContext.SaveChangesAsync();

                // Handle Documents Update - Delete existing and upload new
                int documentsDeleted = 0;
                int documentsUploaded = 0;

                if (data.Documents != null && data.Documents.Any())
                {
                    // Delete existing documents
                    var existingDocuments = _dbContext.DocumentMaster
                        .Where(d => d.DocumentId == data.Id.ToString() && d.DocumentType == "TaskDocuments")
                        .ToList();

                    if (existingDocuments.Any())
                    {
                        foreach (var doc in existingDocuments)
                        {
                            // Delete physical file
                            if (!string.IsNullOrWhiteSpace(doc.DocumentPath) && System.IO.File.Exists(doc.DocumentPath))
                            {
                                try
                                {
                                    System.IO.File.Delete(doc.DocumentPath);
                                }
                                catch (Exception fileEx)
                                {
                                    Log.Error(data.UserId, $"Error deleting file {doc.DocumentPath}: {fileEx.Message}", "Task");
                                }
                            }
                        }

                        _dbContext.DocumentMaster.RemoveRange(existingDocuments);
                        await _dbContext.SaveChangesAsync();
                        documentsDeleted = existingDocuments.Count;
                    }

                    // Upload new documents
                    var documentDto = new Documentdto
                    {
                        Documents = data.Documents,
                        DocumentId = data.Id.ToString(),
                        documentType = "TaskDocuments",
                        FolderName = "TaskDocuments",
                        UserID = data.UserId
                    };

                    await _docservice.DocumentsUpload(documentDto);
                    documentsUploaded = data.Documents.Count;
                    changes.Add($"Documents updated: {documentsDeleted} removed, {documentsUploaded} added");
                }

                // Log updates
                if (changes.Any())
                {
                    var changesSummary = string.Join("; ", changes);
                    Log.DataLog($"{data.UserId}",
                        $"Task Id {existingTask.Id} ('{existingTask.TaskName}') updated by {data.UserId}. Changes: {changesSummary}",
                        "Task");

                    // Create UserActivityLog
                    var userActivityLog = new UserActivityLog
                    {
                        SNType = "Task Update",
                        SNTital = "Task Updated",
                        SNDescription = $"Task '{existingTask.TaskName}' (Id: {existingTask.Id}) for project '{existingTask.ProjectName}' was successfully updated by manager '{managerName}'. Changes: {changesSummary}",
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
                    ? $"Task updated successfully. {changes.Count} change(s) applied."
                    : "No changes detected. Task remains unchanged.";

                return Ok(new
                {
                    success = true,
                    message = responseMessage,
                    //data = new
                    //{
                    //    TaskId = existingTask.Id,
                    //    TaskName = existingTask.TaskName,
                    //    ProjectName = existingTask.ProjectName,
                    //    EmployeeUserId = existingTask.EmployeeUserId,
                    //    Status = existingTask.Status,
                    //    ChangesApplied = changes.Count,
                    //    DocumentsDeleted = documentsDeleted,
                    //    DocumentsUploaded = documentsUploaded
                    //}
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error($"{data?.UserId ?? "Unknown"}",
                    $"Error updating task Id {data?.Id}: {ex.Message}",
                    "Task");
                return Ok(new { success = false, message = $"An error occurred while updating task: {ex.Message}" });
            }
        }

        [HttpPost("DeleteTaskById")]
        public async Task<IActionResult> DeleteTaskById(int taskId, string? userId)
        {
            try
            {
                if (taskId <= 0)
                {
                    return Ok(new { success = false, message = "Invalid Task Id." });
                }

                if (string.IsNullOrEmpty(userId))
                {
                    return Ok(new { success = false, message = "UserId is required." });
                }

                var userExists = await _dbContext.Users.AnyAsync(u => u.UserID.ToString().ToLower() == userId.ToLower());
                if (!userExists)
                {
                    return Ok(new { success = false, message = "UserId not found." });
                }

                var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                {
                    return Ok(new { success = false, message = "Task not found." });
                }

                var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Delete related documents
                    var documentList = await _dbContext.DocumentMaster
                        .Where(d => d.InitiationId == taskId && d.DocumentType == "TaskDocuments")
                        .ToListAsync();

                    if (documentList.Any())
                        _dbContext.DocumentMaster.RemoveRange(documentList);

                    // Delete the task
                    _dbContext.Tasks.Remove(task);

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Log.DataLog(userId, $"Task Id {taskId} ('{task.TaskName}') and its documents deleted successfully by {userId}", "Tasks");

                    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserID.ToString().ToLower() == userId.ToLower());
                    var userName = user?.FullName ?? "Unknown User";

                    var userActivityLog = new UserActivityLog
                    {
                        SNType = "Task Deletion",
                        SNTital = "Task Deleted",
                        SNDescription = $"Task '{task.TaskName}' (Id: {taskId}) and its related documents were successfully deleted by user '{userName}'.",
                        SNActionUserId = userId,
                        CreatedOn = DateTime.Now,
                        IsActive = true,
                        IsRead = false
                    };
                    _dbContext.UserActivityLog.Add(userActivityLog);
                    await _dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Task deleted successfully." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { success = false, message = $"Error deleting Task: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }


        #endregion

        #region Task Information related API's

        [HttpGet("GetAllTaskInfoDetailsByProjectId")]
        public async Task<IActionResult> GetAllTaskInfoDetailsByProjectId(int projectId)
        {
            try
            {
                // Validation
                if (projectId <= 0)
                {
                    return Ok(new { success = false, message = "ProjectId is required." });
                }

                // Check if project exists and is active
                var projectExists = await _dbContext.Project
                    .AnyAsync(p => p.Id == projectId && p.IsActive == true);

                if (!projectExists)
                {
                    return Ok(new { success = false, message = "Project not found." });
                }

                // Fetch task-related info with documents
                var TaskInfoDetails = await (from ti in _dbContext.TaskRelatedInfo
                                             where ti.ProjectId == projectId && ti.IsActive == true
                                             orderby ti.CreatedOn descending
                                             select new
                                             {
                                                 ti.Id,
                                                 ti.TaskId,
                                                 ti.ProjectId,
                                                 ti.Comments,
                                                 ti.IsCompleted,
                                                 ti.IsActive,
                                                 ti.CreatedOn,
                                                 ti.CreatedBy,
                                                 ti.ModifiedOn,
                                                 ti.ModifiedBy,
                                                 Documents = (from doc in _dbContext.DocumentMaster
                                                              where doc.DocumentId == ti.Id.ToString() &&
                                                                    doc.DocumentType == "TaskRelatedInfoDocuments"
                                                              select new
                                                              {
                                                                  doc.Id,
                                                                  doc.DocumentId,
                                                                  doc.DocumentType,
                                                                  doc.DocumentPath,
                                                                  doc.DocumentURL,
                                                                  doc.CreatedBy,
                                                                  doc.CreatedOn
                                                              }).ToList()
                                             }).ToListAsync();

                if (TaskInfoDetails == null || TaskInfoDetails.Count == 0)
                {
                    return Ok(new { success = false, message = "No task info found for this project.", data = TaskInfoDetails });
                }

                return Ok(new { success = true, message = "Task info retrieved successfully.", data = TaskInfoDetails });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"An error occurred while retrieving task info: {ex.Message}" });
            }
        }


        [HttpPost("CreateTaskRelatedInfo")]
        public async Task<IActionResult> CreateTaskRelatedInfo([FromForm] TaskRelatedInfoDto data)
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Required field validations
                if (string.IsNullOrWhiteSpace(data.UserId))
                    return Ok(new { success = false, message = "UserId is required." });

                if (data.TaskId == null || data.TaskId <= 0)
                    return Ok(new { success = false, message = "TaskId is required." });

                if (data.ProjectId == null || data.ProjectId <= 0)
                    return Ok(new { success = false, message = "ProjectId is required." });

                if (string.IsNullOrWhiteSpace(data.Comments))
                    return Ok(new { success = false, message = "Comments are required." });

                // Validate TaskId exists (optional - add if you have Task table)
                // var taskExists = await _dbContext.Tasks.AnyAsync(t => t.Id == data.TaskId);
                // if (!taskExists)
                //     return Ok(new { success = false, message = "Task not found. Please provide a valid TaskId." });

                // Validate ProjectId exists
                var projectExists = await _dbContext.Project.AnyAsync(p => p.Id == data.ProjectId);
                if (!projectExists)
                {
                    return Ok(new { success = false, message = "Project not found. Please provide a valid ProjectId." });
                }

                // Create new TaskRelatedInfo
                var newTaskRelatedInfo = new TaskRelatedInfo
                {
                    TaskId = data.TaskId,
                    ProjectId = data.ProjectId,
                    Comments = data.Comments,
                    IsCompleted = data.IsCompleted ?? false,
                    IsActive = data.IsActive ?? true,
                    CreatedBy = data.UserId,
                    CreatedOn = DateTime.Now
                };

                await _dbContext.TaskRelatedInfo.AddAsync(newTaskRelatedInfo);
                await _dbContext.SaveChangesAsync();
                int Id = newTaskRelatedInfo.Id;

                // Handle document uploads
                int documentCount = 0;
                if (data.Documents != null && data.Documents.Any())
                {
                    var documentDto = new Documentdto
                    {
                        Documents = data.Documents,
                        DocumentId = Id.ToString(),
                        documentType = "TaskRelatedInfoDocuments",
                        FolderName = "TaskRelatedInfoDocuments",
                        UserID = data.UserId
                    };

                    await _docservice.DocumentsUpload(documentDto);
                    documentCount = data.Documents.Count;
                }

                // Log creation
                Log.DataLog($"{data.UserId}",
                    $"TaskRelatedInfo Id {newTaskRelatedInfo.Id} created for TaskId {data.TaskId} and ProjectId {data.ProjectId} by {data.UserId}. {documentCount} document(s) uploaded.",
                    "TaskRelatedInfo");

                // Get user details for activity log
                var user = await _dbContext.Users.FirstOrDefaultAsync(u =>
                    u.UserID.ToString().ToLower() == data.UserId.ToString().ToLower());
                var createdByName = user?.FullName ?? "Unknown User";

                // Create UserActivityLog
                var userActivityLog = new UserActivityLog
                {
                    SNType = "Task Info Creation",
                    SNTital = "Task Related Info Created",
                    SNDescription = $"Task related information (Id: {Id}) was successfully created for TaskId {data.TaskId} and ProjectId {data.ProjectId} by user '{createdByName}' with {documentCount} document(s).",
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
                    message = $"Task related information created successfully with {documentCount} document(s).",
                    data = new
                    {
                        Id = newTaskRelatedInfo.Id,
                        TaskId = newTaskRelatedInfo.TaskId,
                        ProjectId = newTaskRelatedInfo.ProjectId,
                        DocumentsUploaded = documentCount
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error($"{data?.UserId ?? "Unknown"}",
                    $"Error creating TaskRelatedInfo: {ex.Message}",
                    "TaskRelatedInfo");
                return Ok(new { success = false, message = $"An error occurred while creating task related information: {ex.Message}" });
            }
        }

        [HttpPost("UpdateTaskRelatedInfo")]
        public async Task<IActionResult> UpdateTaskRelatedInfo([FromForm] TaskRelatedInfoDto data)
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Required field validations
                if (data.Id <= 0)
                    return Ok(new { success = false, message = "TaskRelatedInfo Id is required." });

                if (string.IsNullOrWhiteSpace(data.UserId))
                    return Ok(new { success = false, message = "UserId is required." });

                // Check if TaskRelatedInfo exists
                var existingTaskRelatedInfo = await _dbContext.TaskRelatedInfo.FirstOrDefaultAsync(t => t.Id == data.Id);
                if (existingTaskRelatedInfo == null)
                {
                    return Ok(new { success = false, message = "Task related information not found. Please provide a valid Id." });
                }

                // Validate ProjectId if provided
                if (data.ProjectId != null && data.ProjectId > 0)
                {
                    var projectExists = await _dbContext.Project.AnyAsync(p => p.Id == data.ProjectId);
                    if (!projectExists)
                    {
                        return Ok(new { success = false, message = "Project not found. Please provide a valid ProjectId." });
                    }
                }

                // Track changes for logging
                var changes = new List<string>();

                // Update only the fields that are provided/changed
                if (data.TaskId != null && data.TaskId > 0 && data.TaskId != existingTaskRelatedInfo.TaskId)
                {
                    changes.Add($"TaskId changed from '{existingTaskRelatedInfo.TaskId}' to '{data.TaskId}'");
                    existingTaskRelatedInfo.TaskId = data.TaskId;
                }

                if (data.ProjectId != null && data.ProjectId > 0 && data.ProjectId != existingTaskRelatedInfo.ProjectId)
                {
                    changes.Add($"ProjectId changed from '{existingTaskRelatedInfo.ProjectId}' to '{data.ProjectId}'");
                    existingTaskRelatedInfo.ProjectId = data.ProjectId;
                }

                if (!string.IsNullOrWhiteSpace(data.Comments) && data.Comments != existingTaskRelatedInfo.Comments)
                {
                    changes.Add($"Comments updated");
                    existingTaskRelatedInfo.Comments = data.Comments;
                }

                if (data.IsCompleted != null && data.IsCompleted != existingTaskRelatedInfo.IsCompleted)
                {
                    changes.Add($"IsCompleted changed from '{existingTaskRelatedInfo.IsCompleted}' to '{data.IsCompleted}'");
                    existingTaskRelatedInfo.IsCompleted = data.IsCompleted;
                }

                if (data.IsActive != null && data.IsActive != existingTaskRelatedInfo.IsActive)
                {
                    changes.Add($"IsActive changed from '{existingTaskRelatedInfo.IsActive}' to '{data.IsActive}'");
                    existingTaskRelatedInfo.IsActive = data.IsActive;
                }

                // Set ModifiedBy and ModifiedOn
                existingTaskRelatedInfo.ModifiedBy = data.UserId;
                existingTaskRelatedInfo.ModifiedOn = DateTime.Now;

                _dbContext.TaskRelatedInfo.Update(existingTaskRelatedInfo);
                await _dbContext.SaveChangesAsync();

                // Handle new document uploads
                int newDocumentCount = 0;
                if (data.Documents != null && data.Documents.Any())
                {
                    var documentDto = new Documentdto
                    {
                        Documents = data.Documents,
                        DocumentId = data.Id.ToString(),
                        documentType = "TaskRelatedInfoDocuments",
                        FolderName = "TaskRelatedInfoDocuments",
                        UserID = data.UserId
                    };

                    await _docservice.DocumentsUpload(documentDto);
                    newDocumentCount = data.Documents.Count;
                    changes.Add($"{newDocumentCount} new document(s) uploaded");
                }

                // Get user details for logging
                var user = await _dbContext.Users.FirstOrDefaultAsync(u =>
                    u.UserID.ToString().ToLower() == data.UserId.ToString().ToLower());
                var modifiedByName = user?.FullName ?? "Unknown User";

                // Log updates if changes were made
                if (changes.Any())
                {
                    var changesSummary = string.Join("; ", changes);
                    Log.DataLog($"{data.UserId}",
                        $"TaskRelatedInfo Id {existingTaskRelatedInfo.Id} updated by {data.UserId}. Changes: {changesSummary}",
                        "TaskRelatedInfo");

                    // Create UserActivityLog
                    var userActivityLog = new UserActivityLog
                    {
                        SNType = "Task Info Update",
                        SNTital = "Task Related Info Updated",
                        SNDescription = $"Task related information (Id: {existingTaskRelatedInfo.Id}) was successfully updated by user '{modifiedByName}'. Changes: {changesSummary}",
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
                    ? $"Task related information updated successfully. {changes.Count} change(s) applied."
                    : "No changes detected. Task related information remains unchanged.";

                return Ok(new
                {
                    success = true,
                    message = responseMessage,
                    data = new
                    {
                        Id = existingTaskRelatedInfo.Id,
                        ChangesApplied = changes.Count,
                        NewDocumentsAdded = newDocumentCount
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error($"{data?.UserId ?? "Unknown"}",
                    $"Error updating TaskRelatedInfo Id {data?.Id}: {ex.Message}",
                    "TaskRelatedInfo");
                return Ok(new { success = false, message = $"An error occurred while updating task related information: {ex.Message}" });
            }
        }

        [HttpPost("DeleteTaskRelatedInfoDataById")]
        public async Task<IActionResult> DeleteTaskRelatedInfoDataById(int Id, string? userId)
        {
            try
            {
                if (Id <= 0)
                {
                    return Ok(new { success = false, message = "Invalid TaskRelatedInfo Id." });
                }

                if (string.IsNullOrEmpty(userId))
                {
                    return Ok(new { success = false, message = "UserId is required." });
                }

                var userExists = await _dbContext.Users.AnyAsync(u => u.UserID.ToString().ToLower() == userId.ToLower());
                if (!userExists)
                {
                    return Ok(new { success = false, message = "UserId not found." });
                }

                var taskInfo = await _dbContext.TaskRelatedInfo.FirstOrDefaultAsync(t => t.Id == Id);

                if (taskInfo == null)
                    return Ok(new { success = false, message = "TaskRelatedInfo not found." });

                var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var documentList = await _dbContext.DocumentMaster.Where(d => d.DocumentId == Id.ToString() && d.DocumentType == "TaskRelatedInfoDocuments").ToListAsync();

                    if (documentList.Any())
                        _dbContext.DocumentMaster.RemoveRange(documentList);

                    _dbContext.TaskRelatedInfo.Remove(taskInfo);

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Log.DataLog(userId, $"TaskRelatedInfo Id {Id} and its documents deleted successfully by {userId}", "TaskRelatedInfo");

                    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserID.ToString().ToLower() == userId.ToLower());
                    var userName = user?.FullName ?? "Unknown User";
                    var userActivityLog = new UserActivityLog
                    {
                        SNType = "TaskRelatedInfo Deletion",
                        SNTital = "TaskRelatedInfo Deleted",
                        SNDescription = $"TaskRelatedInfo Id '{Id}' and its related documents were successfully deleted by user '{userName}'.",
                        SNActionUserId = userId,
                        CreatedOn = DateTime.Now,
                        IsActive = true,
                        IsRead = false
                    };
                    _dbContext.UserActivityLog.Add(userActivityLog);
                    await _dbContext.SaveChangesAsync();
                    return Ok(new { success = true, message = "TaskRelatedInfo deleted successfully." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { success = false, message = $"Error deleting TaskRelatedInfo: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        #endregion
    }
}
