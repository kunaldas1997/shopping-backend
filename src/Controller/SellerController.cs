using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using proj1.src.Models;
using proj1.src.Services;

namespace proj1.src.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class SellerController : ControllerBase
    {
        private readonly SellerService _sellerService;
        private readonly IConfiguration _config;
        public SellerController(SellerService sellerService, IConfiguration configuration)
        {
            _sellerService = sellerService;
            _config = configuration;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Create(SellerDTO seller)
        {
            var existingUser = await _sellerService.GetAsync(seller);
            if (existingUser is not null)
            {
                return BadRequest("Exists");
            }
            await _sellerService.CreateAsync(seller);
            return Ok("User Created");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(SellerDTO seller)
        {

            var existingUser = await _sellerService.GetAsync(seller);

            if (existingUser is null)
            {
                return Unauthorized("Invalid");
            }
            else
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(s: _config["AppToken:JWT_Secret"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.Name, existingUser.Id)
                    ]),
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                string userToken = tokenHandler.WriteToken(token);
                var access = new
                {
                    data = userToken
                };
                return Ok(access);
            }

        }
    }
}