using TMS.Models;

namespace TMS.Services.Interfaces
{
    public interface IUser
    {
        public Task<string> CreateUserAsync(User user);

        public Task<User> LoginUserAsync(User user);

        public Task<IEnumerable<User>> GetUsersAsync();
        public Task<string> UpdateUserAsync(User user, int userId);
        public Task<bool> DeleteUser(int userId);
    }
}
