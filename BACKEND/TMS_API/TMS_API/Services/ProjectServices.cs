using DMSAPI.Services;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.EntityFrameworkCore;
using TMS_API.DBContext;
using TMS_API.Models;

namespace TMS_API.Services
{
    public interface IProjectServices
    {
        Task<List<Project>> GetAllProjects();
        Task<Project> GetProjectById(int id);
        //Task<ProjectDto> CreateProject(ProjectDto data);
        //Task<bool> DeleteProjectById(int Id, string UserId);

    }
    public class ProjectServices : IProjectServices
    {
        private readonly AppDbContext _dbContext;

        public ProjectServices(AppDbContext appDbContext)
        {
            _dbContext = appDbContext;
        }


        #region Project Related Logic

        public async Task<List<Project>> GetAllProjects()
        {
            try
            {
                var data = await _dbContext.Project.Where(x => x.IsActive == true).ToListAsync();
                if(data.Count == 0)
                {
                    throw new Exception("Project Details Not Found");
                }
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message ?? "Network error");
            }
        }

        public async Task<Project> GetProjectById(int Id)
        {
            try
            {
                var data = await _dbContext.Project.FirstOrDefaultAsync(x => x.Id == Id && x.IsActive == true);
                if (data == null)
                {
                    throw new Exception("Project Details Not Found");
                }
                return data;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message ?? "Network error");
            }

        }

        public async Task<Project> CreateOrUpdateProject(ProjectDto data)
        {
            var now = DateTime.UtcNow;
            try
            {
                var existingProject = await _dbContext.Project.FirstOrDefaultAsync(x => x.Id == data.Id);

                // Fetch user once
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserID.ToString().ToLower() == data.UserId.ToLower());
                var createdByName = user?.FullName ?? "Unknown User";

                // Check duplicates
                var duplicateProject = await _dbContext.Project.AsNoTracking()
                                                                .AnyAsync(x =>
                                                                    x.ProjectName.ToUpper() == data.ProjectName.ToUpper() &&
                                                                    x.ProjectType.ToUpper() == data.ProjectType.ToUpper() &&
                                                                    x.Id != data.Id);

                if (duplicateProject)
                {
                    throw new Exception("A Project with same Project Name and Project Type already exists.");
                }


                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                if (existingProject != null)
                {
                    // Update existing Project
                    var updatedFields = new List<string>();

                    if (existingProject.ProjectName != data.ProjectName)
                    {
                        updatedFields.Add($"ProjectName: {existingProject.ProjectName} -> {data.ProjectName}");
                        existingProject.ProjectName = data.ProjectName;
                    }
                    if (existingProject.ProjectType != data.ProjectType)
                    {
                        updatedFields.Add($"ProjectType: {existingProject.ProjectType} -> {data.ProjectType}");
                        existingProject.ProjectType = data.ProjectType;
                    }
                    if (existingProject.StartDate != data.StartDate)
                    {
                        updatedFields.Add($"StartDate: {existingProject.StartDate} -> {data.StartDate}");
                        existingProject.StartDate = data.StartDate;
                    }
                    if (existingProject.EndDate != data.EndDate)
                    {
                        updatedFields.Add($"EndDate: {existingProject.EndDate} -> {data.EndDate}");
                        existingProject.EndDate = data.EndDate;
                    }
                    if (!string.Equals(existingProject.ManagerId, data.ManagerId, StringComparison.OrdinalIgnoreCase))
                    {
                        updatedFields.Add($"ManagerId: {existingProject.ManagerId} -> {data.ManagerId}");
                        existingProject.ManagerId = data.ManagerId;
                    }
                    if (!string.Equals(existingProject.ManagerName, data.ManagerName, StringComparison.OrdinalIgnoreCase))
                    {
                        updatedFields.Add($"ManagerName: {existingProject.ManagerName} -> {data.ManagerName}");
                        existingProject.ManagerName = data.ManagerName;
                    }
                    if (!string.Equals(existingProject.Status, data.Status, StringComparison.OrdinalIgnoreCase))
                    {
                        updatedFields.Add($"Status: {existingProject.Status} -> {data.Status}");
                        existingProject.Status = data.Status;
                    }
                    if (existingProject.IsActive != data.IsActive)
                    {
                        updatedFields.Add($"IsActive: " +
                        $"{existingProject.IsActive} -> {data.IsActive}");
                        existingProject.IsActive = data.IsActive;
                    }
                    if (existingProject.IsCompleted != data.IsCompleted)
                    {
                        updatedFields.Add($"IsCompleted: " +
                        $"{existingProject.IsCompleted} -> {data.IsCompleted}");
                        existingProject.IsCompleted = data.IsCompleted;
                    }

                    existingProject.ModifiedBy = data.UserId;
                    existingProject.ModifiedOn = now;
                    _dbContext.Project.Update(existingProject);

                    Log.DataLog($"{data.UserId}", $"Project {existingProject.Id} updated fields: {string.Join(", ", updatedFields)}", "Project");

                    //await AddActivityLog(data, "Project Updated", $"Project '{data.ManagerId}' updated successfully.");

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return existingProject;
                }
                else
                {
                    // Create new Project
                    var newProject = new Project
                    {
                        ProjectName = data.ProjectName,
                        ProjectType = data.ProjectType,
                        ManagerId = data.ManagerId,
                        StartDate = data.StartDate,
                        EndDate = data.EndDate,
                        ManagerName = data.ManagerName,
                        IsActive = data.IsActive,
                        IsCompleted = data.IsCompleted,
                        Status = data.Status,
                        CreatedBy = data.UserId,
                        CreatedOn = now
                    };

                    await _dbContext.Project.AddAsync(newProject);
                    Log.DataLog($"{data.UserId}", $"Project {data.ProjectName} with Type {data.ProjectType} was created successfully by {data.UserId}.", "Project");

                    //await AddActivityLog(data, "Project Created", $"Holiday '{data.ManagerId}' created successfully.");
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return newProject;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message ?? "Network error");
            }
        }

        //public async Task AddActivityLog(ProjectDto data, string type, string description)
        //{

        //    var userActivityLog = new UserActivityLog
        //    {
        //        SNType = type,
        //        SNTital = type,
        //        SNDescription = description,
        //        SNActionUserId = data.UserId,
        //        CreatedOn = DateTime.UtcNow,
        //        IsActive = true,
        //        IsRead = false
        //    };

        //    await _dbContext.UserActivityLog.AddAsync(userActivityLog);
        //}


        #endregion
    }
}
