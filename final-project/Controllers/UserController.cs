﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using final_project.Models;
using final_project.Data;
using Microsoft.AspNetCore.Http;

namespace final_project.Controllers
{
    public class UserController : Controller
    {
        private readonly SweetShopContext _context;

        public UserController(SweetShopContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (email == null || password == null)
            {
                TempData["LoginFailed"] = true;
                return Redirect("/Registration/SignIn");
            }

            var admin = _context.Admins.SingleOrDefault(u => u.Email == email && u.Password == password);

            if (admin == null)
            {
                TempData["LoginFailed"] = true;
                return Redirect("/Registration/SignIn");
            }

            HttpContext.Session.SetString("username", admin.Email);
            HttpContext.Session.SetString("fullName", admin.FullName);

            return RedirectToAction("Welcome", "Admin", null);
        }

        public async Task<IActionResult> Logout()
        {

            if (HttpContext.Session.GetString("username") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            HttpContext.Session.Remove("username");

            return RedirectToAction("Index", "Home", null);
        }
    }
}
