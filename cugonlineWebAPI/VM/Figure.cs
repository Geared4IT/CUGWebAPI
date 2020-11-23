using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cugonlineWebAPI.VM
{
    public class Figure
    {
        public int? LastUpdatedBy { get; set; }

        public int Id { get; set; }
        public string Idx { get; set; }
        public string Title { get; set; }
        public string Meaning { get; set; }
        public string Body { get; set; }
        public string Category { get; set; }
        public string CategoryN { get; set; }
        public int UserId { get; set; }
        public bool IsSuperAdmin { get; set; }
    }
}