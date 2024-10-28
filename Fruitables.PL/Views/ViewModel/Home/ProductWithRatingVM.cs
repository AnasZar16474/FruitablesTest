namespace Fruitables.PL.Views.ViewModel.Home
{
    public class ProductWithRatingVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImagName { get; set; } = null!;
        public decimal Price { get; set; }
        public double AverageRating { get; set; }
    }
}
