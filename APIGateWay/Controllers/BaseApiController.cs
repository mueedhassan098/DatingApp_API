using APIGateWay.Data;
using APIGateWay.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace APIGateWay.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [ApiController]
    [Route("api/[Controller]")]
    public class BaseApiController : ControllerBase
    {
    }
}
