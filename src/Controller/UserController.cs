using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using proj1.src.Models;
using proj1.src.Services;

namespace proj1.src.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IConfiguration _config;
        private readonly ProductService _product;
        public UserController(UserService userService, IConfiguration configuration, ProductService productService)
        {
            _product = productService;
            _userService = userService;
            _config = configuration;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Create(UserDTO user)
        {
            var existingUser = await _userService.GetAsync(user.Email, user.Password);
            if (existingUser is not null)
            {
                var eemsg = new
                {
                    message = "User Exists",
                    status = 400
                };
                return BadRequest(eemsg);
            }
            await _userService.CreateAsync(user);

            var getNewUser = await _userService.GetAsync(user.Email, user.Password);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(s: _config["AppToken:JWT_Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                        new Claim(ClaimTypes.Name, getNewUser.Id)
                ]),


                Expires = DateTime.UtcNow.AddDays(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string userToken = tokenHandler.WriteToken(token);
            var msg = new
            {
                message = "User Created",
                data = userToken
            };
            return Ok(msg);
        }

        [HttpPost("signin")]
        public async Task<IActionResult> Login(UserDTO user)
        {

            var existingUser = await _userService.GetAsync(user.Email, user.Password);

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
                    Subject = new ClaimsIdentity([
                            new Claim(ClaimTypes.Name, existingUser.Id)
                    ]),


                    Expires = DateTime.UtcNow.AddDays(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
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

        [Authorize]
        [HttpGet("account")]
        public async Task<IActionResult> GetUser()
        {
            var userId = _userService.GetAsyncUser(Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));
            if (string.IsNullOrEmpty(userId.Result))
            {
                var d = new
                {
                    message = "Invalid"
                };
                return Unauthorized(d);
            }

            var user = await _userService.GetUserID(userId.Result);
            if (user is null)
            {
                return Unauthorized("Invalid");
            }
            var data = new
            {
                data = user
            };
            return Ok(data);

        }
        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> ToCart([FromQuery] string pid)
        {
            var userId = _userService.GetAsyncUser(Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));
            if (string.IsNullOrEmpty(userId.Result))
            {
                var d = new
                {
                    message = "Invalid"
                };
                return Unauthorized(d);
            }

            var user = await _userService.GetUserID(userId.Result);
            if (user is null)
            {
                return Unauthorized("Invalid");

            }

            var existingProduct = await _product.GetAsync(pid);
            if (existingProduct is null)
            {
                return NotFound();
            }
            await _userService.AddToCart(user.Id, pid);

            var message = new
            {
                message = "Added to Cart"
            };
            return Ok(message);
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteFromCart([FromQuery] string pid)
        {
            var userId = _userService.GetAsyncUser(Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));
            if (string.IsNullOrEmpty(userId.Result))
            {
                var d = new
                {
                    message = "Invalid"
                };
                return Unauthorized(d);
            }

            var user = await _userService.GetUserID(userId.Result);
            if (user is null)
            {
                var d = new
                {
                    message = "Invalid"
                };
                return Unauthorized(d);

            }
            await _userService.RemoveFromCart(user.Id, pid);


            var message = new
            {
                message = "Deleted From Cart"
            };
            return Ok(message);
        }
    }
}