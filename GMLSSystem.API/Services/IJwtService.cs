using GMLSSystem.API.Models;

namespace GMLSSystem.API.Services
{
    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user, IList<string> roles);
    }
}