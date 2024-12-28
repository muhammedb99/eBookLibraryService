using System.Net.Mail;
using eBookLibraryService.Helpers;
using eBookLibraryService.Models;
using eBookLibraryService.Services;
using eBookLibraryService.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace eBookLibraryService.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> _signInManager;
        private readonly UserManager<Users> _userManager;
        private readonly NotificationService _notificationService;

        public AccountController(
            SignInManager<Users> signInManager,
            UserManager<Users> userManager,
            NotificationService notificationService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var cart = HttpContext.Session.GetObject<Cart>("Cart");
            ViewBag.CartItemCount = cart?.Items.Count ?? 0;
            base.OnActionExecuting(context);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Email or password is incorrect.");
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new Users
                {
                    FullName = model.Name,
                    Email = model.Email,
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Send Welcome Email
                    var emailMessage = $@"<h1>Welcome to eBookLibraryService!</h1>
                                        <p>Dear {model.Name},</p>
                                        <p>Thank you for signing up for eBookLibraryService. We’re excited to have you on board!</p>
                                        <p>Happy reading,</p>
                                        <p>The eBookLibraryService Team</p>";

                    await _notificationService.SendEmailAsync(
                        recipientEmail: model.Email,
                        subject: "Welcome to eBookLibraryService!",
                        message: emailMessage
                    );

                    TempData["SuccessMessage"] = "Registration successful! A welcome email has been sent.";
                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Email not found!");
                }
                else
                {
                    // Generate a random 6-digit verification code
                    var verificationCode = new Random().Next(100000, 999999).ToString();

                    // Store the code in TempData (or database for persistence)
                    TempData["VerificationCode"] = verificationCode;
                    TempData["Email"] = model.Email;

                    // Send the code to the user's email
                    await SendVerificationCodeEmailAsync(model.Email, verificationCode);

                    TempData["Message"] = "Verification code has been sent to your email. Please check your inbox.";
                    return RedirectToAction("EnterVerificationCode");
                }
            }
            return View(model);
        }


        private async Task SendVerificationCodeEmailAsync(string email, string verificationCode)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential("ebookstorenoty@gmail.com", "fzntvkrsxivqftay"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("ebookstorenoty@gmail.com", "eBook Library"),
                Subject = "Your Verification Code",
                Body = $"<p>Your verification code is:</p><h2>{verificationCode}</h2><p>Please use this code to verify your email and access the password reset page.</p>",
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Log or handle email sending exceptions
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }


        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid email confirmation request.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Verify the token
            var result = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, "ResetPassword", token);
            if (result)
            {
                // Redirect to Change Password page
                return RedirectToAction("ChangePassword", new { username = user.UserName });
            }

            return BadRequest("Invalid or expired token.");
        }


        [HttpGet]
        public IActionResult ChangePassword(string email)
        {
            var model = new ChangePasswordViewModel { Email = email };
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user != null)
                {
                    var result = await _userManager.RemovePasswordAsync(user);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddPasswordAsync(user, model.NewPassword);
                        if (result.Succeeded)
                        {
                            TempData["Message"] = "Password updated successfully!";
                            return RedirectToAction("Login", "Account");
                        }
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Email not found!");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (!string.IsNullOrEmpty(model.FullName))
            {
                user.FullName = model.FullName;
            }

            if (!string.IsNullOrEmpty(model.Email))
            {
                var emailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!emailResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View("Profile", model);
                }
            }

            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                var phoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!phoneResult.Succeeded)
                {
                    foreach (var error in phoneResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View("Profile", model);
                }
            }

            if (!string.IsNullOrEmpty(model.Password))
            {
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    ModelState.AddModelError("", "Could not remove old password.");
                    return View("Profile", model);
                }

                var addPasswordResult = await _userManager.AddPasswordAsync(user, model.Password);
                if (!addPasswordResult.Succeeded)
                {
                    foreach (var error in addPasswordResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View("Profile", model);
                }
            }

            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateField(string fieldName, string value)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            IdentityResult result = null;
            switch (fieldName)
            {
                case "FullName":
                    user.FullName = value;
                    result = await _userManager.UpdateAsync(user);
                    break;
                case "Email":
                    result = await _userManager.SetEmailAsync(user, value);
                    break;
                case "PhoneNumber":
                    result = await _userManager.SetPhoneNumberAsync(user, value);
                    break;
                default:
                    TempData["Message"] = "Invalid field name.";
                    return RedirectToAction("Profile");
            }

            if (result != null && !result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            TempData["Message"] = "Field updated successfully!";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                TempData["Message"] = "Passwords do not match or are empty.";
                return RedirectToAction("Profile");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
            {
                TempData["Message"] = "Could not remove old password.";
                return RedirectToAction("Profile");
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, newPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {
                    TempData["Message"] += error.Description + " ";
                }
                return RedirectToAction("Profile");
            }

            TempData["Message"] = "Password updated successfully!";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var users = _userManager.Users.ToList();
            var userViewModels = users.Select(user => new ManageUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = _userManager.GetRolesAsync(user).Result.FirstOrDefault() ?? "User"
            }).ToList();

            return View(userViewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Message"] = "Invalid user ID.";
                return RedirectToAction("ManageUsers");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Message"] = "User not found.";
                return RedirectToAction("ManageUsers");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Message"] = "User deleted successfully.";
            }
            else
            {
                TempData["Message"] = "Failed to delete user.";
            }

            return RedirectToAction("ManageUsers");
        }


        [HttpGet]
        public IActionResult EnterVerificationCode()
        {
            return View();
        }
        [HttpPost]
        public IActionResult EnterVerificationCode(string code)
        {
            var storedCode = TempData["VerificationCode"]?.ToString();
            var email = TempData["Email"]?.ToString();

            if (string.IsNullOrEmpty(storedCode) || string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Verification code expired. Please try again.");
                return RedirectToAction("VerifyEmail");
            }

            if (code == storedCode)
            {
                // Clear TempData after successful verification
                TempData.Remove("VerificationCode");
                TempData.Remove("Email");

                // Redirect to Change Password
                return RedirectToAction("ChangePassword", new { email = email });
            }

            // Add error message if the code is incorrect
            ModelState.AddModelError("", "The verification code you entered is incorrect. Please try again.");
            TempData.Keep("VerificationCode"); // Keep the verification code for the next attempt
            TempData.Keep("Email"); // Keep the email for the next attempt
            return View();
        }



    }
}