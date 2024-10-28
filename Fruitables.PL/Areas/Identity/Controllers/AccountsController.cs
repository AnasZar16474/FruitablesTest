using Fruitables.DAL.Data;
using Fruitables.DAL.Models;
using Fruitables.PL.Areas.Identity.Models.ViewModels;
using Fruitables.PL.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace Fruitables.PL.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountsController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AccountsController(UserManager<ApplicationUser> userManager, ApplicationDbContext Context, SignInManager<ApplicationUser> signInManager,
          RoleManager<IdentityRole> roleManager) {
            this.userManager = userManager;
            context = Context;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
        }
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ApplicationUser user = new ApplicationUser()
            {
                Gender = model.Gender,
                Address = model.Address,
                UserName = model.Name,
                Email = model.Email,
                PhoneNumber = model.Phone,
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmEmail = Url.Action("ConfirmEmail", "Accounts", new { userId = user.Id, Token = token }, protocol: HttpContext.Request.Scheme);
                var email = new DAL.Models.Email()
                {
                    Subject = "confirmedEmail",
                    Body = confirmEmail,
                    Recivers = user.Email,
                };
                EmailSetting.SendEmail(email);
                await userManager.AddToRoleAsync(user, "User");
                var cart = new Cart
                {
                    ApplicationUserId = user.Id,
                    CreatedAt = DateTime.Now
                };
                context.Carts.Add(cart);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(LogIn));
            }
            return View(model);
        }
        public async Task<IActionResult> ConfirmEmail(string Token, string userId)
        {
            if (Token is null || userId is null)
            {
                return NotFound();
            }
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return NotFound();
            }
            var result = await userManager.ConfirmEmailAsync(user, Token);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(LogIn));
            }
            return NotFound();
        }
        [AllowAnonymous]
        public IActionResult LogIn(string returnUrl = null)
        {
            var model = new LogInWithGoogleVM
            {
                ReturnUrl = returnUrl ?? Url.Content("~/")
            };
            var modelA = new CompositLogInVM
            {
                LogInWithGoogle = model,
                LogIn = new LogInViewModel() // تأكد من تهيئة LogIn هنا

            };

            return View(modelA);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult LogInA(string returnUrl = null)
        {            
                var modelA = new LogInWithGoogleVM
                {
                    ReturnUrl= returnUrl ?? Url.Content("~/")
                };
            var model = new CompositLogInVM
            {
                LogInWithGoogle = modelA,
                LogIn = new LogInViewModel()
            };

            return View(model);
        }

            [AllowAnonymous]
            [HttpPost]
            public async Task<IActionResult> LogIn(CompositLogInVM model)
            {
                var user = await userManager.FindByEmailAsync(model.LogIn.Email);
                if (user is null)
                {
                    return NotFound();
                }
                var result = await signInManager.PasswordSignInAsync(user.UserName, model.LogIn.Password, model.LogIn.RememberMe, true);
                if (result.Succeeded)
                {
                    if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
                    {
                        return RedirectToAction("create", "Products", new { area = "Dashboard" });
                    }
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
                return View(model);
            }
            [HttpPost]
            [AllowAnonymous]
            public IActionResult ExternalLogin(string provider, string returnUrl = null)
            {
                var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Accounts", new { ReturnUrl = returnUrl });
                var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                return Challenge(properties, provider);
            }

            [HttpGet]
            [AllowAnonymous]
            public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
            {
                returnUrl = returnUrl ?? Url.Content("~/");
                if (remoteError != null)
                {
                    ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                    return View("Login");
                }
                var info = await signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return RedirectToAction(nameof(Login));
                }
                var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
                if (result.Succeeded)
                {
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                    if (email != null)
                    {
                        // التحقق مما إذا كان البريد الإلكتروني موجود بالفعل
                        var existingUser = await userManager.FindByEmailAsync(email);
                        if (existingUser != null)
                        {
                            await signInManager.SignInAsync(existingUser, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }

                        // المتابعة لإنشاء مستخدم جديد إذا لم يكن البريد الإلكتروني موجودًا
                        var user = new ApplicationUser { UserName = email, Email = email };
                        var createResult = await userManager.CreateAsync(user);
                        if (createResult.Succeeded)
                        {
                            var cart = new Cart
                            {
                                ApplicationUserId = user.Id,
                                CreatedAt = DateTime.Now
                            };
                            context.Carts.Add(cart);
                            await context.SaveChangesAsync();
                            await userManager.AddLoginAsync(user, info);
                            await signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            foreach (var error in createResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                       
                        }
                        var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                        var profilePicture = info.Principal.FindFirstValue("picture");
                        if (user == null)
                        {
                            user = new ApplicationUser { UserName = firstName, Email = email };
                            var createResultA = await userManager.CreateAsync(user);
                            if (createResultA.Succeeded)
                            {
                                var cart = new Cart
                                {
                                    ApplicationUserId = user.Id,
                                    CreatedAt = DateTime.Now
                                };
                                context.Carts.Add(cart);
                                await context.SaveChangesAsync();
                                await userManager.AddLoginAsync(user, info);
                                await signInManager.SignInAsync(user, isPersistent: false);
                                return LocalRedirect(returnUrl);
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Failed to create user account.");
                                return View("Login");
                            }
                        }
                        await userManager.AddLoginAsync(user, info);
                        await signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                    ModelState.AddModelError(string.Empty, "Email not available from external provider.");
                    return View("Login");
                }
            }
            public IActionResult ForgotPassword()
            {
                return View();
            }
            [HttpPost]
            public async Task<IActionResult> SendResetPasswordURL(ForgotPasswordVM VM)
            {
                if (ModelState.IsValid)
                {

                    var user = await userManager.FindByEmailAsync(VM.Email);
                    if (user is not null)
                    {
                        var token = await userManager.GeneratePasswordResetTokenAsync(user);
                        var resetPasswordUrl = Url.Action("ResetPassword", "Accounts", new { email = VM.Email, token = token }, protocol: HttpContext.Request.Scheme);
                        var email = new DAL.Models.Email()
                        {
                            Subject = "ResetPassword",
                            Recivers = VM.Email,
                            Body = resetPasswordUrl,
                        };
                        EmailSetting.SendEmail(email);
                    }
                    return Content("checkYourEmail");
                }
                return NotFound();

            }
            public IActionResult ResetPassword(string Email, string Token)
            {
                return View();
            }
            [HttpPost]
            public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user is not null)
                {
                    var result = await userManager.ResetPasswordAsync(user, model.Token, model.password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(LogIn));
                    }
                }
                return View(model);
            }
            public IActionResult UsersView()
            {
                var users = userManager.Users.ToList();
                var DisplayUsers = users.Select(user => new DisplayUsersViewModel()
                {
                    City = user.Address,
                    Gender = user.Gender ?? "aaaaaaaa",
                    Name = user.UserName,
                    RoleName = userManager.GetRolesAsync(user).Result
                });

                return View(DisplayUsers);
            }
            [Authorize(Roles = "Admin,SuperAdmin")]
            public IActionResult CreateRole()
            {
                return View();
            }
            [HttpPost]
            [Authorize(Roles = "Admin,SuperAdmin")]
            public async Task<IActionResult> CreateRole(RoleViewModel model)
            {
                IdentityRole Role = new IdentityRole()
                {
                    Name = model.RoleName,
                };
                var Result = await roleManager.CreateAsync(Role);
                if (Result.Succeeded)
                {
                    return RedirectToAction(nameof(RoleView));
                }

                return View(model);
            }

            public IActionResult RoleView()
            {
                var Roles = roleManager.Roles.ToList();
                var DisplayRoles = Roles.Select(aa => new RoleViewModel()
                {
                    RoleName = aa.Name
                }).ToList();
                return View(DisplayRoles);
            }
            [Authorize(Roles = "Admin,SuperAdmin")]
            public async Task<IActionResult> DeleteRole(string Id)
            {
                var role = await roleManager.FindByNameAsync(Id);
                var Results = await roleManager.DeleteAsync(role);
                if (Results.Succeeded)
                {
                    return RedirectToAction(nameof(RoleView));
                }
                return View();

            }
            [Authorize(Roles = "Admin,SuperAdmin")]
            public async Task<IActionResult> DeleteUser(string Id)
            {
                var user = await userManager.FindByNameAsync(Id);
                var Results = await userManager.DeleteAsync(user);
                if (Results.Succeeded)
                {
                    return RedirectToAction(nameof(UsersView));
                }
                return View();
            }
            [Authorize(Roles = "Admin,SuperAdmin")]
            public async Task<IActionResult> UpdateUser(string Id)
            {
                var user = await userManager.FindByNameAsync(Id);

                if (user != null)
                {
                    DisplayUsersViewModel aa = new DisplayUsersViewModel()
                    {
                        Name = user.UserName ?? "Unknow",
                        Gender = user.Gender ?? "Unknow",
                        City = user.Address ?? "Unknow",
                        id = user.Id,
                    };
                    return View(aa);

                }
                return Content("User Is Null");

            }
            [HttpPost]
            [Authorize(Roles = "Admin,SuperAdmin")]
            public async Task<IActionResult> UpdateUser(DisplayUsersViewModel model)
            {
                var user = await userManager.FindByIdAsync(model.id);

                user.Gender = model.Gender;
                user.Address = model.City;
                user.UserName = model.Name;

                var result = await userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(UsersView));
                }

                return View(model);
            }
            [Authorize(Roles = "Admin,SuperAdmin")]
            public async Task<IActionResult> ChangeRoles(string Id)
            {
                EditRolesViewModel aa = new EditRolesViewModel()
                {
                    Id = Id,
                    RolesList = roleManager.Roles.Select(
                        role => new SelectListItem
                        {
                            Value = role.Id,
                            Text = role.Name,
                        }
                        ).ToList()
                };

                return View(aa);
            }
            [HttpPost]
            public async Task<IActionResult> ChangeRoles(EditRolesViewModel model)
            {
                var user = await userManager.FindByNameAsync(model.Id);
                var roleCurrent = await userManager.GetRolesAsync(user);
                var Results = await userManager.RemoveFromRolesAsync(user, roleCurrent);
                var role = await roleManager.FindByIdAsync(model.selectedRoles);
                var roleNew = await userManager.AddToRoleAsync(user, role.Name);

                return RedirectToAction(nameof(UsersView));
            }
            public async Task<IActionResult> LogOut()
            {
                await signInManager.SignOutAsync();
                return RedirectToAction(nameof(LogIn));
            }
        }
    }
