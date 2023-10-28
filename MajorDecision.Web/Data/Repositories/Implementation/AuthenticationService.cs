using MajorDecision.Web.Data.Repositories.Abstract;
using MajorDecision.Web.Models;
using MajorDecision.Web.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MajorDecision.Web.Data.Repositories.Implementation
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;        

        public AuthenticationService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        public async Task<Status> LoginAsync(Login model)
        {
            var status = new Status();
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                status.StatusCode = 0;
                status.Message = "Invalid username";
                return status;
            }
                        
            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                status.StatusCode = 0;
                status.Message = "Invalid password";
                return status;
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, false, true);
            if (signInResult.Succeeded)
            {               
                status.StatusCode = 1;
                status.Message = "Logged successfully";
                return status;
            }
            else if (signInResult.IsLockedOut)
            {
                status.StatusCode = 0;
                status.Message = "User Locked out";
                return status;
            }
            else
            {
                status.StatusCode = 0;
                status.Message = "Error on loggin in";
                return status;
            }

        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<Status> RegistrationAsync(Registration model)
        {
            var status = new Status();
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                status.StatusCode = 0;
                status.Message = "User already created, create new username";
                return status;
            }

            ApplicationUser user = new ApplicationUser
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                Name = model.FirstName,
                Email = model.Email,
                UserName = model.Username,
                EmailConfirmed = true,
            };



            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                status.StatusCode = 0;
                status.Message = "User creation failed";
                return status;
            }           

            status.StatusCode = 1;
            status.Message = "User has registered successfully";
            return status;
        }

        public async Task<Status>ChangePasswordAsync(ChangePassword model, string username)
        {
            var status = new Status();
            var user=await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                status.Message = "User not found";
                status.StatusCode = 0;
                return status;
            }
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded && model.NewPassword==model.PasswordConfirm)
            {
                status.Message = "Password has updated successfully";
                status.StatusCode = 1;

                if (model.NewPassword != model.PasswordConfirm)
                {
                    status.Message = "New password and confirm password doesn't match";
                    status.StatusCode = 0;
                }
            }
          
            else
            {
                status.Message = "Old password is wrong";
                status.StatusCode = 0;
            }
            return status;
        }
    }
}
