using Microsoft.Extensions.Options;
using MongoDB.Driver;
using proj1.src.Models;
using System.IdentityModel.Tokens.Jwt;
using proj1.src.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Google.Apis.Storage.v1.Data;

namespace proj1.src.Services
{
    public class OrderService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Product> _products;
        private readonly IMongoCollection<Seller> _seller;
        private readonly IMongoCollection<OrderDB> _order;

        private readonly ProductService _prodService;
        private readonly UserService _userService;

        public OrderService(IOptions<Connection> shopConnection, ProductService productService, UserService userService)
        {
            var mongoDB = new MongoClient(shopConnection.Value.ConnectionString);
            var userDB = mongoDB.GetDatabase(shopConnection.Value.Database);

            _users = userDB.GetCollection<User>(shopConnection.Value.User);
            _order = userDB.GetCollection<OrderDB>(shopConnection.Value.Order);

            _prodService = productService;
            _userService = userService;
        }




        public async Task<string> CreateOrder(User userData)
        {
            string orderId = OrderIDGenerator.GenerateOrderId();

            var id = userData?.Id;

            var cart = new List<ProductDets>();
            var seller = new List<string>();
            var priceSum = userData.Cart.TotalCost;
            if (userData?.Cart?.ProductList != null)
            {
                foreach (var product in userData.Cart.ProductList)
                {
                    if (product.Product != null)
                    {
                        ProductDets tmp = new()
                        {
                            Product_Id = product.Product.Id,
                            Count = product.Count
                        };

                        var sellerTmp = product.Product?.SellerID;

                        if (!seller.Contains(sellerTmp))
                        {
                            seller.Add(sellerTmp);
                        }
                        cart.Add(tmp);
                        for (int i = 1; i <= product.Count; ++i)
                        {
                            
                            await _userService.RemoveFromCart(id, product.Product.Id);
                        }
                    }
                }
            }


            OrderDB new_order = new()
            {
                OrderRef = orderId,
                UserId = id,
                Seller_List = [.. seller],
                Product_List = cart,
                TotalCost = priceSum,
                Status = "Pending"
            };

                                                    
            await _order.InsertOneAsync(new_order);
            return new_order.OrderRef;
        }


        public async Task<dynamic> GetOrderDetails(string uid)
        {
            var getOrderDetails = await _order.Find(order => order.UserId == uid).ToListAsync();
      
            if (getOrderDetails is null || !getOrderDetails.Any())
            {
                var msgStat = new
                {
                    status = 404,
                    message = "No Such Orders exist"
                };
                return msgStat;
            }

            var o_order = new List<OrderData>();

            foreach (var orderDetail in getOrderDetails)
            {
                var prod_list = new List<MiniProdName>();

                foreach (var productEntry in orderDetail.Product_List)
                {
                    var product = await _prodService.GetAsync(productEntry.Product_Id);
                    if (product is null)
                    {
                        var msg = new
                        {
                            status = 404,
                            message = "Product does not exist"
                        };
                        return msg;
                    }
                    else
                    {
                        MiniProdName p = new()
                        {
                            ProductName = product.ProductName,
                            Count = productEntry.Count
                        };
                        prod_list.Add(p);
                    }


                }

                o_order.Add(new OrderData()
                {
                    OrderRef = orderDetail.OrderRef,
                    Product_List = prod_list,
                    Status = orderDetail.Status
                });
            }

            var msgStatFinal = new
            {
                status = 201,
                message = o_order
            };

            return msgStatFinal;
        }
    }
}