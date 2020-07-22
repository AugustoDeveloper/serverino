using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PortAreaApp.Controllers.V1
{
    [Route("[controller]"), ApiController]
    public class ApplicationController : ControllerBase
    {
        [HttpGet("all")]
        public async Task<IActionResult> GetAllApplicationsAsync()
        {
            return await Task.FromResult(Ok(new[] {"teste"}));
        }
        
    }
}