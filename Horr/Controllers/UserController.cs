using Entities.Enums;
using Entities.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using Services.Implementations;
using System.Data;
using System.Linq;
namespace Horr.Controllers
{
    public class UserController : Controller
    {
        private readonly AuthService _authService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserController(AuthService authService, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _authService = authService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            //Call service to add user to db
            var result = await _authService.RegisterAsync(dto);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                return View(dto);
            }
            // On successful registration, sign in the user
            var user = await _userManager.FindByIdAsync(result.Data.Id);
            //This creates the authentication cookie
            await _signInManager.SignInAsync(user, isPersistent: false);

            //Redirect user to corresponding dashboard based on role

            return await RedirectToRoleDashboard(user);
        }


        #region Helper
        private async Task<IActionResult> RedirectToRoleDashboard(User user)
        {
            // Get the list of roles for this specific user
            var roles = await _userManager.GetRolesAsync(user);

            // Check roles and redirect accordingly
            if (roles.Contains(UserRole.Freelancer.ToString()))
            {
                return RedirectToAction("Index", "FreelancerController");
            }
            else if (roles.Contains(UserRole.Client.ToString()))
            {
                return RedirectToAction("Index", "ClientDashboard");
            }

            // Default fallback (e.g., Home Page) if they have no role
            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}
