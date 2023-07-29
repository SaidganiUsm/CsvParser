using CsvParser.Data;
using Microsoft.AspNetCore.Mvc;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using CsvParser.Models;

namespace CsvParser.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public UserController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            // Validation of uploaded file
            if (file == null || file.Length <= 0)
            {
                return BadRequest("File is empty");
            }

            using (var streamReader = new StreamReader(file.OpenReadStream()))
            using (var csvReader = new CsvReader(streamReader, 
                new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var records = csvReader.GetRecords<User>().ToList();

                foreach (var record in records)
                {
                    var existingUser = _dbContext.Users.Find(record.UserIdentifier);

                    if(existingUser != null)
                    {
                        _dbContext.Users.Add(record);
                    } else
                    {
                        _dbContext.Entry(existingUser).CurrentValues.SetValues(record);
                    }
                }

                _dbContext.SaveChanges();
            }

            return Ok("Data from uploaded CSV file has been sucessfully added to database");
        }

        [HttpGet]
        public IActionResult GetUsers(string sortField = "Username", 
            bool sortAscending = true, int? limit = null) 
        {
            IQueryable<User> query = _dbContext.Users;

            // Sorting
            if (sortField == "Username")
            {
                query = sortAscending ? query.OrderBy(u => u.UserName) : 
                    query.OrderByDescending(u => u.UserName);
            }

            // Limitations
            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }
            
            var users = query.ToList();
            return Ok(users);
        }
    }
}
