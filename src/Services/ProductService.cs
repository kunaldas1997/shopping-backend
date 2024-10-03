using Microsoft.Extensions.Options;
using MongoDB.Driver;
using proj1.src.Models;
namespace proj1.src.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _productCollection;
        public ProductService(IOptions<Connection> shopConnection)
        {
            var mongoDB = new MongoClient(shopConnection.Value.ConnectionString);
            var productDB = mongoDB.GetDatabase(shopConnection.Value.Database);
            _productCollection = productDB.GetCollection<Product>(shopConnection.Value.Products);

        }

        public async Task<List<Product>> GetAsync()
        {
            return await _productCollection.Find(_ => true).ToListAsync();
        }

        public async Task<List<Product>> GetAsyncWithCat(string category)
        {
            return await _productCollection.Find(x => x.Category == category).ToListAsync();
        }
        public async Task<Product?> GetAsync(string id)
        {
            var product = await _productCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            return product;
        }

        public async Task<List<Product>> GetAsyncBySeller(string userId)
        {
            return await _productCollection.Find(s => s.SellerID == userId).ToListAsync();
        }

        public async Task<List<Product>> GetBetweenPriceRange(decimal range_min, decimal range_max)
        {
            return await _productCollection.Find(s => (s.ProductPrice >= range_min) && (s.ProductPrice <= range_max)).ToListAsync();
        }

        // Following are callable by seller only.
        public async Task CreateAsync(Product product) => await _productCollection.InsertOneAsync(product);
        public async Task UpdateAsync(string id, Product product) => await _productCollection.ReplaceOneAsync(x => x.Id == id, product);
        public async Task DeleteAsync(string id) => await _productCollection.DeleteOneAsync(x => x.Id == id);
    }

}
