using MajorDecision.Data.Services.Abstract;
using MajorDecision.Web.Models.Entities;
using MajorDecision.Web.Models.ViewModels.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace MajorDecision.Data.Services.Implementation
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
        public async Task<(Status Status, ApplicationUser? User, IList<string> userRoles)> LoginAsync(Login model)
        {
            var status = new Status();
            var user = await _userManager.FindByNameAsync(model.UsernameOrEmail);
            var email = await _userManager.FindByEmailAsync(model.UsernameOrEmail);
            if (user != null)
            {
                var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
                if (signInResult.Succeeded)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName)
                    };
                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }

                    status.StatusCode = 1;
                    status.Message = "Logged successfully";
                    return (status, user, userRoles);
                }
                else if (signInResult.IsLockedOut)
                {
                    status.StatusCode = 0;
                    status.Message = "User Locked out";
                    return (status, null, new List<string>());
                }
                else if (!await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    status.StatusCode = 0;
                    status.Message = "Invalid password";
                    return (status, null, new List<string>());
                }
                else
                {
                    status.StatusCode = 0;
                    status.Message = "Error on loggin in";
                    return (status, null, new List<string>());
                }
            }
            else if (email != null)
            {
                var signInResult = await _signInManager.PasswordSignInAsync(email, model.Password, model.RememberMe, true);
                if (signInResult.Succeeded)
                {
                    var userRoles = await _userManager.GetRolesAsync(email);
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, email.UserName)
                    };
                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }
                    status.StatusCode = 1;
                    status.Message = "Logged successfully";
                    return (status, email, userRoles);
                }
                else if (signInResult.IsLockedOut)
                {
                    status.StatusCode = 0;
                    status.Message = "User Locked out";
                    return (status, null, new List<string>());
                }
                else if (!await _userManager.CheckPasswordAsync(email, model.Password))
                {
                    status.StatusCode = 0;
                    status.Message = "Invalid password";
                    return (status, null, new List<string>());
                }
                else
                {
                    status.StatusCode = 0;
                    status.Message = "Error on loggin in";
                    return (status, null, new List<string>());
                }
            }
            else
            {
                status.StatusCode = 0;
                status.Message = "Username is not found";
                return (status, null, new List<string>());
            }
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<(Status Status, ApplicationUser? User, IList<string> userRoles)> RegistrationAsync(Registration model)
        {
            var status = new Status();
            var userExists = await _userManager.FindByNameAsync(model.Username);
            var checkEmail = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                status.StatusCode = 0;
                status.Message = "Error! User already created, create new username";
                return (status, null, new List<string>());
            }
            if (checkEmail != null)
            {
                status.StatusCode = 0;
                status.Message = "Error! Email already using";
                return (status, null, new List<string>());
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
                model.Role = "Admin";
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
                return (status, null, new List<string>());
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
            var userRoles = await _userManager.GetRolesAsync(user);
            status.StatusCode = 1;
            status.Message = "User has registered successfully";
            return (status, user, userRoles);
        }

        public async Task<Status> ChangePasswordAsync(ChangePassword model, string userId)
        {
            var status = new Status();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                status.StatusCode = 0;
                status.Message = "User not found";
                return status;
            }

            if (model.NewPassword != model.PasswordConfirm)
            {
                status.StatusCode = 0;
                status.Message = "New password and confirm password doesn't match";
                return status;
            }

            if (model.CurrentPassword == model.NewPassword)
            {
                status.StatusCode = 0;
                status.Message = "The old password must not be the same as the new password";
                return status;
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                status.StatusCode = 0;
                status.Message = "Old password is wrong";
                return status;
            }

            status.StatusCode = 1;
            status.Message = "Password has updated successfully";
            return status;
        }
    }
}
