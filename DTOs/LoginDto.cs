using System.ComponentModel.DataAnnotations;

namespace Bookify_API.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }

    }
}
