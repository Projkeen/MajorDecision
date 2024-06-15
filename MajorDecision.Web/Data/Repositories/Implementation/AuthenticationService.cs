using MajorDecision.Web.Data.Repositories.Abstract;
using MajorDecision.Web.Models;
using MajorDecision.Web.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace MajorDecision.Web.Data.Repositories.Implementation
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthenticationService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<Status> LoginAsync(Login model)
        {
            var status = new Status();
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                status.StatusCode = 0;
                status.Message = "Username is not found";
                return status;
            }

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                status.StatusCode = 0;
                status.Message = "Invalid password";
                return status;
            }
            
            //await _userManager.AddToRoleAsync(user, "Admin");
            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, false, true);
            if (signInResult.Succeeded)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName)
                };
                foreach(var  userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                //if (!await _roleManager.RoleExistsAsync("User"))
                //    await _roleManager.CreateAsync(new IdentityRole
                //    {
                //        Name = "User",
                //        NormalizedName = "USER",
                //        Id = Guid.NewGuid().ToString(),
                //        ConcurrencyStamp = Guid.NewGuid().ToString()
                //    });
                //if (await _roleManager.RoleExistsAsync("User"))
                //{
                //    await _userManager.AddToRoleAsync(user,"User");
                //}


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
                status.Message = "Error! User already created, create new username";
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

            if (model.SecretPassword == "123456789")
            {
                //model.Role = "Admin";
                model.Role="Admin";
            }
            else
            {
                model.Role = "User";
            }

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                status.StatusCode = 0;
                status.Message = "User creation failed";
                return status;
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = model.Role,
                    NormalizedName = model.Role.ToUpper(),
                    Id = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                });
            if (await _roleManager.RoleExistsAsync(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }
            
                
            status.StatusCode = 1;
            status.Message = "User has registered successfully";
            return status;
        }

        public async Task<Status> ChangePasswordAsync(ChangePassword model, string username)
        {
            var status = new Status();
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                status.Message = "User not found";
                status.StatusCode = 0;
                return status;
            }
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded && model.NewPassword == model.PasswordConfirm)
            {
                status.Message = "Password has updated successfully";
                status.StatusCode = 1;

                if (model.NewPassword != model.PasswordConfirm)
                {
                    status.Message = "New password and confirm password doesn't match";
                    status.StatusCode = 0;
                }
                if (model.CurrentPassword == model.NewPassword)
                {
                    status.Message = "The old password must not be the same as the new password";
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
