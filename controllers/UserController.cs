using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace WebAppIdentityTest.controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Route("user/test")]
        [Authorize]
        [HttpGet]
        public IActionResult Test()
        {
            var c = HttpContext.User.Identity.Name;
            return Ok($"{c} be van jelentkezve");
        }

        [HttpPost]
        [Route("user/login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var c = HttpContext;

            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);

            if (result.Succeeded)
            {
                var appUser = _userManager.Users.SingleOrDefault(r => r.UserName == model.UserName);
                await _signInManager.SignInAsync(appUser, isPersistent: false);

                return Ok(true);
            }
            else
            {
                return BadRequest(false);
            }
        }

        [HttpPost]
        [Route("user/logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(true);
        }

        [Route("user/register")]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] RegisterUserRequest request)
        {
            var user = new IdentityUser();
            user.UserName = request.UserName;
            user.Email = request.Email;
            user.Id = Guid.NewGuid().ToString();
 
            //Check wether user exists
            var u = await _userManager.FindByNameAsync(request.UserName);
            if (u != null)
            {
                return Ok(new RegisterUserResponse() { Success = false, Field = "new_username", Detail = "User name already taken." });
            }

            //Validate username
            Regex r = new Regex(@"^[a-zA-Z0-9\-]+$");
            if (!r.IsMatch(request.UserName))
            {
                return Ok(new RegisterUserResponse() { Success = false, Field = "new_username", Detail = "Only characters A-Z, a-z, numbers and '-' are acceptable." });
            }

            //Trying to create user
            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                var newUser = await _userManager.FindByNameAsync(request.UserName);
                if (newUser == null)
                {
                    return Ok(new RegisterUserResponse() { Success = false, Field = "message", Detail = "Error creating new user" });
                }
                else
                {
                    return Ok(new RegisterUserResponse() { Success = true, Field = "message", Detail = "User successfully added." });
                }
            }
            else
            {
                return Ok(new RegisterUserResponse() { Success = false, Field = "message", Detail = string.Join(";", (result.Errors.Select(e => e.Description))) });
            }
        }
    }

    public class RegisterUserRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// A regisztrációnel ezen az osztályon keresztül kap tájékoztatát a kliens 
    /// a művelet eredményéről
    /// </summary>
    public class RegisterUserResponse
    {
        public bool Success { get; set; }

        public string Field { get; set; } = string.Empty;

        public string Detail { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
