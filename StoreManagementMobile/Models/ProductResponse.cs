// File: StoreManagementMobile\Models\ProductResponse.cs
using System;

namespace StoreManagementMobile.Models
{
    public class ProductResponse
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public int SupplierId { get; set; }
        
        public string? ProductName { get; set; } 
        public string? Barcode { get; set; }
        public decimal Price { get; set; } 
        public string? Unit { get; set; } 
        public string? ImagePath { get; set; } 
            public string? ImageUrl 
            {
                get => ImagePath;
                set => ImagePath = value;
            }

        public string? Status { get; set; } 
        public DateTime CreatedAt { get; set; }
    }
}