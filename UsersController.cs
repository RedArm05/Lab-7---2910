using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using CsvHelper;
using Lab5_Elijah_Mckeehan.Shared;
using Microsoft.AspNetCore.Hosting;

namespace Lab5_Elijah_Mckeehan.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UsersController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveUsersToCsv([FromBody] List<User> users)
        {
            try
            {
                var dataPath = Path.Combine(_env.ContentRootPath, "Data");
                Directory.CreateDirectory(dataPath); // Ensure folder exists

                var csvPath = Path.Combine(dataPath, "users.csv");

                using var writer = new StreamWriter(csvPath);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

                csv.WriteHeader<User>();
                await csv.NextRecordAsync();
                await csv.WriteRecordsAsync(users);

                return Ok(new { message = "Users saved to CSV." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to save users: {ex.Message}");
            }
        }
    }
}
