//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace cugonlineWebAPI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class BusinessProduct
    {
        public long Id { get; set; }
        public Nullable<long> BusinessId { get; set; }
        public string Name { get; set; }
        public string SKUBarcode { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public Nullable<long> CurrentSupplier { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<System.DateTime> LastSyncDate { get; set; }
        public Nullable<int> Unit_id { get; set; }
        public Nullable<int> Category_id { get; set; }
        public double Unit_in_stock { get; set; }
        public decimal Unit_price { get; set; }
        public Nullable<double> Discount_percentage { get; set; }
        public Nullable<double> Reorder_level { get; set; }
        public Nullable<long> User_id { get; set; }
        public string ImagePath { get; set; }
    
        public virtual Business Business { get; set; }
    }
}
