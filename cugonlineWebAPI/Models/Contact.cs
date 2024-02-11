using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace cugonlineWebAPI.Models
{
    [Table("Contacts")]
    public class Contact
    {
        public long ContactId { get; set; }
        [StringLength(100)]
        public string CompanyName { get; set; }
        [StringLength(100)]
        public string CompanyEmail { get; set; }
        public string Message { get; set; }
        public string CellNumber { get; set; }

    }
}