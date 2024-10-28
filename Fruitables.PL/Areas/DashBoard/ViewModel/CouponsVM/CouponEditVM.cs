namespace Fruitables.PL.Areas.DashBoard.ViewModel.CouponsVM
{
    public class CouponEditVM
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
