using Entities.Enums;
using Entities.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTOs.User;
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
            //Call the AuthService to register the user

            //Receive the result from AuthService

            //Check if registration was successful

            //If successful. return Ok response with data(AuthResponse)

            //If failed, return BadRequest response with errors
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (!result.Succeeded)
                return BadRequest(result);

            // React will receive: { succeeded: true, data: { accessToken: "...", ... } }
            return Ok(result);
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
