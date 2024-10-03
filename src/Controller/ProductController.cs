using Microsoft.AspNetCore.Mvc;
using proj1.src.Services;
using proj1.src.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Net.Http.Headers;
namespace proj1.src.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly SellerService _sellerService;
        public ProductController(ProductService productService, SellerService sellerService)
        {
            _productService = productService;
            _sellerService = sellerService;
        }

        [HttpGet]
        public async Task<List<Product>> Get()
        {
            return await _productService.GetAsync();
        }

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Product>> GetProduct(string id)
        {
            var product = await _productService.GetAsync(id);
            if (product is null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpGet("{category}")]
        public async Task<IActionResult> GetProductOfCategory(string category)
        {
            var products = await _productService.GetAsyncWithCat(category);
            if (products is null)
            {
                var data = new
                {
                    message = $"No Category of type {category} exists."
                };

                return NotFound(data);
            }

            return Ok(products);
        }

        [HttpGet("price-range")]
        public async Task<IActionResult> GetProductsInRange([FromQuery] decimal range_min, [FromQuery] decimal range_max)
        {
            var products = await _productService.GetBetweenPriceRange(range_min, range_max);

            if (products is null)
            {
                var data = new
                {
                    message = $"No Products found between {range_min} and {range_max}."
                };

                return NotFound(data);
            }

            return Ok(products);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post(Product product)
        {

            var userId = _sellerService.GetAsyncUserID(Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));
            if (string.IsNullOrEmpty(userId.Result))
            {
                return Unauthorized("User ID could not be retrieved.");
            }


            var fileURLS = new List<string>();
            var sanitizeField = Path.GetInvalidFileNameChars().Aggregate(product.ProductName, (current, c) => current.Replace(" ", "_"));
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", sanitizeField);


            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            foreach (var filePath in product.ProductImg)
            {
                // Validate the file path

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    continue;
                }

                try
                {

                    var sourceFilePath = Path.GetFullPath(filePath);
                    var destinationFilePath = Path.Combine(basePath, Path.GetFileName(filePath));



                    if (!System.IO.File.Exists(sourceFilePath))
                    {
                        Console.WriteLine($"Source file not found: {sourceFilePath}");
                        continue;
                    }


                    System.IO.File.Copy(sourceFilePath, destinationFilePath, overwrite: true);


                    var fileUrl = $"/uploads/{sanitizeField}/{Path.GetFileName(filePath)}";
                    fileURLS.Add(fileUrl);
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Error copying file from {filePath}: {ex.Message}");
                }
            }


            product.SellerID = userId.Result.ToString();
            product.ProductImg = fileURLS.ToArray();
            await _productService.CreateAsync(product);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }

        [Authorize]
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Put(string id, Product product)
        {
            var userId = _sellerService.GetAsyncUserID(Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));
            if (string.IsNullOrEmpty(userId.Result))
            {
                return Unauthorized("User ID could not be retrieved.");
            }
            else
            {
                var _product = await _productService.GetAsync(id);
                if (_product is null)
                {
                    return NotFound();
                }


                if (_product.SellerID == userId.Result)
                {
                    product.Id = _product.Id;
                    await _productService.UpdateAsync(id, product);
                    var response = new
                    {
                        message = "Updated"
                    };
                    return Ok(response);
                }
            }
            return BadRequest();
        }

        [Authorize]
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = _sellerService.GetAsyncUserID(Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));
            if (string.IsNullOrEmpty(userId.Result))
            {
                return Unauthorized("User ID could not be retrieved.");
            }

            else
            {
                var product = await _productService.GetAsync(id);
                if (product is null)
                {
                    return NotFound();
                }

                if (product.SellerID == userId.Result)
                {

                    var sanitizedFolderName = Path.GetInvalidFileNameChars().Aggregate(product.ProductName, (current, c) => current.Replace(" ", "_"));

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", sanitizedFolderName);

                    await _productService.DeleteAsync(id);
                    if (Directory.Exists(filePath))
                    {
                        Directory.Delete(filePath);
                    }

                    var data = new
                    {
                        data = "Delete Success"
                    };
                    return Ok(data);
                }
            }

            return BadRequest();
        }


    }
}
