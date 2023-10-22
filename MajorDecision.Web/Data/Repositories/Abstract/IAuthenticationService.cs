using MajorDecision.Web.Models.Authentication;

namespace MajorDecision.Web.Data.Repositories.Abstract
{
    public interface IAuthenticationService
    {
        Task<Status> LoginAsync(Login model);
        Task<Status> RegistrationAsync(Registration model);
        Task LogoutAsync();
        Task<Status>ChangePasswordAsync(ChangePassword model, string username);
    }
}
