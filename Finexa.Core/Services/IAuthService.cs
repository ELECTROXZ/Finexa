using System.Threading.Tasks;
using Finexa.Domain.Entities;

namespace Finexa.Core.Services
{
    public interface IAuthService
    {
        User? CurrentUser { get; }
        bool IsAuthenticated { get; }
        Task<bool> LoginAsync(string username, string password);
        void Logout();
    }
}
