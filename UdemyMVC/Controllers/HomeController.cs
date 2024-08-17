using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UdemyMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace UdemyMVC.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationModel> signInManager;
        private readonly UserManager<ApplicationModel> userManager;
        private readonly UdemyDataBase context;

        public HomeController(ILogger<HomeController> logger,SignInManager<ApplicationModel> signInManager,
			UserManager<ApplicationModel> userManager,
			UdemyDataBase context)
		{
			_logger = logger;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.context = context;
        }
        public async Task<IActionResult> Index()
		{
            if (signInManager.IsSignedIn(User))
            {
				return RedirectToAction("Main", "Home");
            }
            var roles = await userManager.GetUsersInRoleAsync("Instructor");
            ViewBag.instructors = roles;
			var course = context.Courses.Include(s => s.Instructor).ToList();
			ViewBag.course = course;
            return View("Index");
		}
		public async Task<IActionResult> Main() {
            var roles = await userManager.GetUsersInRoleAsync("Instructor");
            ViewBag.instructors = roles;
            var course = context.Courses.Include(s => s.Instructor).ToList();
            ViewBag.course = course;
            return View("Main");	
		}

		public IActionResult About()
		{
			return View("About");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
