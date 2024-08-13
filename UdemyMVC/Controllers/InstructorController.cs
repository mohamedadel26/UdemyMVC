﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UdemyMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace UdemyMVC.Controllers
{
    public class InstructorController : Controller
    {
        private readonly UserManager<ApplicationModel> userManager;
        private readonly UdemyDataBase context;

        public InstructorController(UserManager<ApplicationModel> userManager , 
            UdemyDataBase context)
        {
            this.userManager = userManager;
            this.context = context;
        }
        public IActionResult Profile(string id) {
            ApplicationModel? user = context.Users.Include(u => u.Course).FirstOrDefault(s=>s.Id==id);
            if (user == null) return NotFound();
            ViewBag.count = context.Courses.Where(c => c.InstructorID == id).Count();
            ViewBag.Courses = context.Courses.Where(c => c.InstructorID == id);
            return View("Profile",user);
        }
    }
}
