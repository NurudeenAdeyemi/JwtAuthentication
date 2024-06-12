using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using productTestApi.Authentication;

namespace productTestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginModel model)
        {
            if(model.UserName == "test" && model.Password == "password")
            {
                var token = TokenService.GenerateToken(model.UserName);
                return Ok(new {Token = token});
            }
            return Unauthorized();
        }
    }

    public class UserLoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}