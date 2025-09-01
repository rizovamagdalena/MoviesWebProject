using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesWEB.Models;
using MoviesWEB.Models.System;
using MoviesWEB.Service.Interface;
using System.Security.Claims;

namespace MoviesWEB.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            ViewData["BodyClass"] = "default-bg";
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return View(loginRequest);

            LoginResponse loginResponse = await _userService.CheckUserCredidentals(loginRequest);

            if (loginResponse != null)
            {
                System.Diagnostics.Debug.WriteLine($"Login response: {loginResponse}");
                System.Diagnostics.Debug.WriteLine($"Login response: {loginResponse.ToString()}");
                System.Diagnostics.Debug.WriteLine($"Login response user: {loginResponse.User.ToString()}");


                var user = loginResponse.User;

                var claims = new List<Claim>
                {
                    new Claim("Id", user.Id.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Username),
                    new Claim(ClaimTypes.Name,  user.Name),
                    new Claim(ClaimTypes.Role, user.Role),

                    new Claim("Phone", user.Phone ?? string.Empty)

                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                              new ClaimsPrincipal(claimsIdentity),
                                              authProperties);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View(loginRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name;

            if (!string.IsNullOrEmpty(username))
            {
                await _userService.LogoutUser(username); 
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            ViewData["BodyClass"] = "default-bg";
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            if (!ModelState.IsValid)
                return View(registerRequest);

            bool registerSuccess = await _userService.TryRegisterRequest(registerRequest);

            if (registerSuccess)
            {

                return RedirectToAction("Login", "Account");
            }

            ModelState.AddModelError("", "Registration failed. Please try again.");
            return View(registerSuccess);

        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var nameClaim = User.FindFirst(ClaimTypes.Name);
            var phoneClaim = User.FindFirst("Phone");
            var usernameClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (usernameClaim == null)
                return Unauthorized();

            var profile = new UserProfile
            {
                Name = nameClaim?.Value ?? "Unknown",
                Phone = phoneClaim?.Value ?? "N/A",
                Username = usernameClaim.Value
            };

            return View(profile);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserProfile model)
        {
            if (ModelState.IsValid)
            {
                bool updated = await _userService.UpdateUserProfileAsync(model);

                if (updated)
                {
                    var identity = (ClaimsIdentity)User.Identity;

                    var nameClaim = identity.FindFirst(ClaimTypes.Name);
                    var phoneClaim = identity.FindFirst("Phone");

                    if (nameClaim != null)
                    {
                        identity.RemoveClaim(nameClaim);
                        identity.AddClaim(new Claim(ClaimTypes.Name, model.Name));
                    }

                    if (phoneClaim != null)
                    {
                        identity.RemoveClaim(phoneClaim);
                        identity.AddClaim(new Claim("Phone", model.Phone));
                    }

                    // Refresh the authentication cookie
                    await HttpContext.SignOutAsync();
                    await HttpContext.SignInAsync(new ClaimsPrincipal(identity));


                    return RedirectToAction("MyProfile");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update profile.");
                }
            }

            return View(model);
        }

        public IActionResult MyProfile()
        {
            var nameClaim = User.FindFirst(ClaimTypes.Name);
            var phoneClaim = User.FindFirst("Phone");
            var usernameClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (usernameClaim == null)
                return Unauthorized();

            var profile = new UserProfile
            {
                Name = nameClaim?.Value ?? "Unknown",
                Phone = phoneClaim?.Value ?? "N/A",
                Username = usernameClaim.Value
            };

            return View(profile);
        }
    }

}

