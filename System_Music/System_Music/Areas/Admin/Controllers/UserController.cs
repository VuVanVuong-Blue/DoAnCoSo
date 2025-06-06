using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System_Music.Models.SqlModels;
using System_Music.Services.Interfaces;
using System_Music.Areas.Admin.Models;

namespace System_Music.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;

        public UserController(IUserService userService, UserManager<User> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            var userWithRoles = new List<(User User, string Role)>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                // Lấy vai trò đầu tiên (vì dropdown chỉ cho phép chọn 1 vai trò)
                var role = roles.FirstOrDefault() ?? "None";
                userWithRoles.Add((user, role));
            }
            ViewBag.AllRoles = new List<string> { "None", SD.Role_User, SD.Role_Admin, SD.Role_Artist };
            return View(userWithRoles);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRole = roles.FirstOrDefault() ?? "None";
            ViewBag.AllRoles = new List<string> { "None", SD.Role_User, SD.Role_Admin, SD.Role_Artist };
            return View(user);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRole = roles.FirstOrDefault() ?? "None";
            ViewBag.AllRoles = new List<string> { "None", SD.Role_User, SD.Role_Admin, SD.Role_Artist };
            return View(user);
        }

        public IActionResult Create()
        {
            ViewBag.AllRoles = new List<string> { "None", SD.Role_User, SD.Role_Admin, SD.Role_Artist };
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string Password, string selectedRole)
        {
            if (ModelState.IsValid)
            {
                var result = await _userManager.CreateAsync(user, Password);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(selectedRole) && selectedRole != "None")
                    {
                        await _userManager.AddToRoleAsync(user, selectedRole);
                    }
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            ViewBag.AllRoles = new List<string> { "None", SD.Role_User, SD.Role_Admin, SD.Role_Artist };
            return View(user);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRole = roles.FirstOrDefault() ?? "None";
            ViewBag.AllRoles = new List<string> { "None", SD.Role_User, SD.Role_Admin, SD.Role_Artist };
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, User user, string selectedRole)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            // Lấy người dùng hiện tại từ database để giữ các trường không có trong form
            var existingUser = await _userService.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Chỉ cập nhật các trường được gửi từ form
                    existingUser.UserName = user.UserName;
                    existingUser.Email = user.Email;
                    existingUser.FullName = user.FullName;
                    existingUser.Country = user.Country;
                    // Cập nhật thông tin người dùng
                    await _userService.UpdateUserAsync(existingUser);

                    // Cập nhật vai trò
                    var userToUpdate = await _userManager.FindByIdAsync(id);
                    if (userToUpdate == null)
                    {
                        return NotFound();
                    }

                    var currentRoles = await _userManager.GetRolesAsync(userToUpdate);
                    var removeResult = await _userManager.RemoveFromRolesAsync(userToUpdate, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        foreach (var error in removeResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        throw new Exception("Failed to remove existing roles.");
                    }

                    if (!string.IsNullOrEmpty(selectedRole) && selectedRole != "None")
                    {
                        var addResult = await _userManager.AddToRoleAsync(userToUpdate, selectedRole);
                        if (!addResult.Succeeded)
                        {
                            foreach (var error in addResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            throw new Exception("Failed to add new role.");
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }

            // Nếu ModelState không hợp lệ, truyền lại dữ liệu để hiển thị form
            ViewBag.UserRole = (await _userManager.GetRolesAsync(existingUser)).FirstOrDefault() ?? "None";
            ViewBag.AllRoles = new List<string> { "None", SD.Role_User, SD.Role_Admin, SD.Role_Artist };
            return View(existingUser);
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
