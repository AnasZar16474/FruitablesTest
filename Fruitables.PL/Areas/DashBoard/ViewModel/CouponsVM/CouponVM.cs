﻿namespace Fruitables.PL.Areas.DashBoard.ViewModel.CouponsVM
{
    public class CouponVM
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsActive { get; set; }
    }
}