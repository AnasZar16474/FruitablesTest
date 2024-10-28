using AutoMapper;
using Fruitables.DAL.Data;
using Fruitables.DAL.Models;
using Fruitables.PL.Areas.DashBoard.ViewModel.CouponsVM;
using Fruitables.PL.Areas.DashBoard.ViewModel.ProductFruitsVM;
using Fruitables.PL.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Fruitables.PL.Areas.DashBoard.Controllers
{
    [Area("DashBoard")]
    public class CouponsController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;

        public CouponsController(ApplicationDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }
        public IActionResult Index()
        {
            var coupon = dbContext.Coupons.ToList();
            var vm = mapper.Map<IEnumerable<CouponVM>>(coupon);
            return View(vm);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CouponCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var coupons = mapper.Map<Coupon>(model);
            coupons.IsActive = true;
            dbContext.Coupons.Add(coupons);
            dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int Id)
        {
            var coupon = dbContext.Coupons.Find(Id);
            if (coupon is null)
            {
                return RedirectToAction(nameof(Index));
            }
            var vm = mapper.Map<CouponDeleteVM>(coupon);
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletedConfirmed(int Id)
        {
            var coupon = dbContext.Coupons.Find(Id);
            if (coupon is null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                dbContext.Remove(coupon);
                dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

        }
        public IActionResult Edit(int Id)
        {
            var coupon = dbContext.Coupons.Find(Id);
            if (coupon is null)
            {
                return NotFound();
            }
            else
            {

                return View(mapper.Map<CouponEditVM>(coupon));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CouponEditVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var coupon = dbContext.Coupons.Find(vm.Id);

            if (coupon is null)
            {
                return NotFound();
            }
            else
            {
                mapper.Map(vm, coupon);
                dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

        }
    }
}
