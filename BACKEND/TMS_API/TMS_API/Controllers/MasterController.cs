using DMSAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS_API.DBContext;
using TMS_API.Models;

namespace SOW.Controllers
{
    [ApiController]
    [Route("api/MasterController")]
    public class MasterController : Controller
    {
        private readonly AppDbContext _dbContext;

        public MasterController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Masters Related API's

        [HttpPost("CreateMaster")]
        public async Task<IActionResult> CreateMaster([FromBody] MasterDto masterDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new { Success = false, Message = "Invalid data", Errors = ModelState });
                }

                if (string.IsNullOrEmpty(masterDto.MasterName) || (masterDto.MasterName) == "string")
                {
                    return Ok(new { Success = false, Message = "MasterName is required" });
                }

                if (!string.IsNullOrEmpty(masterDto.MasterValue))
                {
                    var existingMaster = await _dbContext.Masters
                        .AnyAsync(m => m.MasterValue.ToLower() == masterDto.MasterValue.ToLower() && m.MasterName.ToLower() == masterDto.MasterName.ToLower());
                    if (existingMaster)
                    {
                        return Ok(new { Success = false, Message = "MasterValue already exists" });
                    }
                }

                var master = new Masters
                {
                    MasterName = masterDto.MasterName,
                    MasterValue = masterDto.MasterValue,
                    IsActive = masterDto.IsActive ?? true,
                    CreatedBy = masterDto.CreatedBy,
                    CreatedOn = DateTime.Now
                };

                _dbContext.Masters.Add(master);
                await _dbContext.SaveChangesAsync();
                Log.DataLog(master.Id.ToString(), $"Master created successfully with Name: '{master.MasterName}' and Value: '{master.MasterValue}' by '{master.CreatedBy}'", "Master Log");
                return Ok(new { Success = true, Message = "Master created successfully", Data = master.Id });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("UpdateMaster")]
        public async Task<IActionResult> UpdateMaster([FromBody] MasterDto masterDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new { Success = false, Message = "Invalid data", Errors = ModelState });
                }
                if (masterDto.Id <= 0)
                {
                    return Ok(new { Success = false, Message = "Valid ID is required" });
                }
                if (string.IsNullOrEmpty(masterDto.MasterName) || (masterDto.MasterName) == "string")
                {
                    return Ok(new { Success = false, Message = "MasterName is required" });
                }
                var allmasters = await _dbContext.Masters.ToListAsync();
                var existingMaster = allmasters
                    .FirstOrDefault(m => m.Id == masterDto.Id && m.IsActive == true);
                var existvalue = allmasters.Where(x => x.MasterName.ToLower() == existingMaster.MasterName.ToLower() && x.Id != masterDto.Id && x.MasterValue.ToLower() == masterDto.MasterValue.ToLower()).FirstOrDefault();
                if (existvalue != null)
                {
                    return Ok(new { Success = false, Message = "Master value already exist" });
                }

                if (existingMaster == null)
                {
                    return NotFound(new { Success = false, Message = "Master not found or inactive" });
                }

                existingMaster.MasterName = masterDto.MasterName;
                existingMaster.MasterValue = masterDto.MasterValue;
                existingMaster.IsActive = masterDto.IsActive ?? true;
                existingMaster.ModifiedBy = masterDto.ModifiedBy;
                existingMaster.ModifiedOn = DateTime.Now;

                _dbContext.Masters.Update(existingMaster);
                await _dbContext.SaveChangesAsync();
                Log.DataLog(masterDto.Id.ToString(), $"Master updated successfully with Name: '{masterDto.MasterName}' and Value: '{masterDto.MasterValue}' by '{masterDto.ModifiedBy}'", "Master Log");
                return Ok(new { Success = true, Message = "Master updated successfully" });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("GetAllMasters")]
        public async Task<IActionResult> GetAllMasters()
        {
            try
            {
                var masters = await _dbContext.Masters
                    .Where(m => m.IsActive == true)
                    .OrderBy(m => m.MasterName)
                    .ThenBy(m => m.CreatedOn)
                    .Select(m => new MasterDto
                    {
                        Id = m.Id,
                        MasterName = m.MasterName,
                        MasterValue = m.MasterValue,
                        IsActive = m.IsActive,
                        CreatedBy = m.CreatedBy,
                        CreatedOn = m.CreatedOn,
                        ModifiedBy = m.ModifiedBy,
                        ModifiedOn = m.ModifiedOn
                    })
                    .ToListAsync();

                return Ok(new { Success = true, Data = masters, Message = "Masters retrieved successfully" });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("GetMasterById")]
        public async Task<IActionResult> GetMasterById(int id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return Ok(new { success = false, message = "Id is required" });
                }

                var MasterId = await _dbContext.Masters.AnyAsync(m => m.Id == id && m.IsActive == true);
                if (!MasterId)
                {
                    return Ok(new { success = false, message = "Id Not Found" });
                }

                var master = await _dbContext.Masters.ToListAsync();
                var result = (from m in master
                              where m.IsActive == true &&
                                    m.Id == id
                              select new
                              {
                                  m.Id,
                                  m.MasterName,
                                  m.MasterValue,
                                  m.IsActive,
                                  m.CreatedBy,
                                  m.CreatedOn,
                                  m.ModifiedBy,
                                  m.ModifiedOn

                              }).ToList();

                return Ok(new { success = true, message = "MasterDetails extracted successfully", data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("DeleteMasterById")]
        public async Task<IActionResult> DeleteMasterById(int id, string? UserId)
        {
            try
            {
                if (id <= 0)
                {
                    return Ok(new { Success = false, Message = "Valid ID is required" });
                }
                if (string.IsNullOrWhiteSpace(UserId))
                {
                    return Ok(new { success = false, message = "UserId is required" });
                }
                var User = await _dbContext.Users.AnyAsync(x => x.UserID.ToString().ToLower() == UserId.ToString() && x.IsActive == true);
                {
                    if (!User)
                    {
                        return Ok(new { success = false, message = "UserId Not Found" });
                    }
                }

                var existingMaster = await _dbContext.Masters
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (existingMaster == null)
                {
                    return NotFound(new { Success = false, Message = "Master not found" });
                }


                _dbContext.Masters.Remove(existingMaster);
                await _dbContext.SaveChangesAsync();
                Log.DataLog(existingMaster.Id.ToString(), $"Master deleted successfully with name: '{existingMaster.MasterName}', and value: '{existingMaster.MasterValue}', by '{UserId}'", "Master Log");
                return Ok(new { Success = true, Message = "Master deleted successfully" });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("GetAllMasterNames")]
        public async Task<IActionResult> GetAllMasterNames()
        {
            try
            {
                var masterNames = await _dbContext.Masters
                    .Where(m => m.IsActive == true)
                    .Select(m => m.MasterName)
                    .Distinct()
                    .Where(name => !string.IsNullOrEmpty(name))
                    .OrderBy(name => name)
                    .ToListAsync();

                return Ok(new
                {
                    Success = true,
                    Data = masterNames,
                    Message = "Master names retrieved successfully",
                });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("GetMasterDetailsByName")]
        public async Task<IActionResult> GetMasterDetailsByName(string MasterName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(MasterName))
                {
                    return Ok(new { success = false, message = "MasterName is required" });
                }
                var Mastername = await _dbContext.Masters.AnyAsync(m => m.MasterName.ToLower() == MasterName.ToLower() && m.IsActive == true);
                if (!Mastername)
                {
                    return Ok(new { success = false, message = "MasterDetails Not Found" });
                }

                var master = await _dbContext.Masters.ToListAsync();
                var result = (from m in master
                              where m.IsActive == true &&
                                    m.MasterName.ToLower() == MasterName.ToLower()
                              select new
                              {
                                  m.Id,
                                  m.MasterName,
                                  m.MasterValue,
                                  m.IsActive,
                                  m.CreatedBy,
                                  m.CreatedOn,
                                  m.ModifiedBy,
                                  m.ModifiedOn
                              }).ToList();

                return Ok(new { success = true, message = "MasterDetails extracted successfully", data = result });

            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Message = ex.Message });
            }
        }
        


        #endregion
    }
}

