using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SampleApp.Controllers
{
    [Route("api/[controller]"), ApiController]
    public class BooksController : ControllerBase
    {
        [HttpGet("all")]
        async public Task<IActionResult> GetAllBooks([FromServices]IConfiguration configuration)
            => await Task.FromResult(Ok(configuration.GetValue<string>("booksResult", "Didn't came from configuration file")));
    }
}