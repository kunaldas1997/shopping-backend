using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using proj1.src.Services;

namespace proj1.src.Controller
{

    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;

        public OrderController(OrderService orderService, IConfiguration configuration, UserService userService)
        {
            _orderService = orderService;
            _configuration = configuration;
            _userService = userService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create()
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
            var userData = await _userService.GetUserID(userId.Result);
            var orderData = await _orderService.CreateOrder(userData);

            var message = new
            {
                message = $"Your Order ID is {orderData}"
            };
            return Ok(message);
        }

        [HttpGet("get-order")]
        public async Task<IActionResult> GetOrderStatus()
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
            var userData = await _userService.GetUserID(userId.Result);
            var orderData = await _orderService.GetOrderDetails(userData.Id);

       
            if (orderData.status == 201)
            {
                return Ok(orderData);
            }
            else
            {
                return BadRequest(orderData);

            }
        }
    }
}
