using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Finexa.Core.Services;
using Finexa.Domain.Entities;
using Finexa.Infrastructure.Database;
using Finexa.Shared.Security;

namespace Finexa.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly FinexaDbContext _context;
        private User? _currentUser;

        public User? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        public AuthService(FinexaDbContext context)
        {
            _context = context;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.IsActive);

            if (user == null)
            {
                return false;
            }

            if (HashHelper.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            {
                _currentUser = user;
                return true;
            }

            return false;
        }

        public void Logout()
        {
            _currentUser = null;
        }
    }
}
