using Binder_Cart.Dtos;
using Binder_Cart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Binder_Cart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationController> _logger;
        public AuthenticationController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, 
            IConfiguration configuration,
            ILogger<AuthenticationController> logger)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        [Route("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
            {
                _logger.LogError("User  already exists, Role - User, User : {@user}", model.UserName);
                //return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists" });
                return Ok(new ResponseDto { StatusVal = "Error", Message = "User already exists" });
            }

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("User creation failed, Role - User, User : {@user}", user);
                //return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed" });
                return Ok(new ResponseDto { StatusVal = "Error", Message = "User creation failed" });
            }

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));  
            

            if (await _roleManager.RoleExistsAsync(UserRoles.User))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }
            _logger.LogInformation("User Registered successfully, Role - User, User : {@user}", user);

            return Ok(new ResponseDto { StatusVal = "Success", Message = "User created successfully." });
        }

        [HttpPost]
        [Route("RegisterAdmin")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDto model)
        {
            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
            {
                _logger.LogError("User  already exists, Role - Admin, User : {@user}", model.UserName);
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto { StatusVal = "Error", Message = "User already exists" });
            }

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("User creation failed, Role - Admin, User : {@user}", user);
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto { StatusVal = "Error", Message = "User creation failed" });
            }

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            }

            _logger.LogInformation("User Registered successfully, Role - Admin, User : {@user}" , user);
            return Ok(new ResponseDto { StatusVal = "Success", Message = "User created successfully." });
        }

        [HttpPost]
        [Route("Login")] 
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim> {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                };
                foreach (var userrole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userrole));
                }
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidateAudience"],
                    expires: DateTime.Now.AddHours(5),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                Log.Information("Login successfully {@user}", user);
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                });
                //return Ok(new Response { Status = "Success", Message = "Login successfully." });
            }
            Log.Information("Login Unauthorized");
            return Unauthorized();
        }

        [HttpPost]
        [Route("Token")]
        public async Task<IActionResult> Token(string userName, string password)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null && await _userManager.CheckPasswordAsync(user, password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim> {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                };
                foreach (var userrole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userrole));
                }
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidateAudience"],
                    expires: DateTime.Now.AddHours(5),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                });
                //return Ok(new Response { Status = "Success", Message = "Login successfully." });
            }
            return Unauthorized();
        }
    }
}
