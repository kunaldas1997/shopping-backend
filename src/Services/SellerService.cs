using Microsoft.Extensions.Options;
using MongoDB.Driver;
using proj1.src.Models;
using System.IdentityModel.Tokens.Jwt;

namespace proj1.src.Services
{
#pragma warning disable CS8603 // Possible null reference return.
    public class SellerService
    {
        private readonly IMongoCollection<Seller> _sellers;
        public SellerService(IOptions<Connection> shopConnection)
        {
            var mongoDB = new MongoClient(shopConnection.Value.ConnectionString);
            var sellerDB = mongoDB.GetDatabase(shopConnection.Value.Database);
            _sellers = sellerDB.GetCollection<Seller>(shopConnection.Value.Seller);
        }


        public async Task<Seller> CreateAsync(SellerDTO seller)
        {
            var sellerData = new Seller{
                Email = seller.Email,
                Password =  seller.Password
            };
            await _sellers.InsertOneAsync(sellerData);
            return sellerData;
        }

        public async Task<Seller> GetAsync(SellerDTO seller)
        {
            var user = await _sellers.Find(s => s.Email == seller.Email && 
            s.Password == seller.Password).FirstOrDefaultAsync();
            return user;
        }

        public async Task<string> GetAsyncUserID(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenH = handler.ReadJwtToken(token);
            var data = tokenH.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;
            
            var user = await _sellers.Find(s => s.Id == data).FirstOrDefaultAsync();
            return user.Id;
        }
    }
#pragma warning restore CS8603 // Possible null reference return.
}