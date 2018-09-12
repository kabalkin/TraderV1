using System.ComponentModel.DataAnnotations;

namespace TraderV1.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Не указан usename")]
        public string UserName { get; set; }
 
        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}