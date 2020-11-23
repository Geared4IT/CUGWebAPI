using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace cugonlineWebAPI.DTO
{
    public class EmailModelDTO
    {
        [Required, Display(Name = "David Allen")]
        public string toname { get; set; }
        [Required, Display(Name = "gearup4it@gmail.com"), EmailAddress]
        public string toemail { get; set; }
        [Required]
        public string subject { get; set; }
        [Required]
        public string message { get; set; }
    }
}