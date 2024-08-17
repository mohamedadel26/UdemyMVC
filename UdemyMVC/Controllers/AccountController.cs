using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UdemyMVC.ViewModels;
using ApplicationModel = UdemyMVC.Models.ApplicationModel;


namespace UdemyMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationModel> userManager;
        private readonly SignInManager<ApplicationModel> signInManager;

        public AccountController(UserManager<ApplicationModel> _userManager  , SignInManager<ApplicationModel> _signInManager)
        {
            this.userManager = _userManager;
            this.signInManager = _signInManager;    
        }
        [HttpGet]
        public  IActionResult Login()
        { 
           
            if (signInManager.IsSignedIn(User)) {
                return RedirectToAction("Main", "Home");
            }
                return View("Login", new LoginViewModelcs());
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModelcs vm) {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Main", "Home");
            }
            if (ModelState.IsValid) { 
       var result=    await userManager.FindByEmailAsync(vm.Email);
                if (result == null) {
                    ModelState.AddModelError("", "Invalid Login!");
                    return View("Login", vm);
                }
                bool found= await  userManager.CheckPasswordAsync(result, vm.Password);
                if (!found) {
                    ModelState.AddModelError("", "Invalid Login");
                    return View("Login", vm);
                }
               await signInManager.SignInAsync(result, vm.RememberMe);
             return    RedirectToAction("Main", "Home");
            }
            return View("Login", vm); 
        }

        public async Task<IActionResult> Logout() {
            if (signInManager.IsSignedIn(User))
            {
            await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            else {
                return RedirectToAction("Login", "Account");    
            }
          

        }
      
    }
}
