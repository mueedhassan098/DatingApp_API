using APIGateWay.Data;
using APIGateWay.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIGateWay.Controllers
{
    public class BuggyController:BaseApiController
    {
        private readonly DataContextClass _dataContext;

        public BuggyController(DataContextClass dataContext)
        {
            _dataContext = dataContext;
        }
        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return "secret text";
        }
        [HttpGet("not-found")]
        public ActionResult<App_User> GetNotFound()
        {
            var thing = _dataContext.Users.Find(-1);
            if (thing == null)
            {
                return NotFound();
            }
            return thing;

        }
        [HttpGet("server-error")]
        public ActionResult<string> GetServerError()
        {
            var thing = _dataContext.Users.Find(-1);

            var thingToReturn = thing.ToString();

            return thingToReturn;
        }
        //public ActionResult<string> GetServerError()
        //{
        //    var thing = _dataContext.Users.Find(-1);

        //    if (thing == null)
        //    {
        //        return StatusCode(500, "Internal Server Error: User not found");
        //    }

        //    var thingToReturn = thing.ToString();
        //    return thingToReturn;
        //}
        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest()
        {
           return BadRequest("it was not a good request");
        }
      
    }
}
