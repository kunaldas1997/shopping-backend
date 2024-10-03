using System.Text;


namespace proj1.src.Services
{
    public class OrderIDGenerator
    {
        private static readonly Random random = new();

        public static string GenerateOrderId()
        {
            string first_three = GenerateOrderIdString(3);
            string second_part = GenerateOrderIdString(16);

            return $"{first_three}-{second_part}";
        }

        private static string GenerateOrderIdString(int n)
        {
            StringBuilder result = new StringBuilder(n);
            for (int i = 0; i < n; i++)
            {

                result.Append(random.Next(10));

            }

            return result.ToString();

        }
    }
}
