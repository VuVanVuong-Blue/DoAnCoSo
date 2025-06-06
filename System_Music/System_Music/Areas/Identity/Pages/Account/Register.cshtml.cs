// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System_Music.Areas.Admin.Models;
using System_Music.Models.SqlModels;

namespace System_Music.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            [RegularExpression(@"^(?=.*[A-Z]).*$", ErrorMessage = "Mật khẩu phải có ít nhất một chữ cái viết hoa.")]
            public string Password { get; set; }


            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            [Required]
            [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
            [Display(Name = "Họ và tên")]
            public string Fullname { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "Quốc gia không được vượt quá 100 ký tự.")]
            [Display(Name = "Quốc gia")]
            public string Country { get; set; }

            // Thay Age thành DateOfBirth
            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Ngày sinh")]
            public DateTime DateOfBirth { get; set; }

            public string? Role { get; set; }
            [ValidateNever]
            public IEnumerable<SelectListItem> RoleList { get; set; } 
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!await _roleManager.RoleExistsAsync(SD.Role_User))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_User));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Artist));
            }

            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            Input = new InputModel
            {
                RoleList = new List<SelectListItem>
        {
            new SelectListItem { Text = "Người dùng", Value = SD.Role_User },
            new SelectListItem { Text = "Nghệ sĩ", Value = SD.Role_Artist },
            new SelectListItem { Text = "Quản trị viên", Value = SD.Role_Admin }
        }
            };
        }


        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // Set additional properties
                user.FullName = Input.Fullname;
                user.Country = Input.Country;
                user.DateOfBirth = Input.DateOfBirth;

                _logger.LogInformation($"Đang tạo người dùng: {Input.Email}, Họ tên: {Input.Fullname}, Địa chỉ: {Input.Country}, Ngày sinh: {Input.DateOfBirth}");

                IdentityResult result = await _userManager.CreateAsync(user, Input.Password);
                _logger.LogInformation($"Kết quả tạo người dùng: {result.Succeeded}");
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created successfully.");
                }
                else
                {
                    _logger.LogError("Lỗi khi tạo người dùng:");
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError($"Error: {error.Description}");
                    }
                }

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    if(!string.IsNullOrEmpty(Input.Role))
                    {
                        _logger.LogInformation($"Đang thêm user vào role: {Input.Role}");
                        await _userManager.AddToRoleAsync(user, Input.Role);
                    }
                    else
                    {
                        _logger.LogInformation("Không có role, mặc định thêm vào role: User");
                        await _userManager.AddToRoleAsync(user, SD.Role_User);
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        _logger.LogInformation($"Đang đăng nhập user với email: {Input.Email}");
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("Đăng nhập thành công và chuyển hướng.");
                        return LocalRedirect(returnUrl);
                    }
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogError("ModelState không hợp lệ. Các lỗi:");
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            _logger.LogError($"Key: {state.Key}, Error: {error.ErrorMessage}");
                        }
                    }
                    return Page();
                }

            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private User CreateUser()
        {
            try
            {
                _logger.LogInformation("Đang tạo đối tượng User...");
                return Activator.CreateInstance<User>();
            }
            catch
            {
                _logger.LogError($"Không thể tạo đối tượng User:");
                throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. " +
                    $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
    }
}
