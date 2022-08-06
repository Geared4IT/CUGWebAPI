using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace cugonlineWebAPI.DTO
{
    public class BusinessProductsDTO
    {
        public long Id { get; set; }
        public long BusinessId { get; set; }
        public string Name { get; set; }
        public string SKUBarcode { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public long CurrentSupplier { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastSyncDate { get; set; }
        public int Unit_id { get; set; }
        public int Category_id { get; set; }
        public float Unit_in_stock { get; set; }
        public decimal Unit_price { get; set; }
        public float Discount_percentage { get; set; }
        public float Reorder_level { get; set; }
        public long User_id { get; set; }
        public bool IsActive { get; set; }
        public string ErrorMessage { get; set; }
        public Image photo { get; set; }
        public string PhotoImageFormat { get; set; }
    }
}