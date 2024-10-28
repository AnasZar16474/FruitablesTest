using Fruitables.DAL.Data;
using Fruitables.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Fruitables.PL.Areas.DashBoard.Controllers
{
    public class BaseController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext dbContext;

        public BaseController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
        }

        public JsonResult GetCartItemCount()
        {
            int cartItemCount = HttpContext.Session.GetInt32("TotalProduct") ?? 0;
            return Json(new { count = cartItemCount });
        }
        public JsonResult GetTotalPrice()
        {
            var totalPrice = HttpContext.Session.GetString("TotalPrice") ?? "";
            decimal.TryParse(totalPrice, out decimal TotalPrice);
            var FinalPrice = TotalPrice + 3;
            return Json(new { totalPrice = TotalPrice, totalPrice1 = FinalPrice });
        }
        public JsonResult GetTotalPriceAfterDiscount(decimal totalPrice, decimal discount)
        {
            TempData["Discount"] = discount.ToString();
            var discountedPrice = totalPrice - (totalPrice * (discount / 100));
            HttpContext.Session.SetString("DiscountedPrice", discountedPrice.ToString());
            var FinalPrice = discountedPrice + 3;
            return Json(new {  discountedPrice, FinalPrice });
        }
        public async override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                context.Result = RedirectToAction("Login", "Accounts", new { area = "Identity" });
                return;
            }

            var cart = await dbContext.Carts
               .Include(c => c.CartItem)
               .ThenInclude(ci => ci.Product)
               .FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);

            if (cart != null)
            {
                var totalPrice = cart.CartItem.Sum(ci => ci.Product.Price * ci.Quantity);
                int totalProducts = cart.CartItem.Sum(ci => ci.Quantity);

                HttpContext.Session.SetInt32("TotalProduct", totalProducts);
                HttpContext.Session.SetString("TotalPrice", totalPrice.ToString());
            }

            ViewBag.TotalProducts = GetSessionInt("TotalProduct", 0);
            ViewBag.Location = user.Address ?? "";
            ViewBag.TotalPrice = GetSessionDecimal("TotalPrice", 0);
            ViewBag.DiscountedPrice = GetSessionDecimal("DiscountedPrice", 0);

            await base.OnActionExecutionAsync(context, next);
        }

        private int GetSessionInt(string key, int defaultValue)
        {
           return HttpContext.Session.GetInt32(key) ?? defaultValue;
        }
        private decimal GetSessionDecimal(string key, decimal defaultValue)
        {
            var valueString = HttpContext.Session.GetString(key);
            return decimal.TryParse(valueString, out var value) ? value : defaultValue;
        }

    }
}
