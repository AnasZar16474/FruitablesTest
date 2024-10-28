namespace Fruitables.PL.Areas.DashBoard.ViewModel.CouponsVM
{
    public class CouponCreateVM
    {
        public string Code { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
