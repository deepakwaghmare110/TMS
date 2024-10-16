using System.ComponentModel.DataAnnotations;

namespace TMS.Models
{
    public class User
    {
        [Key]
        public int? Id { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public string? ResponseMessage { get; set; }
    }
}
