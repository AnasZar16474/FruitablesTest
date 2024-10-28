using Fruitables.DAL.Models;

namespace Fruitables.PL.Views.ViewModel.Home
{
    public class CompositeVM
    {
        public IEnumerable<ProductWithRatingVM> ProductWithRating { get; set; } = null!;
        public IEnumerable<ProductsVM> ProductFruit { get; set; } = null!;
        public IEnumerable<ProductsVM> ProductVegetable { get; set; } = null!;
        public IEnumerable<ProductWithRatingVM> TopSelling { get; set; } = null!;
        public IEnumerable<CommentProductVM> commentProducts { get; set; } = null!;


    }
}
