using AutoMapper;
using Fruitables.DAL.Data;
using Fruitables.DAL.Models;
using Fruitables.PL.Areas.DashBoard.Controllers;
using Fruitables.PL.Models;
using Fruitables.PL.Views.ViewModel.Home;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Fruitables.PL.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;
        private readonly UserManager<ApplicationUser> userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext, IMapper mapper, UserManager<ApplicationUser> userManager) : base(userManager, dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return RedirectToAction("Login", "Accounts", new { area = "Identity" });
            }
            var product =await dbContext.Products.Where(P => P.Type == DAL.Models.ProductType.Fruit ).ToListAsync();
            var vm = mapper.Map<IEnumerable<ProductsVM>>(product);
            var productA =await dbContext.Products.Where(P => P.Type == DAL.Models.ProductType.Vegetable).ToListAsync();
            var vmA = mapper.Map<IEnumerable<ProductsVM>>(productA);
            var cart = await dbContext.Carts
               .Include(c => c.CartItem)
                   .ThenInclude(ci => ci.Product)
               .FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
           var TopSelling= GetTopSellingProducts(6);
            var productsWithAvgRating = await dbContext.Products

          .Select(p => new ProductWithRatingVM
          {
              ImagName=p.ImagName,
              Price= p.Price,
              Id = p.Id,
              Name = p.Name,
              AverageRating = dbContext.Reviews
                  .Where(r => r.ProductId == p.Id)
                  .Average(r => (double?)r.Rating) ?? 0,
             
          })
          .ToListAsync();
            var reviews = await dbContext.Reviews.Include(r => r.ApplicationUser).ToListAsync();
            var vmB = mapper.Map<IEnumerable<CommentProductVM>>(reviews);
            var model = new CompositeVM
            {
                ProductWithRating= productsWithAvgRating,
                TopSelling = TopSelling,
                ProductFruit = vm,
                ProductVegetable=vmA,
                commentProducts=vmB,
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public List<ProductWithRatingVM> GetTopSellingProducts(int topN)
        {
            var topSellingProducts = dbContext.OrderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(topN)
                .ToList();
            var productIds = topSellingProducts.Select(x => x.ProductId).ToList();
            var products = dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => new ProductWithRatingVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImagName = p.ImagName,
                    AverageRating = dbContext.Reviews
                        .Where(r => r.ProductId == p.Id)
                        .Average(r => (double?)r.Rating) ?? 0
                })
                .ToList();

            return products;
        }

    }
}
