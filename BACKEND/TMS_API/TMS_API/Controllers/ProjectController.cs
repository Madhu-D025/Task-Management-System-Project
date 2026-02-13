using Microsoft.AspNetCore.Mvc;
using System.Data.SqlTypes;
using TMS_API.DBContext;
using TMS_API.Services;

namespace TMS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : Controller
    {
        private readonly IProjectServices _projectServices;
        private readonly AppDbContext _dbContext;

        public ProjectController(IProjectServices projectServices, AppDbContext dbContext)
        {
            _projectServices = projectServices;
            _dbContext = dbContext;
        }
    }
}
