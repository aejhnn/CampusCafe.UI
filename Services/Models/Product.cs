namespace CampusCafe.UI.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Ingredients { get; set; } = string.Empty;
        public bool AvailabilityStatus { get; set; }
    }
}