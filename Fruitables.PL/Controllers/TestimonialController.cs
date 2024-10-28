using AutoMapper;
using Fruitables.DAL.Data;
using Fruitables.DAL.Models;
using Fruitables.PL.Areas.DashBoard.Controllers;
using Fruitables.PL.Views.ViewModel.Home;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fruitables.PL.Controllers
{
    public class TestimonialController : BaseController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public TestimonialController(UserManager<ApplicationUser> userManager, ApplicationDbContext context ,IMapper mapper) : base(userManager, context)
        {
            this.userManager = userManager;
            this.context = context;
            this.mapper = mapper;
        }
        public async Task<IActionResult> Index()
        {
            var reviews = await context.Reviews.Include(r => r.ApplicationUser).ToListAsync();
            var vm = mapper.Map<IEnumerable<CommentProductVM>>(reviews);
            return View(vm);
        }
    }
}
