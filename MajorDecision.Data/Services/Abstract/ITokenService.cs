using MajorDecision.Web.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajorDecision.Data.Services.Abstract
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser applicationUser, IList<string> roles);
    }
}
