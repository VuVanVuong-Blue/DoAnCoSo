using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.DTOs;
using System_Music.Services.Interfaces;
using System_Music.Areas.Admin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace System_Music.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            ViewBag.AllRoles = new List<string> { "None", SD.Role_User, SD.Role_Admin, SD.Role_Artist };
            return View(users);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            
            ViewBag.UserRole = user.Roles.FirstOrDefault() ?? "None";
            ViewBag.AllRoles = new List<string> { "None", SD.Role_User, SD.Role_Admin, SD.Role_Artist };
            return View(user);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            
            ViewBag.UserRole = user.Roles.FirstOrDefault() ?? "None";
            ViewBag.AllRoles = new List<string> { "None", SD.Role_User, SD.Role_Admin, SD.Role_Artist };
            return View(user);
        }

        public IActionResult Create()
        {
            ViewBag.AllRoles = new List<string> { "None", SD.Role_User, SD.Role_Admin, SD.Role_Artist };
            return View(new UserRegisterRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserRegisterRequest request, string selectedRole)
        {
            if (ModelState.IsValid)
            {
                try {
                    await _userService.AddUserAsync(request);
                    var user = await _userService.GetUserByEmailAsync(request.Email);
                    if (user != null && !string.IsNullOrEmpty(selectedRole) && selectedRole != "None")
                    {
                        await _userService.AddToRoleAsync(user.Id, selectedRole);
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            ViewBag.AllRoles = new List<string> { "None", SD.Role_User, SD.Role_Admin, SD.Role_Artist };
            return View(request);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            
            ViewBag.UserRole = user.Roles.FirstOrDefault() ?? "None";
            ViewBag.AllRoles = new List<string> { "None", SD.Role_User, SD.Role_Admin, SD.Role_Artist };
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserDto userDto, string selectedRole)
        {
            if (id != userDto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.UpdateUserAsync(userDto);

                    var currentRoles = await _userService.GetRolesAsync(id);
                    await _userService.RemoveFromRolesAsync(id, currentRoles);

                    if (!string.IsNullOrEmpty(selectedRole) && selectedRole != "None")
                    {
                        await _userService.AddToRoleAsync(id, selectedRole);
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }

            ViewBag.UserRole = selectedRole;
            ViewBag.AllRoles = new List<string> { "None", SD.Role_User, SD.Role_Admin, SD.Role_Artist };
            return View(userDto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> DeleteConfirmed(string id) 
        {
            await _userService.DeleteUserAsync(id); 
            return RedirectToAction(nameof(Index)); 
        }
    }
}
