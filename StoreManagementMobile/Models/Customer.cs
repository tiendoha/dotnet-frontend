using System.Text.Json.Serialization;

namespace StoreManagementMobile.Models
{
    public class Customer
    {
        [JsonPropertyName("customerId")]
        public int CustomerId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("address")]
        public string Address { get; set; } = "";
    }
}
