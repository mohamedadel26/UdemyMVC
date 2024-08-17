using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using System.Security.Claims;
using UdemyMVC.Models;
using UdemyMVC.ServiceLayer;
using UdemyMVC.ViewModels;

namespace UdemyMVC.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly UserManager<ApplicationModel> userManager;
        private readonly SignInManager<ApplicationModel> signInManager;
        private readonly UdemyDataBase context;

        public CourseController(IWebHostEnvironment webHostEnvironment ,
            UserManager<ApplicationModel>userManager , 
            SignInManager<ApplicationModel>signInManager,
            UdemyDataBase context)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
        }
        [HttpGet]
        public IActionResult Add()
        {
            return View("Add", new CourseViewModel());

        }
        [HttpPost]
        public IActionResult Add(CourseViewModel vm ,IFormFile photo)
        {
            if (!ModelState.IsValid) 
            return View("Add", vm);
                string? img = GetPhotoPath(photo);
            if (img == null) {
                ModelState.AddModelError("", "invalid photo");
                return View("Add", vm);
            } 
            
            vm.CourseImage = img; 
            Course course = CourseViewToCourse.Convert(vm, User.FindFirstValue(ClaimTypes.NameIdentifier));
            context.Courses.Add(course);
            context.SaveChanges();

            return RedirectToAction("Main", "Home");
        }
        public async Task<IActionResult> ViewCourse(int id) { 
            ViewBag.CourseId = id;
            Course? course = context.Courses.Include(s=>s.Instructor).FirstOrDefault(s=>s.ID == id);
            ViewBag.UserID =User.FindFirstValue(ClaimTypes.NameIdentifier); 
            return View("ViewCourse" , course);
        }
        public IActionResult Enrollment(string userId, int CourseID) {
            Enrollment en = new Enrollment
            {
                CourseID = CourseID,
                UserID = userId
            };   
            context.Enrollments.Add(en);
            context.SaveChanges();

            return RedirectToAction("Main", "Home");
        }

        private string? GetPhotoPath(IFormFile imageFile)
        {

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadPath = Path.Combine(webHostEnvironment.WebRootPath, "CourseImage");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                var uniquePath = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var photoPath = Path.Combine(uploadPath, uniquePath);
                using (var filestream = new FileStream(photoPath, FileMode.Create))
                {
                    imageFile.CopyTo(filestream);
                    filestream.Close();
                }
                return $"/CourseImage/+{uniquePath}";
            }
            return null;


        }
    }
}
