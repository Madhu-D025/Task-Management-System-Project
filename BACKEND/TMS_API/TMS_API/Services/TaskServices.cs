
using TMS_API.DBContext;

namespace TMS_API.Services
{
    public interface ITaskServices
    {
        //Task<List<TaskDto>> GetAllTasks();
        //Task<TaskDto> GetTaskById(int id);
        //Task<TaskDto> CreateTask(TaskDto data);
        //Task<bool> DeleteTaskById(int Id, string UserId);
    }
    public class TaskServices : ITaskServices
    {
        private readonly AppDbContext _dbContext;

        public TaskServices(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

    }
}
