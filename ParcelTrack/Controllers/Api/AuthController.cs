using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ParcelTrack.Controllers.Api
{
    // Part A: POST /api/auth/login → JWT that lasts 60 minutes and carries the user's roles.
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        public class LoginRequest
        {
            public string Email { get; set; } = "";
            public string Password { get; set; } = "";
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Address that isn't an email at all → 400
            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            {
                return BadRequest(new { message = "Email must be a valid address" });
            }

            IdentityUser? user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                // Same message for wrong password or unknown email
                return Unauthorized(new { message = "Invalid email or password" });
            }

            IList<string> roles = await _userManager.GetRolesAsync(user);

            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? "")
            };
            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            string keyString = _config["Jwt:Key"]!;
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds);

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                expiresIn = 3600,
                roles = roles
            });
        }
    }
}
