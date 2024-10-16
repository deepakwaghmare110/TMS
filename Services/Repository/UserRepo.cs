using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TMS.Data;
using TMS.Models;
using TMS.Services.Interfaces;

namespace TMS.Services.Repository
{
    public class UserRepo : IUser
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<UserRepo> _logger;

        public UserRepo(ApplicationDBContext context, ILogger<UserRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> CreateUserAsync(User user)
        {
            try
            {
                var UserNameParam = new SqlParameter("@UserName", user.UserName ?? (object)DBNull.Value);
                var passwordParam = new SqlParameter("@Password", user.Password ?? (object)DBNull.Value);


                var responseMessageParam = new SqlParameter("@ResponseMessage", SqlDbType.NVarChar, 256) { Direction = ParameterDirection.Output };

                await _context.Database.ExecuteSqlRawAsync(
                   "EXEC [dbo].[RegisterUser] @UserName, @Password, @ResponseMessage OUTPUT",
                   UserNameParam, passwordParam, responseMessageParam
               );

                var ResponseMessage = responseMessageParam.Value == DBNull.Value ? string.Empty : (string)responseMessageParam.Value;

                return ResponseMessage;


            }
            catch (Exception ex)
            {
                return "Internal Server Error :" + ex.Message;

            }

        }

        public async Task<User> LoginUserAsync(User user)
        {
            try
            {
                var UserNameParam = new SqlParameter("@UserName", user.UserName ?? (object)DBNull.Value);
                var passwordParam = new SqlParameter("@Password", user.Password ?? (object)DBNull.Value);

                var UserIdParam = new SqlParameter("@UserIdOutput", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var RoleParam = new SqlParameter("@Role", SqlDbType.NVarChar, 10) { Direction = ParameterDirection.Output };
                var responseMessageParam = new SqlParameter("@ResponseMessage", SqlDbType.NVarChar, 256) { Direction = ParameterDirection.Output };

                await _context.Database.ExecuteSqlRawAsync(
                   "EXEC [dbo].[LoginUser] @UserName, @Password, @ResponseMessage OUTPUT, @UserIdOutput OUTPUT, @Role OUTPUT",
                   UserNameParam, passwordParam, responseMessageParam, UserIdParam, RoleParam
               );
                

                return new User
                {
                    ResponseMessage = responseMessageParam.Value == DBNull.Value ? string.Empty : (string)responseMessageParam.Value,
                    Id = UserIdParam.Value == DBNull.Value ? 0 : Convert.ToInt32(UserIdParam.Value),
                    Role = RoleParam.Value == DBNull.Value ? string.Empty : (string)RoleParam.Value,
                };


            }
            catch (Exception ex)
            {
                return new User
                {
                    ResponseMessage = "Internal Server Error :" + ex.Message,
                    Id = null,
                    Role = null,
                };

            }
        }
    }
}
