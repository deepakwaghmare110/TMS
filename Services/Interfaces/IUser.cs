using TMS.Models;

namespace TMS.Services.Interfaces
{
    public interface IUser
    {
        public Task<string> CreateUserAsync(User user);

        public Task<User> LoginUserAsync(User user);
    }
}
