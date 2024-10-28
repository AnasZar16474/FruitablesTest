using Fruitables.DAL.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fruitables.PL.Views.ViewModel.Home
{
    public class CommentProductVM
    {
        public int ProductId { get; set; }
        public string ApplicationUserId { get; set; }
        public int Rating { get; set; }
        public string? Text { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public Product Product { get; set; }
    }
}
