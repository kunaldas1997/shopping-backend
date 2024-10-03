using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;




namespace proj1.src.Models
{
    // Single Product Info for Product-Page
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [BsonElement("product_name")]
        [JsonPropertyName("product_name")]
        public required string ProductName { get; set; }

        [BsonElement("description")]
        [JsonPropertyName("description")]
        public required string Description { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        [BsonElement("product_price")]
        [JsonPropertyName("product_price")]
        public required decimal ProductPrice { get; set; }

        [BsonElement("product_img")]
        [JsonPropertyName("product_img")]
        public required string[] ProductImg { get; set; }

        [BsonElement("seller_id")]
        [JsonPropertyName("seller_id")]
        public string? SellerID { get; set; }

        [BsonElement("category")]
        [JsonPropertyName("category")]
        public required string Category { get; set; }

    }

    // Multiple Product Page / Home / Leaving it here for now
    public class ProductPage
    {
        [BsonElement("product_list")]

        public required Product[] ProductList { get; set; }
    }


    // Seller Profile


    public class SellerDTO
    {
        [BsonElement("email")]
        [JsonPropertyName("email")]
        public required string Email { get; set; }

        [BsonElement("password")]
        [JsonPropertyName("password")]
        public required string Password { get; set; }

    }
    public class Seller
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [BsonElement("email")]
        [JsonPropertyName("email")]
        public required string Email { get; set; }

        [BsonElement("password")]
        [JsonPropertyName("password")]
        public required string Password { get; set; }

        [BsonElement("user_name")]
        [JsonPropertyName("user_name")]
        public string? UserName { get; set; }

        [BsonElement("type")]
        [JsonPropertyName("type")]
        public string? Type { get; set; }

    }

    // User Profile

    public class UserDTO
    {
        [BsonElement("email")]
        [JsonPropertyName("email")]
        public required string Email { get; set; }

        [BsonElement("password")]
        [JsonPropertyName("password")]
        public required string Password { get; set; }

        [BsonElement("user_name")]
        [JsonPropertyName("user_name")]
        public  string? UserName { get; set; }
    }

    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [BsonElement("email")]
        [JsonPropertyName("email")]
        public required string Email { get; set; }

        [BsonElement("password")]
        [JsonPropertyName("password")]
        public required string Password { get; set; }

        [BsonElement("user_name")]
        [JsonPropertyName("user_name")]
        public string? UserName { get; set; }

        [BsonElement("cart")]
        [JsonPropertyName("cart")]
        public Cart? Cart { get; set; }

        [BsonElement("type")]
        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }


    // Cart

    public class CartObject
    {
        [BsonElement("product")]
        [JsonPropertyName("product")]
        public Product? Product { get; set; }
        
        [BsonElement("count")]
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
    public class Cart
    {
        [BsonElement("product_list")]
        [JsonPropertyName("product_list")]
        public CartObject[]? ProductList { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        [BsonElement("total_cost")]
        [JsonPropertyName("total_cost")]
        public decimal? TotalCost { get; set; }


    }


    // User Token
    public class TokenDB
    {
        [BsonElement("user_id")]
        
        public required string UserId { get; set; }

        [BsonElement("token")]
        public required string Token { get; set; }

    }

    // Order

    public class ProductDets
    {
        [BsonElement("product_id")]
        [JsonPropertyName("product_id")]
        public required string Product_Id { get; set; }

        [BsonElement("count")]
        [JsonPropertyName("count")]
        public required int Count { get; set; }
    }

    public class OrderDB
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [BsonElement("reference_id")]
        [JsonPropertyName("reference_id")]
        public required string OrderRef { get; set; }


        [BsonElement("user_id")]
        [JsonPropertyName("user_id")]
        public required string UserId { get; set; }

        [BsonElement("seller_list")]
        [JsonPropertyName("seller_list")]
        public required string[] Seller_List { get; set; }

        [BsonElement("ordered_items")]
        [JsonPropertyName("ordered_items")]
        public required List<ProductDets> Product_List { get; set; }

        [BsonElement("status")]
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        [BsonElement("total_cost")]
        [JsonPropertyName("total_cost")]
        public decimal? TotalCost { get; set; }

    }

    public class OrderData
    {

        [BsonElement("reference_id")]
        [JsonPropertyName("reference_id")]
        public required string OrderRef { get; set; }

        [BsonElement("ordered_items")]
        [JsonPropertyName("ordered_items")]
        public required List<MiniProdName> Product_List { get; set; }

        [BsonElement("status")]
        [JsonPropertyName("status")]
        public required string Status { get; set; }
    }

    public class MiniProdName
    {
        [BsonElement("product_name")]
        [JsonPropertyName("product_name")]
        public required string ProductName { get; set; }

        [BsonElement("product_count")]
        [JsonPropertyName("product_count")]
        public required int Count { get; set; }

    }
}
