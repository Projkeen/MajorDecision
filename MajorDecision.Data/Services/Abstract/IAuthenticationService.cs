using MajorDecision.Web.Models.Entities;
using MajorDecision.Web.Models.ViewModels.Authentication;

namespace MajorDecision.Data.Services.Abstract
{
    public interface IAuthenticationService
    {
        Task<(Status Status, ApplicationUser? User, IList<string> userRoles)> LoginAsync(Login model);
        Task<(Status Status, ApplicationUser? User, IList<string> userRoles)> RegistrationAsync(Registration model);
        Task LogoutAsync();
        Task<Status> ChangePasswordAsync(ChangePassword model, string username);
    }
}
