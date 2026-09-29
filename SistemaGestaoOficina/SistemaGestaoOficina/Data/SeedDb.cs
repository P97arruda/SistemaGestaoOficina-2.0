using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Helpers;

namespace SistemaGestaoOficina.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public SeedDb(
            DataContext context,
            IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();

            // Criar Roles
            await _userHelper.CheckRoleAsync("Admin");
            await _userHelper.CheckRoleAsync("Funcionario");
            await _userHelper.CheckRoleAsync("Cliente");

            // Procurar o Admin
            var user = await _userHelper.GetUserByEmailAsync("testecinel007@gmail.com");

            // Se não existir, criar
            if (user == null)
            {
                user = new User
                {
                    FirstName = "Admin",
                    LastName = "Oficina",
                    Email = "testecinel007@gmail.com",
                    UserName = "testecinel007@gmail.com",
                    PhoneNumber = "900000001"
                };

                var result = await _userHelper.AddUserAsync(user, "Admin123!");

                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Could not create the Admin user in seeder");
                }

                await _userHelper.AddUserToRoleAsync(
                    user,
                    "Admin");

                // Confirmar email automaticamente
                var token =
                    await _userHelper.GenerateEmailConfirmationTokenAsync(user);

                await _userHelper.ConfirmEmailAsync(
                    user,
                    token);
            }

            // Garantir que o utilizador pertence ao Role Admin
            var isInRole =
                await _userHelper.IsUserInRoleAsync(
                    user,
                    "Admin");

            if (!isInRole)
            {
                await _userHelper.AddUserToRoleAsync(
                    user,
                    "Admin");
            }
        }
    }
}