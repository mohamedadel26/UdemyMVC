using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UdemyMVC.Models;
using UdemyMVC.ServiceLayer;
using UdemyMVC.ViewModels;

namespace UdemyMVC.Controllers
{ 

	public class RegisterController : Controller
	{
		UdemyDataBase context; 
		private readonly UserManager<ApplicationModel> userManager;
		private readonly SignInManager<ApplicationModel> signInManager;
		private readonly RoleManager<IdentityRole> roleManager;
        private readonly IWebHostEnvironment webHostEnvironment;

        public RegisterController(UserManager<ApplicationModel> userManager ,SignInManager<ApplicationModel>signInManager,
			RoleManager<IdentityRole> roleManager ,
            IWebHostEnvironment webHostEnvironment,
            UdemyDataBase _context)
        {
			this.userManager = userManager; 
			this.signInManager = signInManager;
			this.roleManager = roleManager;
            this.webHostEnvironment = webHostEnvironment;
            this.context = _context;
		}


		[HttpGet]
		public IActionResult Register()
		{ 
	     if (signInManager.IsSignedIn(User))
            {
				return RedirectToAction("Main", "Home");
            }
            return View("Register" , new RegisterViewModels());
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModels vm ,IFormFile photo)
		{
			if (signInManager.IsSignedIn(User))
			{
				return RedirectToAction("Main", "Home");
            }
				if (ModelState.IsValid)
				{

					
					if (!await roleManager.RoleExistsAsync(vm.Role))
					{
						IdentityResult result1 = await roleManager.CreateAsync(new IdentityRole(vm.Role));
						if (!result1.Succeeded)
						{
							ModelState.AddModelError("", "failed to create role!");
							return View(vm);
						}



					}

				string? img = GetPhotoPath(photo);
				if (img == null)
				{
					ModelState.AddModelError("", "Invalid Photo selected!");
					return View(vm);

				}
				vm.Image = img;
				
				var userModel = RegisterUser.SignToUser(vm);//var for any change in IdenetityUser(clean code)
				
				IdentityResult result = await userManager.CreateAsync(userModel, vm.Password);
					if (!result.Succeeded)
					{
						foreach (var err in result.Errors)
						{
							ModelState.AddModelError("", err.Description);
						}
						return View(vm);
					}
					IdentityResult role = await userManager.AddToRoleAsync(userModel, vm.Role);
					if (!role.Succeeded)
					{
						ModelState.AddModelError("", "Failed to assign Role");
						return View(vm);
					}
				if (vm.Role != "User") { 
					IdentityResult role2 = await userManager.AddToRoleAsync(userModel,"User");
                    if (!role2.Succeeded)
                    {
                        ModelState.AddModelError("", "Failed to assign Role");
                        return View(vm);
                    }

                }

					
		
					await signInManager.SignInAsync(userModel, vm.RememberMe);
					context.SaveChanges();
				return RedirectToAction("Main", "Home");
				}



				return View(vm);
			}
		private  string? GetPhotoPath(IFormFile imageFile)
		{

			if (imageFile != null && imageFile.Length > 0)
			{
				var uploadPath = Path.Combine(webHostEnvironment.WebRootPath, "uploads");
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
				return $"/uploads/+{uniquePath}"; 
			}
			return null;


		}
	}
}
