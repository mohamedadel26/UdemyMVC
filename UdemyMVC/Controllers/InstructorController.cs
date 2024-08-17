using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UdemyMVC.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using UdemyMVC.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace UdemyMVC.Controllers
{
    [Authorize]
    public class InstructorController : Controller
    {
        private readonly UserManager<ApplicationModel> userManager;
        private readonly UdemyDataBase context;
        private readonly IWebHostEnvironment webHostEnvironment;

        public InstructorController(UserManager<ApplicationModel> userManager , 
            UdemyDataBase context, IWebHostEnvironment webHostEnvironment)
        {
            this.userManager = userManager;
            this.context = context;
            this.webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Profile(string id) {
            ApplicationModel? user = context.Users.Include(u => u.Course).FirstOrDefault(s=>s.Id==id);
            if (user == null) return NotFound();
            ViewBag.count = context.Courses.Where(c => c.InstructorID == id).Count();
            ViewBag.Courses = context.Courses.Where(c => c.InstructorID == id);
            return View("Profile",user);
        }

        public IActionResult ShowCourses()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationModel? user = context.Users.Include(s => s.Enrolement).ThenInclude(s => s.Course).FirstOrDefault(s => s.Id == id);
            return View("AllCourses", user);
        }

        public IActionResult Edit(string id) {
            ApplicationModel? user = context.Users.Include(u => u.Course).FirstOrDefault(s=>s.Id==id);
            if (user == null) return NotFound();
            EditInstructorViewModel vm = new EditInstructorViewModel()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Address = user.Address,
                Field = user.Field,
                Image = user.Image
            };
            return View("Edit",vm);
        }

        [HttpPost]
        public IActionResult SaveEdit(EditInstructorViewModel user,IFormFile profileImg) {

            if(!ModelState.IsValid) return View("Edit", user);
            ApplicationModel? _user = context.Users.Include(u => u.Course).FirstOrDefault(s=>s.Id==user.Id);
            if (_user == null) return NotFound();
            _user.UserName = user.UserName;
            _user.Email = user.Email;
            _user.Address = user.Address;
            _user.Field = user.Field ?? " ";
            if (profileImg != null) {
                // delete old image
                if (_user.Image != null) {
                    string oldPath = Path.Combine(webHostEnvironment.WebRootPath, _user.Image);
                    if (System.IO.File.Exists(oldPath)) {
                        System.IO.File.Delete(oldPath);
                    }
                }
                string? img = GetPhotoPath(profileImg);
                _user.Image = img; 
            }
            context.Users.Update(_user);
            context.SaveChanges();
            return RedirectToAction("Profile",new {id = user.Id});
        }


        private string? GetPhotoPath(IFormFile imageFile) {
            if (imageFile != null && imageFile.Length > 0) {
                var uploadPath = Path.Combine(webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadPath)) {
                    Directory.CreateDirectory(uploadPath);
                }
                var uniquePath = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var photoPath = Path.Combine(uploadPath, uniquePath);
                using (var filestream = new FileStream(photoPath, FileMode.Create)) {
                    imageFile.CopyTo(filestream);
                    filestream.Close();
                }
                return $"/uploads/+{uniquePath}";
            }
            return null;
        }
    }
}
