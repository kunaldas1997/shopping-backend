using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using proj1.src.Models;

namespace proj1.src.Services
{
#pragma warning disable
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Product> _products;

        private readonly ProductService _prodService;

        public UserService(IOptions<Connection> shopConnection, ProductService productSerice)
        {
            var mongoDB = new MongoClient(shopConnection.Value.ConnectionString);
            var userDB = mongoDB.GetDatabase(shopConnection.Value.Database);
            _users = userDB.GetCollection<User>(shopConnection.Value.User);
            _products = userDB.GetCollection<Product>(shopConnection.Value.Products);

            _prodService = productSerice;
        }


        public async Task<User> CreateAsync(UserDTO user)
        {
            var userData = new User
            {
                UserName = user.UserName,
                Password = user.Password,
                Email = user.Email
            };
            await _users.InsertOneAsync(userData);
            return userData;
        }

        public async Task<User> GetAsync(string email, string password)
        {
            var _user = await _users.Find(user => user.Email == email &&
            user.Password == password).FirstOrDefaultAsync();
            return _user;
        }

        public async Task<User> GetUserID(string? id)
        {
            var user = await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
            return user;
        }


        public async Task<string> GetAsyncUser(string token){
            var handler = new JwtSecurityTokenHandler();
            var tokenH = handler.ReadJwtToken(token);
            var data = tokenH.Claims.FirstOrDefault(claim => claim.Type =="unique_name")?.Value;
            return data;
        }


        public async Task<User> AddToCart(string? id, string pid)
        {
            var product = await _prodService.GetAsync(pid);
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);

            // Ensure the cart is initialized if it doesn't exist
            var initializeCartIfNull = Builders<User>.Update.Set(u => u.Cart, new Cart { ProductList = [], TotalCost = 0m });
            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            };

            var user = await _users.FindOneAndUpdateAsync(
                filter,
                Builders<User>.Update.Combine(
                    Builders<User>.Update.SetOnInsert(u => u.Cart, new Cart { ProductList = [], TotalCost = 0m })
                ),
                options
            );

            var cartObj = new CartObject
            {
                Product = product,
                Count = 1
            };

            // Check if the product already exists in the cart
            var existingProduct = user.Cart?.ProductList?.FirstOrDefault(o => o.Product?.Id == cartObj.Product?.Id);

            if (existingProduct != null)
            {
                // Increment the count of the existing product
                existingProduct.Count += 1;

                // Update the total cost
                var update = Builders<User>.Update
                    .Set(u => u.Cart.ProductList, user.Cart.ProductList)
                    .Inc(u => u.Cart.TotalCost, cartObj.Product?.ProductPrice ?? 0);

                var updateOptions = new FindOneAndUpdateOptions<User>
                {
                    ReturnDocument = ReturnDocument.After
                };

                var updatedUser = await _users.FindOneAndUpdateAsync(filter, update, updateOptions);
                return updatedUser;
            }
            else
            {
                // Ensure Cart is initialized before attempting to add a new product
                if (user.Cart == null)
                {
                    var initializeCart = Builders<User>.Update.Set(u => u.Cart, new Cart { ProductList = [], TotalCost = 0m });
                    await _users.UpdateOneAsync(filter, initializeCart);
                    user.Cart = new Cart { ProductList = [], TotalCost = 0m };
                }

                // Add new product to the cart
                var update = Builders<User>.Update
                    .AddToSet(u => u.Cart.ProductList, cartObj)
                    .Inc(u => u.Cart.TotalCost, cartObj.Product?.ProductPrice ?? 0);

                var updateOptions = new FindOneAndUpdateOptions<User>
                {
                    ReturnDocument = ReturnDocument.After
                };

                var updatedUser = await _users.FindOneAndUpdateAsync(filter, update, updateOptions);
                return updatedUser;
            }
        }

        public async Task<User> RemoveFromCart(string? id, string pid)
        {
            var product = await _prodService.GetAsync(pid);
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);

            var user = await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
            
            var product_remove = user.Cart.ProductList.FirstOrDefault(o => o.Product?.Id == pid);


            if (product_remove != null)
            {

                if (product_remove.Count > 1)
                {
                    product_remove.Count -= 1;
                    var update = Builders<User>.Update.Set(u => u.Cart.ProductList, user.Cart.ProductList).Inc(u => u.Cart.TotalCost, -product_remove.Product?.ProductPrice);
                    var updateOptions = new FindOneAndUpdateOptions<User>
                    {
                        ReturnDocument = ReturnDocument.After
                    };

                    var updatedUser = await _users.FindOneAndUpdateAsync(filter, update, updateOptions);
                    return updatedUser;
                }
                else
                {
                    var update = Builders<User>.Update
                        .PullFilter(u => u.Cart.ProductList, Builders<CartObject>.Filter.Eq(o => o.Product.Id, pid))
                        .Inc(u => u.Cart.TotalCost, -product_remove.Product?.ProductPrice);

                    var updateOptions = new FindOneAndUpdateOptions<User>
                    {
                        ReturnDocument = ReturnDocument.After
                    };

                    var updatedUser = await _users.FindOneAndUpdateAsync(filter, update, updateOptions);
                    return updatedUser;
                }
            }
            else
            {
                throw new InvalidOperationException("Product not found in the cart.");
            }
        }
    }
#pragma warning restore
}