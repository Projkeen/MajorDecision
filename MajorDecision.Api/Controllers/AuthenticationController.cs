using MajorDecision.Data.Dto.Authentication;
using MajorDecision.Data.Services.Abstract;
using MajorDecision.Web.Models.Entities;
using MajorDecision.Web.Models.ViewModels.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MajorDecision.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        public AuthenticationController(IAuthenticationService authService, ITokenService tokenService, UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _tokenService = tokenService;
            _userManager = userManager;
        }

        [HttpPost("registration")]
        public async Task<IActionResult> Registration([FromBody] Registration model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (status, user, roles) = await _authService.RegistrationAsync(model);
            if (status.StatusCode == 0 || user == null)
                return BadRequest(new { status.StatusCode, status.Message });
            //var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.CreateToken(user, roles);

            return Ok(new
            {
                User = new NewUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    Token = token,
                },
                Status = new { status.StatusCode, status.Message }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var (status, user, roles) = await _authService.LoginAsync(model);
            if (status.StatusCode == 0 || user == null)
                return BadRequest(new { status.StatusCode, status.Message });
            //var roles = await _userManager.GetRolesAsync(user);
            //var role = await _authService.GetRoleAsync(user.UserName);
            var token = _tokenService.CreateToken(user, roles);
            return Ok(new
            {
                User = new NewUser
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = token,
                },
                Status = new { status.StatusCode, status.Message }
            });
        }        
    }
}
