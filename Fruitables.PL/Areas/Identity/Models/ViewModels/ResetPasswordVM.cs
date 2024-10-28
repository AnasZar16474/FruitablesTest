using System.ComponentModel.DataAnnotations;

namespace Fruitables.PL.Areas.Identity.Models.ViewModels
{
    public class ResetPasswordVM
    {
        [DataType(DataType.Password)]
        public string password { get; set; }
        [DataType(DataType.Password)]
        [Compare(nameof(password))]
        public string confirmedPassword {  get; set; }
        public string Email {  get; set; }
        public string Token {  get; set; }
    }
}
