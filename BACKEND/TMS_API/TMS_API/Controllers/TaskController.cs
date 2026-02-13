using Microsoft.AspNetCore.Mvc;
using TMS_API.DBContext;
using TMS_API.Services;

namespace TMS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : Controller
    {
        private readonly ITaskServices _taskServices;
        private readonly AppDbContext _dbContext;

        public TaskController(ITaskServices taskServices, AppDbContext dbContext)
        {
            _taskServices = taskServices;
            _dbContext = dbContext;
        }
    }
}
