using Fruitables.DAL.Data;
using Fruitables.DAL.Models;
using Fruitables.PL.Areas.DashBoard.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fruitables.PL.Controllers
{
    public class CartController : BaseController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;

        public CartController(UserManager<ApplicationUser> userManager, ApplicationDbContext _context) : base(userManager, _context)
        {
            this.userManager = userManager;
            context = _context;
        }
        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);
            if(user is null)
            {
                return RedirectToAction("Login", "Accounts", new { area = "Identity" });
            }
            var cart = await context.Carts
                .Include(c => c.CartItem)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { ApplicationUserId = user.Id };
                context.Carts.Add(cart);
                await context.SaveChangesAsync();
            }
            return View(cart);
        }
        public async Task<IActionResult> Add(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Json(new { success = false, message = "يرجى تسجيل الدخول أولاً" });
            }
            var cart = await context.Carts
                .Include(c => c.CartItem)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { ApplicationUserId = user.Id };
                context.Carts.Add(cart);
                await context.SaveChangesAsync();
            }

            var cartItem = cart.CartItem.FirstOrDefault(ci => ci.ProductId == Id);
            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = Id,
                    Quantity = 1
                };
                context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity++;
                context.CartItems.Update(cartItem);
            }
            await context.SaveChangesAsync();
            var totalPrice = ViewBag.TotalPrice;
            if (TempData["Discount"] != null)
            {
                var discountString = TempData["Discount"]?.ToString();
                decimal discount;
                if (!decimal.TryParse(discountString, out discount))
                {
                    discount = 0;
                }
                var discountedPrice = GetTotalPriceAfterDiscount(totalPrice, discount);
            }
            else
            {
                var discountedPrice = GetTotalPriceAfterDiscount(totalPrice, 0);
            }
            int cartItemCount = await context.CartItems
           .Where(ci => ci.CartId == cart.CartId) 
            .SumAsync(item => item.Quantity);
            return Json(new { success = true, cartItemCount });
        }
        public IActionResult decrease(int newQuantityA,int cartItemIdA)
        {
            var count = newQuantityA-1;
            if (count <= 0)
            {
                return BadRequest("Quantity must be greater than zero.");
            }
            var cartItem = context.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemIdA);

            if (cartItem == null)
            {
                return NotFound("CartItem not found.");
            }
            cartItem.Quantity = count;
            context.SaveChanges();
            var totalPrice = ViewBag.TotalPrice;
            if (TempData["Discount"] != null)
            {
                var discountString = TempData["Discount"]?.ToString();
                decimal discount;
                if (!decimal.TryParse(discountString, out discount))
                {
                    discount = 0;
                }
                var discountedPrice = GetTotalPriceAfterDiscount(totalPrice, discount);
            }
            else
            {
                var discountedPrice = GetTotalPriceAfterDiscount(totalPrice, 0);
            }
            return RedirectToAction("Index");
        }
        public IActionResult Increase(int newQuantityB, int cartItemIdB)
        {
            var count = newQuantityB + 1;
            if (count <= 0)
            {
                return BadRequest("Quantity must be greater than zero.");
            }
            var cartItem = context.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemIdB);

            if (cartItem == null)
            {
                return NotFound("CartItem not found.");
            }
            cartItem.Quantity = count;
            var totalPrice = ViewBag.TotalPrice;
            if (TempData["Discount"] != null)
            {
                var discountString = TempData["Discount"]?.ToString();
                decimal discount;
                if (!decimal.TryParse(discountString, out discount))
                {
                    discount = 0;
                }
                var discountedPrice = GetTotalPriceAfterDiscount(totalPrice, discount);
            }
            else
            {
                var discountedPrice = GetTotalPriceAfterDiscount(totalPrice, 0);
            }
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult RemovtItem(int Id)
        {
            var cartItem = context.CartItems.FirstOrDefault(ci => ci.CartItemId == Id);
            context.CartItems.Remove(cartItem);
            context.SaveChanges();
            var totalPrice = ViewBag.TotalPrice;
            if (TempData["Discount"] != null)
            {
                var discountString = TempData["Discount"]?.ToString();
                decimal discount;
                if (!decimal.TryParse(discountString, out discount))
                {
                    discount = 0; 
                }
                var discountedPrice = GetTotalPriceAfterDiscount(totalPrice, discount);
            }
            else
            {
                var discountedPrice = GetTotalPriceAfterDiscount(totalPrice, 0);
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            var coupon = await context.Coupons
                .FirstOrDefaultAsync(c => c.Code == couponCode && c.IsActive && c.ExpirationDate > DateTime.Now);

            if (coupon != null)
            {
                var discount = coupon.DiscountPercentage;
                var user = await userManager.GetUserAsync(User);

                if (user is null)
                {
                    return RedirectToAction("Login", "Accounts", new { area = "Identity" });
                }

                var cart = await context.Carts
                    .Include(c => c.CartItem)
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);

                if (cart == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                var totalPrice = ViewBag.TotalPrice;
                var discountedPrice = GetTotalPriceAfterDiscount(totalPrice, coupon.DiscountPercentage);
                coupon.IsActive = false;
                context.SaveChanges();
                return Json(new { success = true, message = "Coupon is Applied", discountedPrice });
            }
            else
            {
                return Json(new { success = false, message = "Coupon is expired or invalid" });
            }
        }
    }
}
