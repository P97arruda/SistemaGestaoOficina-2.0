using Microsoft.AspNetCore.Identity;
using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Helpers
{
    public interface IUserHelper
    {
        Task<User> GetUserByEmailAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string password);
    }
}
