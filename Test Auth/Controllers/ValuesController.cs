using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Test_Auth.Data;
using Test_Auth.Models.Roles;
using Test_Auth.Models.Users;

namespace Test_Auth.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private UserManager<User> _userManager { get; set; }
        private RoleManager<Role> _roleManager { get; set; }
        private readonly ApplicationSettings _appSettings;
        private Context _context { get; set; }
        public ValuesController(UserManager<User> user, RoleManager<Role> role, Context context, IOptions<ApplicationSettings> appSettings)
        {
            _userManager = user;
            _roleManager = role;
            _appSettings = appSettings.Value;
            _context = context;
        }
        [HttpPost("Name")]
        public async Task<IActionResult> CreateRole(string Name)
        {
            Role role = new Role
            {
                Name = Name,
                NormalizedName = Name.ToUpper()
            };
            await _roleManager.CreateAsync(role);
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> CreateClaim(Claims claim)
        {
            await _context.Claims.AddAsync(claim);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> ManageUserClaim(UserClaimModel userClaim)
        {
            Claims claim = await _context.Claims.FindAsync(userClaim.claimId);
            Claim Claim = new Claim(claim.Type, claim.Value);
            User user = await _userManager.FindByIdAsync(userClaim.userId);
            await _userManager.AddClaimAsync(user, Claim);
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser(ApplicationUserModel userModel)
        {
            var applicationUser = new User()
            {
                UserName = userModel.UserName,
                Password = userModel.Password
            };
            var result = await _userManager.CreateAsync(applicationUser, userModel.Password);
            return Ok(result);
        }
        [HttpPost] 
        public async Task<IActionResult> ManageUserRole(UserRoleModel userRole)
        {
            User user = await _userManager.FindByIdAsync(userRole.userId);
            Role role = await _roleManager.FindByIdAsync(userRole.roleId);
            await _userManager.AddToRoleAsync(user, role.Name);
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            var user = await _userManager.FindByNameAsync(loginModel.UserName);
            IList<string> role = await _userManager.GetRolesAsync(user);
            IList<Claim> Claims = await _userManager.GetClaimsAsync(user);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                List<Claim> claims = new List<Claim> {
                    new Claim("UserId", user.Id.ToString()),
                };
                foreach (string r in role)
                {
                    claims.Add(new Claim(ClaimTypes.Role, r));
                };
                foreach (Claim c in Claims)
                {
                    claims.Add(c);
                };
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                return Ok(new { token});
            }
            else
            {
                return BadRequest(new { message = "Username or password is incorrect" });
            }
        }
        [HttpGet]
        [Authorize(Roles = "Student")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetRole()
        {
            ICollection<Role> roles = await _roleManager.Roles.ToListAsync();
            return Ok(roles);
        }
        [HttpGet]
        [Authorize(Policy = "GetClaim")]
        public async Task<IActionResult> GetClaim()
        {
            ICollection<Claims> claims = await _context.Claims.ToListAsync();
            return Ok(claims);
        }
    }
}
