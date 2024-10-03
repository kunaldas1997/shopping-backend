using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using proj1.src.Models;
using proj1.src.Services;

namespace proj1.src.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class Common:ControllerBase
    {

        private readonly ProductService _productService;
        public Common(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<List<Product>>> GetProductBySeller(string id)
        {
            Console.WriteLine("Here");
            List<Product> products = await _productService.GetAsyncBySeller(id);
            if (products is null)
            {
                Console.WriteLine("Nothing Found");
            }

            return Ok(products);
        }
    }
}
