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
        private readonly ITaskServices _taskServices;
        private readonly AppDbContext _dbContext;
        private readonly DocumentService _docservice;

        public TaskController(ITaskServices taskServices, AppDbContext dbContext, DocumentService docservice)
        {
            _taskServices = taskServices;
            _dbContext = dbContext;
            _docservice = docservice;
        }



        #region Task Information related API's

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
                int taskRelatedInfoId = newTaskRelatedInfo.Id;

                // Handle document uploads
                int documentCount = 0;
                if (data.Documents != null && data.Documents.Any())
                {
                    var documentDto = new Documentdto
                    {
                        Documents = data.Documents,
                        DocumentId = taskRelatedInfoId.ToString(),
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
                    SNDescription = $"Task related information (Id: {taskRelatedInfoId}) was successfully created for TaskId {data.TaskId} and ProjectId {data.ProjectId} by user '{createdByName}' with {documentCount} document(s).",
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
                        TaskRelatedInfoId = newTaskRelatedInfo.Id,
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
                        TaskRelatedInfoId = existingTaskRelatedInfo.Id,
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


        [HttpPost("DeleteTaskRelatedInfoById")]
        public async Task<IActionResult> DeleteTaskRelatedInfoById([FromBody] DeleteRequestDto data)
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if (data.Id <= 0)
                    return Ok(new { success = false, message = "Valid TaskRelatedInfo Id is required." });

                if (string.IsNullOrWhiteSpace(data.UserId))
                    return Ok(new { success = false, message = "UserId is required." });

                // Check if TaskRelatedInfo exists
                var taskRelatedInfo = await _dbContext.TaskRelatedInfo
                    .FirstOrDefaultAsync(t => t.Id == data.Id);

                if (taskRelatedInfo == null)
                {
                    return Ok(new { success = false, message = "Task related information not found." });
                }

                if (taskRelatedInfo.IsActive == false)
                {
                    return Ok(new { success = false, message = "Task related information is already deleted." });
                }

                // Get documents associated with this TaskRelatedInfo
                var documents = await _dbContext.DocumentMaster
                    .Where(d => d.DocumentId == data.Id.ToString() &&
                               d.DocumentType == "TaskRelatedInfoDocuments")
                    .ToListAsync();

                int documentCount = documents.Count;

                // Soft delete documents (mark as inactive)
                if (documents.Any())
                {
                    foreach (var doc in documents)
                    {
                        doc.ModifiedBy = data.UserId;
                        doc.ModifiedOn = DateTime.Now;
                    }
                    _dbContext.DocumentMaster.UpdateRange(documents);
                }

                // Soft delete TaskRelatedInfo (mark as inactive)
                taskRelatedInfo.IsActive = false;
                taskRelatedInfo.ModifiedBy = data.UserId;
                taskRelatedInfo.ModifiedOn = DateTime.Now;

                _dbContext.TaskRelatedInfo.Update(taskRelatedInfo);
                await _dbContext.SaveChangesAsync();

                // Log deletion
                Log.DataLog($"{data.UserId}",
                    $"TaskRelatedInfo Id {taskRelatedInfo.Id} (TaskId: {taskRelatedInfo.TaskId}, ProjectId: {taskRelatedInfo.ProjectId}) deleted by {data.UserId}. {documentCount} document(s) also deleted.",
                    "TaskRelatedInfo");

                // Get user details for activity log
                var user = await _dbContext.Users.FirstOrDefaultAsync(u =>
                    u.UserID.ToString().ToLower() == data.UserId.ToString().ToLower());
                var deletedByName = user?.FullName ?? "Unknown User";

                // Create UserActivityLog
                var userActivityLog = new UserActivityLog
                {
                    SNType = "Task Info Deletion",
                    SNTital = "Task Related Info Deleted",
                    SNDescription = $"Task related information (Id: {taskRelatedInfo.Id}) for TaskId {taskRelatedInfo.TaskId} and ProjectId {taskRelatedInfo.ProjectId} was successfully deleted by user '{deletedByName}'. {documentCount} associated document(s) also deleted.",
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
                    message = $"Task related information deleted successfully. {documentCount} associated document(s) also deleted.",
                    data = new
                    {
                        TaskRelatedInfoId = taskRelatedInfo.Id,
                        TaskId = taskRelatedInfo.TaskId,
                        ProjectId = taskRelatedInfo.ProjectId,
                        DocumentsDeleted = documentCount
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error($"{data?.UserId ?? "Unknown"}",
                    $"Error deleting TaskRelatedInfo Id {data?.Id}: {ex.Message}",
                    "TaskRelatedInfo");
                return Ok(new { success = false, message = $"An error occurred while deleting task related information: {ex.Message}" });
            }
        }

        // Helper DTO class for Delete operation
        public class DeleteRequestDto
        {
            public int Id { get; set; }
            public string? UserId { get; set; }
        }
        #endregion
    }
}
